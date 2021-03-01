using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Plugin.Messenger;
using NinjaSync.Storage;
using NinjaTasks.Core.Messages;
using NinjaTasks.Model.Storage;
using NinjaTools.Logging;
using NinjaTools.Threading;

namespace NinjaTasks.Core.Services
{
    /// <summary>
    /// this class will invoke sync's when the database has changed.
    /// </summary>
    public class SyncOnDataChangedManager : IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IAccountsStorage _accounts;
        private readonly ISyncManager _syncManager;

        private readonly DelayedCommandQueue _command = new DelayedCommandQueue();
        private readonly MvxSubscriptionToken _token;
        private readonly Random _rand = new Random();
        private readonly SyncStorages _storages;

        public SyncOnDataChangedManager(IMvxMessenger messenger, IAccountsStorage accounts,
                                        ISyncManager syncManager,ISyncStoragesFactory storages)
        {
            _accounts = accounts;
            _syncManager = syncManager;
            _storages = storages.CreateSyncStorages();
            _token = messenger.Subscribe<TrackableStoreModifiedMessage>(OnDataChanged);
        }

        private void OnDataChanged(TrackableStoreModifiedMessage obj)
        {
            // let changes accumulates over 1 s - 3 s
            int nextDelay = 1000 + _rand.Next(2000);
            _command.Add(SyncUnsynced, TimeSpan.FromMilliseconds(nextDelay), "sync");
        }

        private async void SyncUnsynced()
        {
            var accounts = _accounts.GetAccounts().Where(p => p.IsSyncOnDataChanged).ToList();
            if (accounts.Count == 0) return;

            try
            {
                // run twice to make sure downloaded changes are immediately
                // distributed.
                //for (int run = 0; run < 2; ++run)
                {
                    foreach (var a in accounts)
                    {
                        var status = _storages.Status.GetStatus(a.AccountId);
                        string currentCommitId = _storages.Storage.GetLastCommitId();

                        // don't sync if there no uncommited changes or everything is up to date.
                        if (currentCommitId != null && status.LocalCommitId == currentCommitId)
                            continue;

                        var task = _syncManager.SyncNowAsync(a, isManualSync: false);

                        // wait max 5 secs for sync to complete before starting next sync.
                        const int timeout = 5000;
                        await Task.WhenAny(task, Task.Delay(timeout));
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public void Dispose()
        {
            _token.Dispose();
            _command.Cancel(false);
        }
    }
}
