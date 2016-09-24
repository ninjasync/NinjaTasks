using System;
using System.Collections.Generic;
using System.Linq;
using NinjaSync.Journaling;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTools;
using NinjaTools.Progress;

namespace NinjaSync.MasterSlave
{
    public interface ILocalSlaveSyncEndpoint : IModificationAssembler
    {
        /// <summary>
        /// this updates local with all listed modifications.
        /// <para/>
        /// if remote.BasedOnCommitId is null this was a full sync, 
        /// and non-retrieved objects should be deleted.
        /// <para></para> 
        /// </summary>
        void RemoteToLocal(CommitList remote, IUpdateAlgorithm update, IProgress p);

        string CommitChanges();
        void RunInImmediateTransaction(Action a);
        void RunInTransaction(Action a);
    }

    public class LocalSlaveSyncEndpoint : ILocalSlaveSyncEndpoint
    {
        
        protected readonly ITrackableJournalStorage Storage;
        
        private readonly Dictionary<TrackableType,HashSet<string>> _mappedColumns;
        private readonly TrackableType[] _supportedTypes;
        private readonly ModificationAssembler _localTracker;

        public LocalSlaveSyncEndpoint(ITrackableRemoteStorageInfo remote, ITrackableJournalStorage storage)
        {
            _localTracker = new ModificationAssembler(storage);

            _mappedColumns = new Dictionary<TrackableType, HashSet<string>>();

            _supportedTypes = remote.SupportedTypes;

            foreach (var type in _supportedTypes)
            {
                IList<string> mappedColums = remote.GetMappedColums(type);
                if (mappedColums == null) continue;
                var set = new HashSet<string>(mappedColums);
                set.Remove(TrackableProperties.ColId);
                _mappedColumns.Add(type, set);
            }

            Storage = storage;
        }


        /// <summary>
        /// this updates local with all listed modifications.
        /// <para/>
        /// if remote.BasedOnCommitId is null this was a full sync, 
        /// and non-retrieved objects should be deleted.
        /// <para></para> 
        /// </summary>
        public void RemoteToLocal(CommitList remote, IUpdateAlgorithm update, IProgress p)
        {
            var pp = new PartitialProgress(p);
            pp.NextStep(0.8f);

            bool shouldKeepNotExisting = remote.BasedOnCommitId != null;
            List<TrackableId> notDeleted = shouldKeepNotExisting? null: new List<TrackableId>();

            Storage.RunInTransaction(() =>
            {
                update.InitializeUpdate();

                var commit = remote.Flatten();
                
                // UPDATE UPDATES, ordered by TrackableType, to make sure we have
                //                 create all lists before handling new tasks
                //                 possibly depending on them.
                foreach (var mod in commit.Modified.OrderBy(m=>m.Object.TrackableType))
                {
                    ICollection<string> updateColumns = GetUpdatableColumns(mod, update);

                    if (updateColumns != null)
                    {
                        if (CanSaveObject(mod.Object))
                            Storage.Save(mod.Object, updateColumns);
                    }

                    if(notDeleted != null && !mod.Object.IsNew)
                        notDeleted.Add(new TrackableId(mod.Object));
                }

                // DELETE DELETED.
                pp.NextStep(0.2f);
                List<TrackableId> del = commit.Deleted
                                             .Select(d => d.Key)
                                             .ToList();

                if (notDeleted != null)
                {
                    foreach (var type in _supportedTypes)
                    {
                        var keepIds = notDeleted.Where(d => d.Type == type).Select(d => d.ObjectId).ToArray();
                        var delIds = Storage.GetIds(SelectionMode.SelectNotSpecified, type, keepIds);
                        del.AddRange(delIds);
                    }
                }

                var finalDel = update.GetDeletable(del.Distinct()).ToList();
                if (finalDel.Count > 0) 
                    Storage.Delete(SelectionMode.SelectSpecified, finalDel.ToArray());

            });
        }

