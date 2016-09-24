using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NinjaSync.Exceptions;
using NinjaSync.Model;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTasks.Model;
using NinjaTools;
using NinjaTools.Progress;
using TaskWarriorLib;
using TaskWarriorLib.Network;
using TaskWarriorLib.Parser;
using Status = TaskWarriorLib.Status;
using TaskWarriorAccount = NinjaTasks.Model.Sync.TaskWarriorAccount;

namespace NinjaTasks.Sync.TaskWarrior
{
    public class RemoteModification : Modification
    {
        public RemoteModification(TrackableId trackableId, DateTime modifiedAt, TaskWarriorTask tw) 
                               : base(trackableId, modifiedAt)
        {
            TwTask = tw;
        }

        public RemoteModification(ITrackable task, TaskWarriorTask tw )
            : base(task)
        {
            TwTask = tw;
        }

        public TaskWarriorTask TwTask { get; private set; }
    }

    /// <summary>
    /// This class requires to be wrapped by a ListMappingTodoRemoteMasterStorageAdapter,
    /// so that is gets 'TodoTaskWithListName' instead of TodoTask.
    /// 
    /// </summary>
    public class TaskWarriorRemoteStorage : ITodoRemoteMasterStorage
    {
        private readonly ITslConnectionFactory _tsl;
        private readonly ISyncStatusStorage _storage;
        private readonly TaskWarriorAccount _account;

        private readonly TaskWarriorTaskParser _parser = new TaskWarriorTaskParser();
        private readonly TaskWarriorToNinjaMapper _mapper;
        private readonly string _accountId;

        public TaskWarriorRemoteStorage(ITslConnectionFactory tsl, ISyncStatusStorage storage,
                                        TaskWarriorAccount account)
        {
            _tsl = tsl;
            _storage = storage;
            _account = account;
            _accountId = "TaskWarrior:" + account.Id;
            _mapper = new TaskWarriorToNinjaMapper();
        }

        public TrackableRemoteStorageType StorageType { get{ return TrackableRemoteStorageType.HasOnlyImplicitLists; } }

        public TrackableType[] SupportedTypes { get { return new[] {TrackableType.List, TrackableType.Task}; } }
        public IList<string> GetMappedColums(TrackableType type)
        {
            if(type == TrackableType.List)
                return new[] { TodoList.ColDescription };
            if(type == TrackableType.Task)
                return new[] {
                    TodoTask.ColId, TodoTask.ColDescription, TodoTask.ColListFk, 
                    TodoTask.ColStatus,TodoTask.ColSortPosition,TodoTask.ColPriority,
                    TodoTask.ColCreatedAt,  TodoTask.ColModifiedAt, TodoTask.ColCompletedAt, 
                };
            return new string[0];
        }

        public virtual CommitList MergeModifications(CommitList localChanges, IProgress progress)
        {
            var pp = new PartitialProgress(progress);
            pp.NextStep(0.8f);
            var ex = ExchangeChanges(localChanges, pp);
            
            pp.NextStep(0.2f);
            UpdateMappingsFromExchangeResult(ex);

            return ex.RemoteChanges;
        }

        private void UpdateMappingsFromExchangeResult(ExchangeResult ex)
        {
            HashSet<string> saved = new HashSet<string>();

            Debug.Assert(ex.RemoteChanges.Commits.Count == 1);
            // save the current status in the database.
            foreach (var mod in ex.RemoteChanges.Commits.First().DeletedAndModified.OfTypeTodoTask())
            {
                TodoTask task = (TodoTask) mod.Object;
                TaskWarriorTask twTask = ((RemoteModification) mod).TwTask;
                SaveRepresentationAndSetId(twTask, task, twTask.JsonOriginalLine);

                saved.Add(twTask.Uuid);
            }

            foreach (var mod in ex.LocalChanges)
            {
                if (saved.Contains(mod.Key)) continue; // already saved, don't overwrite
                TodoTask task = (TodoTask) mod.Value.Item1.Object;
                SaveRepresentationAndSetId(mod.Value.Item2, task, _parser.ToRepr(mod.Value.Item2));
            }
        }

        private void SaveRepresentationAndSetId(TaskWarriorTask twTask, TodoTask task, string twRepr)
        {
            var map = _storage.GetRepresentation(_accountId, twTask.Uuid);
            if (map == null && task == null) return; // nothing to do... 

            if (map == null)
                map = new SyncRemoteRepresentation();

            map.Uuid = twTask.Uuid;
            map.AccountId = _accountId;
            map.Representation = twRepr;

            _storage.SaveRepresentation(map);

            // TODO:check if this makes sense at all...
            if(task != null) // can be null for deleted objects.
                task.Id = map.Uuid;
        }

        public CommitList SaveModificationsForIds(CommitList commits, IProgress progress)
        {
            foreach (var obj in commits.Commits.SelectMany(m => m.Modified)
                                               .Where(o => o.Object.IsNew))
                obj.Object.Id = SequentialGuid.NewGuidString();
            return commits;
        }

        class ExchangeResult
        {
            public readonly CommitList RemoteChanges = new CommitList();
            public Dictionary<string, Tuple<Modification, TaskWarriorTask>> LocalChanges;
        }

