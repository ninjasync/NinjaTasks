using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NinjaSync.Exceptions;
using NinjaSync.Model;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTools;
using NinjaTools.Logging;
using NinjaTools.Progress;

namespace NinjaSync.MasterSlave
{
    /// <summary>
    /// This keeps track of what needs to be synchronized 
    /// between a ITrackableRemoteMasterStorage and a local Storage.
    /// (SyncronizationStatus)
    /// <para></para>
    /// It is agnostic of the type of mirrored data.
    /// </summary>
    public class SyncWithMasterService : ISyncWithMasterService
    {
        private readonly ILogger _log;

        private readonly ILocalSlaveSyncEndpoint _endpoint;
        private readonly ISyncStatusStorage _status;
        private readonly ITrackableRemoteMasterStorage _remote;
        private readonly string _mirrorAccountId;

        public SyncWithMasterService(ITrackableRemoteMasterStorage remote, 
                                     ILocalSlaveSyncEndpoint local, 
                                     ISyncStatusStorage status,
                                     string mirrorAccountId)
        {
            _endpoint = local;
            _status = status;

            _remote = remote;
            _mirrorAccountId = mirrorAccountId;
            _log = LogManager.GetLogger(typeof (SyncWithMasterService).FullName + "." + mirrorAccountId);
        }

        public Task SyncAsync(ISyncProgress progress)
        {
            return Task.Run(() => Sync(progress));
        }

       
        /// <summary>
        /// Might throw a SyncTryLater exception, in which 
        /// case you can try again later.
        /// </summary>
        /// <param name="progress"></param>
        public void Sync(ISyncProgress progress)
        {
            _log.Debug("starting sync.");

            var pp = new PartitialProgress(progress);
            pp.NextStep(0.7f);
            
            SyncStatus syncStatus = _status.GetStatus(_mirrorAccountId);
            _log.Debug("sync status: " + syncStatus);

            int retryCount = 0;

            CommitList localCommits, remoteCommits;
            string protectChangesSinceCommitId;
            
            while (true)
            {
                try
                {
                    int localModified;
                    int localDeleted;

                    localCommits = _endpoint.GetCommits(syncStatus.LocalCommitId);
                    protectChangesSinceCommitId = localCommits.FinalCommitId;

                    // don't upload commits comming from the remote itself
                    localCommits.Commits.RemoveAll(c => c.CommitId == syncStatus.RemoteExcludeLocalCommitId);

                    localCommits.RemoteCommitId = syncStatus.RemoteCommitId;

                    localModified = localCommits.ModificationCount;localDeleted = localCommits.DeletionCount;
                    progress.LocalModified += localModified;progress.LocalDeleted += localDeleted;

                    // upload changes.

                    // no need and no want to put this into a transaction.
                    // no need, since all of the called methods have to assure
                    // data integrity,
                    // no want, since the remote network access might take some
                    // time, and we don't want to block the database for an
                    // extended amount of time.
                    _log.Debug(
                        "uploading changes: from local/remote commit id: {0}/{1} to {2}; {3}/{4} modifications/deletions",
                        localCommits.BasedOnCommitId, localCommits.RemoteCommitId, localCommits.FinalCommitId, localDeleted, localModified);

                
                    remoteCommits = _remote.MergeModifications(localCommits, pp);
                }
                catch (CommitNotFoundException ex)
                {
                    _log.Error("CouldNotFindCommonAncestor: {0}", ex.Message);

                    ++retryCount;

                    if (retryCount > 2) 
                        throw; // prevent an endless loop.

                    syncStatus.LocalCommitId = null;
                    syncStatus.RemoteCommitId = null;
                    syncStatus.RemoteExcludeLocalCommitId = null;

                    continue;
                }

                break;
            }

            pp.NextStep(0.1f);
            FireRemoteModificationsRetrieved();

            pp.NextStep(0.2f);

            var merger = new ProtectIntermediateChangesUpdateAlgorithm(_endpoint,
                                                                       protectChangesSinceCommitId);

            // if this is a fresh sync, we don't want any of our uploaded 
            // changes to be implicit deleted by the LocalSlaveSyncEndpoint.
            if (remoteCommits.BasedOnCommitId.IsNullOrEmpty())
            {
                localCommits = _endpoint.GetCommits(syncStatus.LocalCommitId);
                var protectedFromDeletion = localCommits.Commits.SelectMany(p => p.Modified).Select(p => p.Key)
                                   .Except(remoteCommits.Commits.SelectMany(p => p.Deleted).Select(p => p.Key));
                
                merger.ProtectFromDeletion(protectedFromDeletion);
            }


            MergeRemoteChanges(remoteCommits, syncStatus, merger, new RedirectingSyncProgress(pp, progress));


            progress.Progress = 1;
            _log.Debug("done. new sync status: " + syncStatus);
        }

        /// <summary>
        /// Merges all remote changes from "remoteSyncIdFrom" carefully not overwriting 
        /// changes from localSyncIdUntil until now.
        /// Updates the sync status in the database.
        /// </summary>
        private void MergeRemoteChanges(CommitList remoteCommits, SyncStatus syncStatus,
                                        ProtectIntermediateChangesUpdateAlgorithm merger, ISyncProgress progress)
        {
            PartitialProgress p = new PartitialProgress(progress);

            p.NextStep(0.5f);
            Debug.Assert(remoteCommits != null);

            // update statistics...
            int remoteDeleted = remoteCommits.DeletionCount;
            int remoteModified = remoteCommits.ModificationCount;


            p.NextStep(0.5f);

            bool hasRemoteUpdate = !remoteCommits.IsDummy && !remoteCommits.IsEmpty;
            string remoteCommitsCommitId = null;

            Action updateProc = () =>
            {
                _log.Debug("merging remote changes:  remote commit id: {0} to {1}/{2}; {3}/{4} modifications/deletions", 
                            remoteCommits.BasedOnCommitId, remoteCommits.FinalCommitId, remoteCommits.RemoteCommitId, remoteModified, remoteDeleted);


                string beforeApplyRemoteCommitId = _endpoint.CommitChanges();
                
                if (hasRemoteUpdate)
                {
                    _endpoint.RemoteToLocal(remoteCommits, merger, p);

                    string afterApplyRemoteCommitId = _endpoint.CommitChanges();
                    if (afterApplyRemoteCommitId != beforeApplyRemoteCommitId)
                    {
                        remoteCommitsCommitId = afterApplyRemoteCommitId;

                        progress.RemoteDeleted += remoteDeleted;progress.RemoteModified += remoteModified;
                    }
                }
               
                // todo: cleanup journal storage.

                // store data about last successfull commit.
                
                syncStatus.LocalCommitId = merger.ProtectChangesSinceButExcludingCommitId;
                syncStatus.RemoteCommitId = remoteCommits.RemoteCommitId;
                syncStatus.RemoteExcludeLocalCommitId = remoteCommitsCommitId;
                syncStatus.LastSync = DateTime.UtcNow;

                _status.SaveStatus(syncStatus);
            };

           // make sure this runs in a transaction, so that either all 
           // changes are merged in, or none.
           // disallow any changes by others immediately,
           // so that we can truely protect all changes since recommitChangeSinceButExcludingCommitId
            if (hasRemoteUpdate)
                _endpoint.RunInImmediateTransaction(updateProc);
            else
                _endpoint.RunInTransaction(updateProc);

        }

        public event EventHandler RemoteModificationsRetrieved;

        private void FireRemoteModificationsRetrieved()
        {
            var handler = RemoteModificationsRetrieved;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
