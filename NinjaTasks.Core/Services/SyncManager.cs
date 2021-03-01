using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Plugin.Messenger;
using NinjaSync;
using NinjaTasks.Core.Messages;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTools;
using NinjaTools.Logging;

namespace NinjaTasks.Core.Services
{
    /// <summary>
    /// schedules regular syncs.
    /// </summary>
    public class SyncManager : ISyncManager, IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IAccountsStorage _storage;
        private readonly ISyncServiceFactory _factory;
        private readonly IMvxMessenger _messenger;
        private bool _isEnabled;
        private readonly HashSet<int> _runningSyncs = new HashSet<int>();
        private CancellationTokenSource _cancelWorker = new CancellationTokenSource();
        private int _activeSyncs;

        public int ActiveSyncs { get { return _activeSyncs; } }

        public SyncManager(IAccountsStorage storage, ISyncServiceFactory factory, IMvxMessenger messenger)
        {
            _storage = storage;
            _factory = factory;
            _messenger = messenger;
        }

        public void Dispose()
        {
            IsEnabled = false;
        }

        public bool IsEnabled { get { return _isEnabled; } set { SetEnabled(value); } }

        public async Task SyncNowAsync(CancellationToken token = default(CancellationToken))
        {
            var accounts = _storage.GetAccounts().ToList();
            await Task.WhenAll(accounts.Select(a=>SyncNowAsync(a, true, new NullSyncProgress(token))));

            _messenger.Publish(new SyncFinishedMessage(this, null)
            {
                IsSyncActive = ActiveSyncs > 0,
                TotalSyncActive = ActiveSyncs,
                IsManualSync = true,
            });
        }

        public void RefreshAccounts()
        {
            if (!_isEnabled) return;
            IsEnabled = false;
            IsEnabled = true; // restart the background worker.
        }

        private List<SyncAccount> GetAutoSyncAccountsByNextSync()
        {
            return _storage.GetAccounts().ToList()
                           .Where(p=>!p.IsManualSyncOnly && !IsSyncActive(p) && p.SyncInterval != default(TimeSpan))
                           .OrderBy(GetNextSync)
                           .ToList();
        }

        public bool IsSyncActive(SyncAccount syncAccount)
        {
            lock (_runningSyncs)
                return _runningSyncs.Contains(syncAccount.Id);
        }

        public DateTime GetNextSync(SyncAccount account)
        {
            if (account.SyncInterval == TimeSpan.Zero) return DateTime.MaxValue;
            // hat jemand an der Uhr gedreht?
            if (account.LastSyncAttempt > DateTime.UtcNow + TimeSpan.FromSeconds(1))
                return DateTime.MinValue;
            return account.LastSyncAttempt + TimeSpan.FromSeconds(account.SyncInterval.TotalSeconds 
                                                                * Math.Min(account.SyncFailureCount+1, 4));
        }

        public Task SyncNowAsync(SyncAccount account, CancellationToken cancel = new CancellationToken(), bool isManualSync = true)
        {
            return SyncNowAsync(account, isManualSync, new NullSyncProgress(cancel));
        }
        public Task SyncNowAsync(SyncAccount account, ISyncProgress progress, bool isManualSync)
        {
            return SyncNowAsync(account, isManualSync, progress);
        }

