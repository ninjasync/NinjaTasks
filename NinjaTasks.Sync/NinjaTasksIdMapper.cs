using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTasks.Model;
using NinjaTools;
using NinjaTools.Collections;
using NinjaTools.Logging;
using NinjaTools.Progress;

namespace NinjaTasks.Sync
{
    public class NinjaTasksIdMapper /*: ICommitListMapper*/
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IStringMappingStorage _idmap;
        private readonly bool _doNotMapLists;
        private Dictionary<ITrackable, string> _remoteObjectToLocalId;

        public NinjaTasksIdMapper(IStringMappingStorage idmap, bool doNotMapLists)
        {
            _idmap = idmap;
            _doNotMapLists = doNotMapLists;
        }

        public CommitList TranslateRemoteToLocal(CommitList remoteCommits)
        {
            CommitList localCommits = new CommitList();
            localCommits.RemoteCommitId = remoteCommits.RemoteCommitId;

            var remoteIdToLocalId = new Dictionary<TrackableId, TrackableId>();
            //var remoteObjectNeedsIdFromLocalKey = new Dictionary<TrackableId, ITrackable>();

            foreach (var remoteCommit in remoteCommits.Commits)
            {
                Commit localCommit = new Commit()
                {
                    BasedOnCommitId = remoteCommit.BasedOnCommitId,
                    CommitId = remoteCommit.CommitId,
                };
                localCommits.Commits.Add(localCommit);

                foreach (var del in remoteCommit.Deleted)
                {
                    var remoteKey = del.Key;
                    var localKey = MapRemoteToLocal(remoteKey);
                    if (localKey == null) continue; // if doesn't exist locally, nothing to do.
                    localCommit.Deleted.Add(new Modification(localKey, del.ModifiedAt));
                }

                // do the lists first, to collect their ids.
                foreach (var mod in remoteCommit.Modified.OfTypeTodoList())
                {
                    var modification = RemoteToLocalModification(mod, remoteIdToLocalId);
                    localCommit.Modified.Add(modification);
                }
                // now the other objects, special ListFk-handling.
                foreach (var mod in remoteCommit.Modified)
                {
                    if(mod.ObjectType == TrackableType.List) continue; // lists handled before.
                    
                    var localMod = RemoteToLocalModification(mod, remoteIdToLocalId);
                    if (localMod.ObjectType == TrackableType.Task)
                    {
                        if (!RemoteToLocalMapTaskLiskFk(localMod, remoteIdToLocalId))
                            continue;
                    }

                    localCommit.Modified.Add(localMod);
                }
            }

            return localCommits;
        }

        private bool RemoteToLocalMapTaskLiskFk(Modification localMod, Dictionary<TrackableId, TrackableId> remoteIdToLocalId)
        {
            var localTask = (TodoTask) localMod.Object;
            
            // if no listfk is provided, accept the task.
            // can happen if the rmeote does not support lists.
            // they will be mapped at a later stage.
            if (localTask.ListFk == null) 
                return true;


            var remoteListKey = new TrackableId(TrackableType.List, localTask.ListFk);

            TrackableId localListKey;
            if (!remoteIdToLocalId.TryGetValue(remoteListKey, out localListKey))
                localListKey = MapRemoteToLocal(remoteListKey);

            if (localListKey == null)
            {
                //Debug.Assert(false, "can this happen? handle graceful, and ignore the task?");
                //throw new Exception("unable to map list. was the list provided?");
                Log.Error("unable to map list for local task {0}. was the list provided?. ignoring task.", localTask.Id);
                return false;
            }
                
            localTask.ListFk = localListKey.ObjectId;
            return true;
        }

        private Modification RemoteToLocalModification(Modification mod, Dictionary<TrackableId, TrackableId> remoteIdToLocalId)
        {
            var remoteKey = new TrackableId(mod.Object);
            var localKey = MapRemoteToLocal(remoteKey);

            var localObj = mod.Object.Clone();

            if (localKey != null)
                localObj.Id = localKey.ObjectId;
            else
            {
                // create a new id if we have none yet.
                localObj.Id = SequentialGuid.NewGuidString();

                localKey = new TrackableId(localObj);
                // save the mapping right away.
                SaveMapping(remoteKey, localKey);
                //remoteObjectNeedsIdFromLocalKey[localKey] = mod.Object;
            }
            remoteIdToLocalId[remoteKey] = localKey;
            var modification = new Modification(localObj, mod.ModifiedPropertiesEx);
            return modification;
        }

