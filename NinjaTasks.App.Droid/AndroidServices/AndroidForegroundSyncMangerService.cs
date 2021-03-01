using System;
using Android.App;
using Android.Content;
using Android.OS;
using NinjaTasks.App.Droid.RemoteStorages.NonsenseApps;
using NinjaTasks.App.Droid.Services;
using NinjaTasks.Core.Services;
using NinjaTasks.Core.Services.Server;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTools.Npc;
using MvvmCross;
using AndroidX.Core.App;

namespace NinjaTasks.App.Droid.AndroidServices
{
    [Service]
    public class AndroidForegroundSyncMangerService : Service
    {
        private static string _notificationTile, _notificationContext;
        private static int _notificationIconId;
        private static Type _notificationView;
        private static BluetoothSyncServerManager _bluetoothServer;
        //private static TcpIpSyncServerManager _tcpIpSyncServer;
        private static ISyncManager _syncManager;
        private static SyncOnDataChangedManager _modifiedManager;
        private static AndroidSyncOnContentProviderChanged _notepadListenerTasks;
        private static AndroidSyncOnContentProviderChanged _notepadListenerLists;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (_bluetoothServer == null)
            {
                // we have been started by the system, possibly after our main process
                // has died abnormally.
                // TODO: implement minimal bootstrap for background operation, possibly
                //       even in our own process.

                StopForeground(true);
                return StartCommandResult.NotSticky;
            }


            if (_bluetoothServer.IsRunning)
            {
                var ctx = Mvx.IoCProvider.Resolve<Context>();
                var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, _notificationView), 0);

                var ongoing = new NotificationCompat.Builder(ctx, "mychannel")
                    .SetContentTitle(_notificationTile)
                    .SetContentText(_notificationContext)
                    .SetContentIntent(pendingIntent)
                    .SetSmallIcon(_notificationIconId)
                    .Notification;

                StartForeground((int)NotificationFlags.ForegroundService, ongoing);
            }
            else
            {
                StopForeground(true);
            }

            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        private static void Initialize()
        {
            _bluetoothServer = Mvx.IoCProvider.Resolve<BluetoothSyncServerManager>();
            _syncManager = Mvx.IoCProvider.Resolve<ISyncManager>();

            _syncManager.IsEnabled = _bluetoothServer.IsRunning;
            _bluetoothServer.Subscribe(x => x.IsRunning, OnRunningChanged);
        }

        private static void OnRunningChanged()
        {
            _syncManager.IsEnabled = _bluetoothServer.IsRunning;

            if (_bluetoothServer.IsRunning)
            {
                var ctx = Mvx.IoCProvider.Resolve<Context>();
                ctx.StartService(new Intent(ctx, typeof(AndroidForegroundSyncMangerService)));

                if (_modifiedManager == null)
                    _modifiedManager = Mvx.IoCProvider.IoCConstruct<SyncOnDataChangedManager>();

                if (_notepadListenerTasks == null)
                    _notepadListenerTasks = new AndroidSyncOnContentProviderChanged(ctx, Mvx.IoCProvider.Resolve<IAccountsStorage>(),
                                                    _syncManager, SyncAccountType.NonsenseAppsNotePad, NpContract.UriTask);
                if (_notepadListenerLists == null)
                    _notepadListenerLists = new AndroidSyncOnContentProviderChanged(ctx, Mvx.IoCProvider.Resolve<IAccountsStorage>(),
                                                    _syncManager, SyncAccountType.NonsenseAppsNotePad, NpContract.UriTaskList);
            }
            else
            {
                if (_modifiedManager != null)
                    _modifiedManager.Dispose();
                _modifiedManager = null;

                if (_notepadListenerTasks != null)
                    _notepadListenerTasks.Dispose();
                _notepadListenerTasks = null;

                if (_notepadListenerLists != null)
                    _notepadListenerLists.Dispose();
                _notepadListenerLists = null;

                var ctx = Mvx.IoCProvider.Resolve<Context>();
                ctx.StopService(new Intent(ctx, typeof(AndroidForegroundSyncMangerService)));
            }
        }

        public static void Initialize(string notificationTile, string notificationContext, int notificationIcon, Type intentActionView)
        {
            _notificationTile = notificationTile;
            _notificationContext = notificationContext;
            _notificationView = intentActionView;
            _notificationIconId = notificationIcon;
            Initialize();
        }

    }
}