using System;
using System.Linq;
using System.Threading.Tasks;
using NinjaSync.Exceptions;
using NinjaSync.Journaling;
using NinjaSync.Model;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTools;
using NinjaTools.Progress;

namespace NinjaSync.MasterSlave
{
    /// <summary>
    /// TODO: bring the code into an understandable form.
    /// </summary>
    public class SyncBetweenSlaveAndMasterService : ISyncWithMasterService
    {
        //private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ITrackableRemoteMasterStorage _master;
        private readonly ITrackableRemoteSlaveStorage _slave;
        private readonly SyncStorages _storages;
        private readonly string _slaveMirrorAccountId;
        private readonly string _masterAccountId;
        private readonly bool _invertRemoteLocalMeaningInProgress;
        private readonly LocalSlaveSyncEndpoint _slaveEndpoint;
        private readonly ModificationAssembler _tracker;

        public SyncBetweenSlaveAndMasterService(ITrackableRemoteMasterStorage master, 
                                         ITrackableRemoteSlaveStorage slave,
                                         SyncStorages intermediate, 
                                         string slaveMirrorAccountId,
                                         string masterAccountId,
                                         bool invertRemoteLocalMeaningInProgress)
        {
            _master = master;
            _slave = slave;
            _storages = intermediate;
            _slaveMirrorAccountId = slaveMirrorAccountId;
            _masterAccountId = masterAccountId;
            _invertRemoteLocalMeaningInProgress = invertRemoteLocalMeaningInProgress;
            _tracker = new ModificationAssembler(_storages.Storage);

            _slaveEndpoint = new LocalSlaveSyncEndpoint(slave, intermediate.Storage);
            
        }

        public void Sync(ISyncProgress progress)
        {
            PartitialProgress pp = new PartitialProgress(progress);

            var status = _storages.Status.GetStatus(_slaveMirrorAccountId);
           
            EventHandler handler = (sender, args) =>
            {
                FireRemoteModificationsRetrieved();

                // (3) just happened.

                // (4) Drop again, possibly recording intermediate changes.
                //     Don't handle CommitNotFound exceptions here, as
                //     we can't modify the database while the SyncWithMaster is in the 
                //     middle of something.
                CommitList slaveCommits = _slave.GetModifications(status.RemoteCommitId, new NullProgress());
                status.RemoteCommitId = slaveCommits.RemoteCommitId;
                _slaveEndpoint.RemoteToLocal(slaveCommits, new DefaultUpdateAlgorithm(), new NullProgress());
                  
                // (5) will happen next. (6) as well.
            };

            Action<Commit> saveCommit = commit =>
            {
                var commitList = new CommitList(commit);
                commitList.RemoteCommitId = status.RemoteCommitId;

                // send to slave
                var actualCommit = _slave.SaveModifications(commitList, new NullProgress());

                // apply the commit
                foreach(var mod in actualCommit.Commits.SelectMany(p=>p.Modified))
                    _storages.Storage.Save(mod.Object, mod.ModifiedProperties);
                _storages.Storage.Delete(SelectionMode.SelectSpecified, actualCommit.Commits.SelectMany(c=>c.Deleted).Select(p=>p.Key).ToArray());
            };

            var masterEndpoint = new LocalSlaveSyncEndpoint(_master, _storages.Storage);
            var myEndpoint = new MyLocalSyncEndpoint(_tracker, masterEndpoint, _storages, saveCommit);
            var masterSync = new SyncWithMasterService(_master, myEndpoint, _storages.Status, _masterAccountId);

            try
            {
                // (1) Drop everything into intermediate DB, enabling change tracking 
                pp.NextStep(0.3f);
                var slaveCommits = GetSlaveCommitsAndHandleCommitNotFound(status, new NullProgress());
                pp.NextStep(0.1f);
                _slaveEndpoint.RemoteToLocal(slaveCommits, new DefaultUpdateAlgorithm(), new NullProgress());

                masterSync.RemoteModificationsRetrieved += handler;

                // (2) Send versioned changes
                pp.NextStep(0.4f);
                var masterProgress = new WrappingSyncProgress(pp);

                masterSync.Sync(masterProgress);

                FillProgress(progress, masterProgress);
            }
           
            finally
            {
                masterSync.RemoteModificationsRetrieved -= handler;
            }

            _storages.Status.SaveStatus(status);

        }