        public CommitList TranslateLocalToRemote(CommitList localCommits, Func<CommitList,IProgress,CommitList> saveForIds)
        {
            // because of possible inter-commit dependencies, we only support 
            // flattened commits with id mapping

            var flattened = localCommits.Flatten();
            
            CommitList remoteCommits = new CommitList();
            remoteCommits.RemoteCommitId = localCommits.RemoteCommitId;

            _remoteObjectToLocalId = new Dictionary<ITrackable, string>(ReferenceEqualityComparer<ITrackable>.Instance);

            // build a mapped modification list
            Commit remoteCommit = new Commit
            {
                BasedOnCommitId = flattened.BasedOnCommitId,
                CommitId = flattened.CommitId,
                Deleted = flattened.Deleted.Select(MapLocalDeletionToRemote).Where(p => p != null).ToList()
            };
            remoteCommits.Commits.Add(remoteCommit);

            // update lists first, so that all lists exist.
            foreach (var m in flattened.Modified.OfTypeTodoList())
            {
                var remoteKey = MapLocalToRemote(new TrackableId(m.Object));

                TodoList list = (TodoList)m.Object.Clone();
                list.Id = remoteKey != null ? remoteKey.ObjectId : null;

                remoteCommit.Modified.Add(new Modification(list, m.ModifiedPropertiesEx));

                if (remoteKey == null)
                    _remoteObjectToLocalId.Add(list, m.Object.Id);
            }

            // if we need list id, get them now.
            // this means we send all deleted tasks and lists over the wire now,
            // along with all modified lists. we don't send modified tasks yet.
            if (_remoteObjectToLocalId.Count > 0 && !_doNotMapLists)
            {
                remoteCommits = saveForIds(remoteCommits, new NullProgress());
                if (remoteCommits.Commits.Count == 0)
                {
                    // build a mapped modification list
                    remoteCommit = new Commit
                    {
                        BasedOnCommitId = flattened.BasedOnCommitId,
                        CommitId = flattened.CommitId,
                    };
                    remoteCommits.Commits.Add(remoteCommit);
                }
                UpdateIdMappings(_remoteObjectToLocalId);
            }

            // now update tasks.
           foreach (var m in flattened.Modified)
            {
                // we handled the lists before...
                if(m.ObjectType == TrackableType.List) continue;
                    
                var localTask = ((TodoTask)m.Object);

                TodoTask remoteTask = (TodoTask)m.Object.Clone();

                var remoteKey = MapLocalToRemote(new TrackableId(m.Object));
                remoteTask.Id = remoteKey != null ? remoteKey.ObjectId : null;

                var remoteListKey = MapLocalToRemote(new TrackableId(TrackableType.List, localTask.ListFk));
                if (remoteListKey == null)
                {
                    // can this happen? handle graceful any ways.
                    Log.Error("local task {0} has no associated remote list mapping. ignoring.", localTask.Id);
                    continue;
                }

                remoteTask.ListFk = remoteListKey.ObjectId;

                remoteCommit.Modified.Add(new Modification(remoteTask, m.ModifiedPropertiesEx));

                if (remoteKey == null)
                    _remoteObjectToLocalId.Add(remoteTask, m.Object.Id);
            }

            return remoteCommits;
        }

        public void CommitLocalToRemote()
        {
            // TODO: run in transaction, for efficiency reasons.
            UpdateIdMappings(_remoteObjectToLocalId);
            // todo: purge deleted mappings.
        }


        private void UpdateIdMappings(Dictionary<ITrackable, string> remoteObjectToLocalId)
        {
            foreach (var obj in remoteObjectToLocalId)
            {
                // should have been set by SaveModifications.
                Debug.Assert(obj.Key.Id != null);
                if (obj.Key.Id == null)
                {
                    Log.Error("remote save did not provide id: {0} ", new TrackableId(obj.Key.TrackableType, obj.Value));
                    continue;
                }

                SaveMapping(new TrackableId(obj.Key), new TrackableId(obj.Key.TrackableType, obj.Value));
            }
        }

        private Modification MapLocalDeletionToRemote(Modification mod)
        {
            var remoteId = MapLocalToRemote(mod.Key);
            if (remoteId == null)
            {
                Log.Error("local deletion has no corresponding remote object: {0} ", mod.Key);
                return null;
            }
            return new Modification(remoteId, mod.ModifiedAt);
        }

        private TrackableId MapLocalToRemote(TrackableId local)
        {
            // identity mapping?
            if (_idmap == null) return local;

            // force our ids.
            if (local.Type == TrackableType.List && _doNotMapLists)
                return local;

            var map = _idmap.GetMappingFromLocal(local.Type, local.ObjectId);
            if (map == null) return null;

            return new TrackableId(map.ObjectType, map.RemoteId);
        }

        private TrackableId MapRemoteToLocal(TrackableId remote)
        {
            // identity mapping?
            if (_idmap == null) return remote;

            // the mapping will have happened earlier.
            if (remote.Type == TrackableType.List && _doNotMapLists)
                return remote;

            var map = _idmap.GetMappingFromRemote(remote.Type, remote.ObjectId);
            if (map == null) return null;

            return new TrackableId(map.ObjectType, map.LocalId); ;
        }

        private void SaveMapping(TrackableId remoteKey, TrackableId localKey)
        {
            Debug.Assert(remoteKey.Type == localKey.Type);

            // identity mapping?
            if (_idmap == null) return;

            // don't save temporary list-ids.
            if (localKey.Type == TrackableType.List && _doNotMapLists)
                return;

            _idmap.SaveMapping(remoteKey.Type, localKey.ObjectId, remoteKey.ObjectId);
        }

    }
}