        private async Task SyncNowAsync(SyncAccount account, bool isManualSync, ISyncProgress progress)
        {
            ISyncService sync=null;
            string syncErrorMsg = null;

            account.LastSyncAttempt = DateTime.UtcNow;
            
            _storage.SaveAccount(account, nameof(account.LastSyncAttempt));

            lock (_runningSyncs)
            {
                if (_runningSyncs.Contains(account.Id))
                {
                    //Log.Info("not syncing {0}: already active.", account.AccountId);
                    return;
                }
                _runningSyncs.Add(account.Id);
            }

            try
            {
                sync = _factory.Create(account);

                Interlocked.Increment(ref _activeSyncs);
                
                _messenger.Publish(new SyncFinishedMessage(this, account)
                {
                    IsSyncActive = true,
                    TotalSyncActive = ActiveSyncs,
                    IsManualSync = isManualSync
                });

                Log.Info("starting to sync {0}.", account.AccountId);
                await sync.SyncAsync(progress);

                SetAccountError(account, null);
            }
            catch (OperationCanceledException ex)
            {
                Log.Info("sync {0}: {1}", account.Id, ex.Message);
            }
            catch (AggregateException ae)
            {
                StringBuilder msg = new StringBuilder();
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    msg.AppendLine(ex.Message);
                    Log.Error(ex);
                }

                syncErrorMsg = msg.ToString();
                SetAccountError(account, syncErrorMsg);
            }
            catch (Exception ex)
            {
                syncErrorMsg = ex.Message;
                SetAccountError(account, syncErrorMsg);
                Log.Error(ex);
            }
            finally
            {
                Log.Info("finally for sync: {0}. RemoteDeleted: {1}. RemoteModified: {2}", 
                         account.AccountId, progress.RemoteDeleted, progress.RemoteModified);

                lock (_runningSyncs)
                    _runningSyncs.Remove(account.Id);

                if (sync != null)
                {
                    Interlocked.Decrement(ref _activeSyncs);
                    _factory.Destroy(sync);
                }

                if(progress.RemoteDeleted > 0 || progress.RemoteModified > 0)
                    _messenger.Publish(new TrackableStoreModifiedMessage(this, ModificationSource.Sync) { Account = account});

                _messenger.Publish(new SyncFinishedMessage(this, account)
                {
                    IsSyncActive = false, 
                    TotalSyncActive = ActiveSyncs,
                    IsManualSync = isManualSync,
                    SyncError = syncErrorMsg,
                    UploadedChanges = progress.LocalDeleted + progress.LocalModified,
                    DownloadedChanges = progress.RemoteDeleted + progress.RemoteModified,
                });
            }
        }

        private void SetAccountError(SyncAccount account, string msg)
        {
            if(msg.IsNullOrEmpty())
                Log.Debug("sync account {0} finished.", account.AccountId);
            else
                Log.Error("sync account {0}: finished with error: {1}", account.AccountId, msg);

            if (msg == null)
            {
                account.SyncFailureCount = 0;
                account.LastSyncError = "";
                account.LastSuccessfulSync = DateTime.UtcNow;
            }
            else
            {
                account.SyncFailureCount += 1;
                account.LastSyncError = msg;
            }
            _storage.SaveAccount(account, nameof(account.SyncFailureCount), 
                                          nameof(account.LastSyncError), 
                                          nameof(account.LastSuccessfulSync));
        }

        private async void SetEnabled(bool value)
        {
            _isEnabled = value;

            // wait before starting up.
            await Task.Delay(2000);

            if(!_isEnabled) _cancelWorker.Cancel();
            else
            {
                _cancelWorker = new CancellationTokenSource();
                RunSyncs(_cancelWorker.Token);
            }
        }

        private async Task SyncDueTasks(CancellationToken token)
        {
            DateTime now = DateTime.UtcNow;
            var accounts = GetAutoSyncAccountsByNextSync();

            if (accounts.Count == 0) return;

            List<Task> syncs = accounts.TakeWhile(a => GetNextSync(a) <= now)
                                       .Select(account => SyncNowAsync(account, false, new NullSyncProgress(token)))
                                       .ToList();

            await Task.WhenAll(syncs);
        }

        private async void RunSyncs(CancellationToken cancel)
        {

            try
            {
                while (true)
                {
                    // don't wait for finish, as we don't want one blocking sync 
                    // to block all others as well
                    #pragma warning disable CS4014
                    SyncDueTasks(cancel);
                    #pragma warning restore CS4014

                    await Task.Delay(TimeSpan.FromSeconds(20), cancel);
                    if (cancel.IsCancellationRequested) return;

                    var now = DateTime.UtcNow;
                    DateTime next;

                    var nextDue = GetAutoSyncAccountsByNextSync().FirstOrDefault();
                    if (nextDue == null || (next = GetNextSync(nextDue)) == DateTime.MaxValue)
                        await Task.Delay(TimeSpan.FromMinutes(2), cancel);
                    else
                    {
                        if (next <= now)
                            await Task.Delay(TimeSpan.FromSeconds(5), cancel);
                        else
                            await Task.Delay(next - now, cancel);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
           
        }

        #pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore CS0067
    }
}
