using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NinjaSync.Exceptions;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTools;
using NinjaTools.Logging;

namespace NinjaSync.P2P
{
    public class P2PSyncService : ISyncService
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IP2PSyncLocalEndpoint _local;
        private readonly IP2PSyncRemoteEndpoint _remote;
        private readonly ISyncStatusStorage _status;
        private readonly string _accountId;

        public P2PSyncService(IP2PSyncLocalEndpoint local, IP2PSyncRemoteEndpoint remote, ISyncStatusStorage status, string accountId)
        {
            _local = local;
            _remote = remote;
            _status = status;
            _accountId = accountId;
        }

        public async Task SyncAsync(ISyncProgress progress)
        {
            await Task.Run(() => Sync(progress));
        }

        public  void Sync(ISyncProgress progress)
        {
            var semaphore = _status.GetAccountSemaphore(_accountId);
            
            if (!semaphore.Wait(0))
            {
                Log.Info("{0}: sync already in progress. bailing out.", _accountId);
                return;
            }

            try
            {
                Log.Info("{0}: starting to sync, aquired sync lock.", _accountId);

                var status = _status.GetStatus(_accountId);
                Debug.Assert(status.LocalCommitId == status.RemoteCommitId);
                string commonAncestor = status.LocalCommitId;

                Log.Debug("{0}: previous common ancestor was '{1}'", _accountId, commonAncestor);

                while (true)
                {
                    try
                    {
                        var myCommits = _local.GetCommitIdsSince(commonAncestor);
                        Log.Debug("{0}: there are {1} new local commits.", _accountId, myCommits.Count);

                        // get remote commits.
                        CommitList remoteCommits = _remote.GetMissingCommits(commonAncestor, _local.StorageId, myCommits);
                        Log.Debug("{0}: retrieved {1}/{2} mod/dels in {3} commits", _accountId, remoteCommits.ModificationCount, remoteCommits.DeletionCount, remoteCommits.Commits.Count);

                        // update remote storage id
                        if(remoteCommits.StorageId.IsNullOrEmpty())
                            throw new ProtocolViolationException("missing StorageId");

                        if (status.RemoteStorageId != remoteCommits.StorageId)
                        {
                            if (!status.RemoteStorageId.IsNullOrEmpty())
                                Log.Warn("remote changed its StorageId from '{0}' to '{1}'", status.RemoteStorageId, remoteCommits.StorageId);

                            status.RemoteStorageId = remoteCommits.StorageId;
                            _status.SaveStatus(status);
                        }

                        // give progress to local, so that he fills in the interesting changed values.
                        CommitList localCommits = _local.MergeCommits(remoteCommits, progress);

                        Log.Debug("{0}: applied {1}/{2} mod/dels", _accountId, progress.RemoteModified, progress.RemoteModified);
                        Log.Debug("{0}: uploading {1}/{2} mod/dels in {3} commits", _accountId, localCommits.ModificationCount, localCommits.DeletionCount, localCommits.Commits.Count);

                        _remote.StoreCommits(localCommits, new NullSyncProgress());

                        status.LastSync = DateTime.UtcNow;
                        status.LocalCommitId = localCommits.FinalCommitId;
                        status.RemoteCommitId = localCommits.FinalCommitId;
                        _status.SaveStatus(status);
                        return;
                    }
                    catch (CommitNotFoundException ex)
                    {
                        if (string.IsNullOrEmpty(commonAncestor))
                            throw;

                        // try again with fresh sync.
                        Log.Warn("remote on {0} could not find commit: {1}. retrying with fresh sync.", _accountId, ex.Message);
                        
                        commonAncestor = null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