        private ExchangeResult ExchangeChanges(CommitList localCommitList, IProgress progress)
        {
            SyncBundle sync = new SyncBundle();

            ExchangeResult ret = new ExchangeResult();
            
            sync.SyncId = localCommitList.RemoteCommitId;

            if (!sync.SyncId.IsNullOrEmpty())
                sync.SyncId = sync.SyncId.Substring(3);


            Commit localChanges = localCommitList.Flatten();
            ret.LocalChanges = localChanges.DeletedAndModified
                                           .Select(l=> new Tuple<Modification, TaskWarriorTask>(l, ModificationToTaskWarrior(l)))
                                           .Where(l=> l.Item2 != null)
                                           .ToDictionary(p=>p.Item2.Uuid);

            sync.ChangedTasks = ret.LocalChanges.Select(p => p.Value.Item2).ToList();

            var twAccount = MapAccount(_account);

            var dx = new TaskWarriorSyncDataExchange(_tsl);

            try
            {
                var remote = dx.ExchangeSyncData(twAccount, sync, progress);

                Commit list = new Commit();

                ret.RemoteChanges.Commits.Add(list);
                // I actually believe, tasks warrior does not need a remoteCommitId.
                // leave this in for sanity checks though.
                ret.RemoteChanges.RemoteCommitId = "tw:" + remote.SyncId;
                list.BasedOnCommitId = localCommitList.RemoteCommitId;
                list.CommitId = "tw:" + remote.SyncId;

                ToModificationList(remote.ChangedTasks, list);

                return ret;

            }
            catch (TaskWarriorTryLaterException ex)
            {
                throw new SyncTryLaterException(TimeSpan.FromSeconds(30), ex);
            }
            catch (TaskWarriorFreshSyncRequiredException ex)
            {
                throw new CommitNotFoundException(ex);
            }
        }

        private void ToModificationList(IList<TaskWarriorTask> remote, Commit ret)
        {
            HashSet<string> alreadyHandled = new HashSet<string>();

            // task warrior sends all modifications to the tasks.
            // we are only interested in the last incarnation of each task.
            foreach (var tw in remote.Reverse())
            {
                if (alreadyHandled.Contains(tw.Uuid))
                    continue;
                alreadyHandled.Add(tw.Uuid);

                var mapping = _storage.GetRepresentation(_accountId, tw.Uuid);

                var twTimestamp = tw.Modified.HasValue ? tw.Modified.Value : DateTime.UtcNow;

                if (tw.Status == Status.deleted)
                {
                    // when there is no local object for this remote object, we have nothing to do.
                    if (mapping == null) continue;

                    ret.Deleted.Add(new RemoteModification(new TrackableId(TrackableType.Task, tw.Uuid), twTimestamp, tw));
                    continue;
                }

                TodoTaskWithListName localTask = new TodoTaskWithListName();

                _mapper.ToNinja(localTask, tw);

                // we don't do tracking of remotely changed fields here [ though maybe we should... ]
                //ModifiedProperties = null, 
                ret.Modified.Add(new RemoteModification(localTask, tw));
            }

        
            // reverse the results again, to get the original
            ret.Modified.Reverse();
            ret.Deleted.Reverse();
        }

        private TaskWarriorTask ModificationToTaskWarrior(Modification mod)
        {
            if (mod.ObjectType != TrackableType.Task)
                return null;

            if (mod.IsDeletion)
            {
                var tw = LoadTaskWarriorTask(mod.Key.ObjectId);
                if (tw == null) return null;

                tw.Status = Status.deleted;
                tw.Modified = _mapper.CheckDate(mod.ModifiedAt, null);
                tw.End = _mapper.CheckDate(mod.ModifiedAt, DateTime.UtcNow);

                return tw;
            }
            else
            {
                // normal modification of task.
                var modTask = (TodoTaskWithListName)mod.Object;
                var modProp = mod.ModifiedProperties;

                var tw = LoadTaskWarriorTask(modTask.Id);
                if (tw == null)
                {
                    tw = new TaskWarriorTask();
                    // set all properties.
                    modProp = null;
                }
                _mapper.FromNinja(tw, modTask, modProp);

                // set uuid if not set.
                if (tw.Uuid.IsNullOrEmpty())
                    tw.Uuid = Guid.NewGuid().ToString();


                return tw;
            }
        }

        private TaskWarriorTask LoadTaskWarriorTask(string uuid)
        {
            var mapping = _storage.GetRepresentation(_accountId, uuid);
            if (mapping == null) return null;
            return _parser.Parse(mapping.Representation);
        }

      private static TaskWarriorLib.TaskWarriorAccount MapAccount(TaskWarriorAccount account)
        {
            var twAccount = new TaskWarriorLib.TaskWarriorAccount();
            twAccount.Key = account.Key;
            twAccount.Org = account.Org;
            twAccount.ServerHostname = account.ServerHostname;
            twAccount.ServerPort = account.ServerPort;
            twAccount.User = account.User;

            twAccount.ClientCertificateAndKeyPfxFile = account.ClientCertificateAndKeyPfxFile;
            twAccount.ClientCertificateAndKeyPem = account.ClientCertificateAndKeyPem;
            twAccount.ServerCertificatePem = account.ServerCertificatePem;
            twAccount.ServerCertificateCrtFile = account.ServerCertificateCrtFile;
            return twAccount;
        }
    }
}
