using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.Accounts;
using Android.App;
using Android.Content;
using Android.OS;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.Platform;
using NinjaSync;
using NinjaSync.Exceptions;
using NinjaTasks.App.Droid.Services;
using NinjaTasks.App.Droid.Views;
using NinjaTasks.Core.Services;
using NinjaTasks.Model.Sync;
using NinjaTools;

namespace NinjaTasks.App.Droid.AndroidServices
{
    public class SyncAdapter : AbstractThreadedSyncAdapter
    {
        readonly List<SyncResultSyncProgress> _activeSyncs = new List<SyncResultSyncProgress>();
        private NotificationManager _notifications;
        

        public SyncAdapter(Context context, bool autoInitialize) : base(context, autoInitialize)
        {
            Initialize(context);
        }

        // For Android 3.0 compat
        public SyncAdapter(Context context, bool autoInitialize, bool allowParallelSyncs)
            : base(context, autoInitialize, allowParallelSyncs)
        {
            Initialize(context);
        }

        private void Initialize(Context context)
        {
            // make sure Mvx is initialized. TODO: actually we only need the IoC container to be
            // initialized, so maybe one could optimize this....
            var setupSingleton = MvxAndroidSetupSingleton.EnsureSingletonAvailable(context);
            setupSingleton.EnsureInitialized();
            _notifications = (NotificationManager) context.GetSystemService(Context.NOTIFICATION_SERVICE);
        }

        public override void OnPerformSync(Account account, Bundle extras, string authority, ContentProviderClient provider, SyncResult syncResult)
        {
            int notifyId=Unique.Create();
            var progress = new SyncResultSyncProgress(syncResult);

            lock(_activeSyncs)
                _activeSyncs.Add(progress);
            

            var syncManager = Mvx.Resolve<ISyncManager>();
            try
            {
                SyncAccount syncAccount;
                TaskWarriorAccount twAccount;

                AndroidAccountsStorageService.FromAccount(AccountManager.Get(Context), account, out syncAccount, out twAccount);

                //notifyId = CreateNotification();
                bool isManual = extras.GetBoolean(ContentResolver.SYNC_EXTRAS_MANUAL);
                
                syncManager.SyncNowAsync(syncAccount, progress, isManual).Wait();

                syncResult.Stats.NumDeletes = progress.RemoteDeleted;
                syncResult.Stats.NumUpdates = progress.RemoteModified;
            }
            catch (SyncTryLaterException ex)
            {
                syncResult.DelayUntil = (int)ex.DelayRetry.TotalSeconds;
                syncResult.Stats.NumIoExceptions += 1;
            }
            catch (Exception ex)
            {
                syncResult.Stats.NumIoExceptions += 1;
            }
            finally
            {
                lock(_activeSyncs)
                    _activeSyncs.Remove(progress);
                if(notifyId != 0)
                    _notifications.Cancel(notifyId);
            }
        }

        private int CreateNotification()
        {
            var pendingIntent = PendingIntent.GetActivity(Context, 0, new Intent(Context, typeof (ConfigureAccountsView)), 0);

            var ongoing = new Notification.Builder(Context)
                .SetContentTitle("starting sync.")
                .SetContentText("sync active")
                .SetContentIntent(pendingIntent)
                .SetSmallIcon(R.Drawable.ic_launcher)
                .Notification;

            int id = Unique.Create();
            _notifications.Notify("StartSync", id, ongoing);
            
            return id;
        }

  

        public override void OnSyncCanceled()
        {
            List<SyncResultSyncProgress> syncs;
            lock (_activeSyncs)
                syncs = _activeSyncs.ToList();
            foreach (var s in syncs)
                s.Cancel = true;
            base.OnSyncCanceled();
        }

        public class SyncResultSyncProgress : ISyncProgress
        {
            private readonly SyncResult _syncResult;
            private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

            public SyncResultSyncProgress(SyncResult syncResult)
            {
                _syncResult = syncResult;
            }

            public float Progress  { set { } }

            public string Title
            {
                set  { }
            }

            public bool Cancel { get { return _cancel.IsCancellationRequested; } set { _cancel.Cancel();}}

            public CancellationToken CancelToken { get { return _cancel.Token; } }

            public int LocalDeleted { get; set; }
            public int LocalModified { get; set; }
            public int RemoteDeleted { get; set; }
            public int RemoteModified { get; set; }
        }
    }
}