        /// <summary>
        /// this updates local with all listed modifications.
        /// <para/>
        /// if remote.BasedOnCommitId is null this was a full sync, 
        /// and non-retrieved objects should be deleted.
        /// <para></para> 
        /// </summary>
        public Commit RemoteToLocalCommit(CommitList remote, IUpdateAlgorithm update, IProgress p)
        {
            var pp = new PartitialProgress(p);
            pp.NextStep(0.8f);

            Commit retCommit = new Commit();

            bool shouldKeepNotExisting = remote.BasedOnCommitId != null;
            List<TrackableId> notDeleted = shouldKeepNotExisting ? null : new List<TrackableId>();

            Storage.RunInTransaction(() =>
            {
                update.InitializeUpdate();

                var commit = remote.Flatten();

                // UPDATE UPDATES, ordered by TrackableType, to make sure we have
                //                 create all lists before handling new tasks
                //                 possibly depending on them.
                foreach (var mod in commit.Modified.OrderBy(m => m.Object.TrackableType))
                {
                    ICollection<string> updateColumns = GetUpdatableColumns(mod, update);

                    if (updateColumns != null)
                    {
                        // TODO: implement CanSaveObject based on the previously seen modifications
                        //if (CanSaveObject(mod.Object))
                        retCommit.Modified.Add(new Modification(mod.Object, updateColumns.Select(l=>new ModifiedProperty(l, mod.ModifiedAt)).ToList()));
                    }

                    if (notDeleted != null && !mod.Object.IsNew)
                        notDeleted.Add(new TrackableId(mod.Object));
                }

                // DELETE DELETED.
                pp.NextStep(0.2f);
                List<TrackableId> del =  commit.Deleted
                                             .Select(d => d.Key)
                                             .ToList();

                if (notDeleted != null)
                {
                    foreach (var type in _supportedTypes)
                    {
                        var keepIds = notDeleted.Where(d => d.Type == type).Select(d => d.ObjectId).ToArray();
                        var delIds = Storage.GetIds(SelectionMode.SelectNotSpecified, type, keepIds);
                        del.AddRange(delIds);
                    }
                }

                var finalDel = update.GetDeletable(del.Distinct()).ToList();
                
                retCommit.Deleted = finalDel.Select(d => new Modification(d, default(DateTime))).ToList();

                if (finalDel.Count > 0)
                    Storage.Delete(SelectionMode.SelectSpecified, finalDel.ToArray());

            });

            return retCommit;
        }

        protected virtual bool CanSaveObject(ITrackable local)
        {
            return true;
        }

        public ICollection<string> GetUpdatableColumns(Modification mod, IUpdateAlgorithm update)
        {

            ICollection<string> defaultColumns = _mappedColumns.ContainsKey(mod.ObjectType)
                                                ? _mappedColumns[mod.ObjectType]
                                                : null;

            ICollection<string> updateColumns = defaultColumns;

            if (mod.ModifiedProperties != null && defaultColumns != null)
            {
                updateColumns = defaultColumns.Intersect(mod.ModifiedProperties).ToList();
            }
            else if (mod.ModifiedProperties != null && updateColumns == null)
            {
                updateColumns = new HashSet<string>(mod.ModifiedProperties);
                updateColumns.Remove(TrackableProperties.ColId);
            }
            else if (updateColumns == null)
            {
                updateColumns = new HashSet<string>(mod.Object.Properties);
                updateColumns.Remove(TrackableProperties.ColId);
            }

            // TODO rewrite, so that the old object does'nt have to get loaded.


            TrackableId localKey = null;

            if (!mod.Object.Id.IsNullOrEmpty())
                localKey = new TrackableId(mod.Object);

            updateColumns = update.GetUpdatableColumns(localKey, mod.Object, updateColumns);

            // don't modify if the only modified column is "ModifiedAt".
            if (updateColumns.Count == 0 || (updateColumns.Count == 1 && updateColumns.Contains(TrackableProperties.ColModifiedAt)))
                return null; // DO NOT SAVE.

            return updateColumns;
        }

        public CommitList GetCommits(string sinceButExcludingCommitId)
        {
            return _localTracker.GetCommits(sinceButExcludingCommitId);
        }

        public string CommitChanges()
        {
            return Storage.CommitChanges();
        }

        public void RunInImmediateTransaction(Action a)
        {
            Storage.RunInImmediateTransaction(a);
        }

        public void RunInTransaction(Action a)
        {
            Storage.RunInTransaction(a);
        }
     
    }
}