        private void FillProgress(ISyncProgress progress, WrappingSyncProgress masterProgress)
        {
            // inverted remote/local meaning if requested
            if (_invertRemoteLocalMeaningInProgress)
            {
                progress.RemoteModified = masterProgress.LocalModified;
                progress.RemoteDeleted = masterProgress.LocalDeleted;
                progress.LocalModified = masterProgress.RemoteModified;
                progress.LocalDeleted = masterProgress.RemoteDeleted;
            }
            else
            {
                progress.RemoteModified = masterProgress.RemoteModified;
                progress.RemoteDeleted = masterProgress.RemoteDeleted;
                progress.LocalModified = masterProgress.LocalModified;
                progress.LocalDeleted = masterProgress.LocalDeleted;
            }
        }

        private CommitList GetSlaveCommitsAndHandleCommitNotFound(SyncStatus status, IProgress p)
        {
            int retries = 0;
            while (true)
            {
                try
                {
                    CommitList slaveCommits = _slave.GetModifications(status.RemoteCommitId, p);
                    status.RemoteCommitId = slaveCommits.RemoteCommitId;
                    return slaveCommits;
                }
                catch (CommitNotFoundException)
                {
                    if (retries != 0) // only retry once.
                        throw;

                    ++retries;

                    // clear local database. reset sync status
                    status.RemoteCommitId = null;
                    status.LocalCommitId = null;
                    status.RemoteExcludeLocalCommitId = null;

                    _storages.Storage.Clear();
                }
            }
        }

      
        public Task SyncAsync(ISyncProgress progress)
        {
            return Task.Run(() => Sync(progress));
        }

        private class MyLocalSyncEndpoint : ILocalSlaveSyncEndpoint
        {
            private readonly IModificationAssembler _tracker;
            private readonly LocalSlaveSyncEndpoint _masterEndpoint; 

            private readonly SyncStorages _storages;
            private readonly Action<Commit> _saveModifications;

            private readonly Guard _guard = new Guard();
            

            public MyLocalSyncEndpoint(ModificationAssembler tracker, 
                                       LocalSlaveSyncEndpoint masterEndpoint, 
                                       SyncStorages storages,
                                       Action<Commit> saveModifications)
            {
                _tracker = tracker;
                _masterEndpoint = masterEndpoint;
                _storages = storages;
                _saveModifications = saveModifications;
            }

            public void RemoteToLocal(CommitList remote, IUpdateAlgorithm update, IProgress p)
            {
                // (3) and (4) are complete.

                // (5) Subtract intermediate changes.
                var localCommit = _masterEndpoint.RemoteToLocalCommit(remote, update, p);

                // (6) save changs.
                _saveModifications(localCommit);

                // when everything went well, save updated synchronization status.
                // status.LocalCommitId is not needed. 
            }

            public CommitList GetCommits(string sinceButExcludingCommitId)
            {
                return _tracker.GetCommits(sinceButExcludingCommitId);
            }

            public string CommitChanges()
            {
                return _storages.Storage.CommitChanges();
            }

            public void RunInImmediateTransaction(Action a)
            {
                using (_guard.Use())
                    _storages.Storage.RunInImmediateTransaction(a);
            }

            public void RunInTransaction(Action a)
            {
                using (_guard.Use())
                    _storages.Storage.RunInTransaction(a);
            }

           
        }

        public event EventHandler RemoteModificationsRetrieved;

        private void FireRemoteModificationsRetrieved()
        {
            EventHandler handler = RemoteModificationsRetrieved;
            if (handler != null) handler(this, EventArgs.Empty);
        }

    }

   
}
