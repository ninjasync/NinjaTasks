using System;
using Android.App;
using Android.Content;
using Android.OS;
using Cirrious.CrossCore;
using NinjaTasks.App.Droid.RemoteStorages.NonsenseApps;
using NinjaTasks.App.Droid.Services;
using NinjaTasks.Core.Services;
using NinjaTasks.Core.Services.Server;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTools.Npc;

using Dot42.Manifest;

namespace NinjaTasks.App.Droid.AndroidServices
{
    [Service]
    public class AndroidForegroundSyncMangerService : Service
    {
        private static string _notificationTile, _notificationContext;
        private static int _notificationIconId;
        private static Type _notificationView;
        private static BluetoothSyncServerManager _bluetoothServer;
        private static TcpIpSyncServerManager _tcpIpSyncServer;
        private static ISyncManager _syncManager;
        private static SyncOnDataChangedManager _modifiedManager;
        private static AndroidSyncOnContentProviderChanged _notepadListenerTasks;
        private static AndroidSyncOnContentProviderChanged _notepadListenerLists;

        public override int OnStartCommand(Intent intent, int flags, int startId)
        {
            if (_bluetoothServer == null)
            {
                // we have been started by the system, possibly after our main process
                // has died abnormally.
                // TODO: implement minimal bootstrap for background operation, possibly
                //       even in our own process.

                StopForeground(true);
                return Service.START_NOT_STICKY;
            }

            if (_bluetoothServer.IsRunning)
            {
                var ctx = Mvx.Resolve<Context>();
                var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, _notificationView), 0);

                var ongoing = new Notification.Builder(ctx)
                    .SetContentTitle(_notificationTile)
                    .SetContentText(_notificationContext)
                    .SetContentIntent(pendingIntent)
                    .SetSmallIcon(_notificationIconId)
                    .Notification;

                StartForeground((int)Notification.FLAG_FOREGROUND_SERVICE, ongoing);
            }
            else
            {
                StopForeground(true);
            }

            return Service.START_STICKY;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        private static void Initialize()
        {
            _bluetoothServer = Mvx.Resolve<BluetoothSyncServerManager>();
            _syncManager = Mvx.Resolve<ISyncManager>();
            

            _syncManager.IsEnabled = _bluetoothServer.IsRunning;
#if !DOT42
            _bluetoothServer.Subscribe(x => x.IsRunning, OnRunningChanged);
#else
            _bluetoothServer.Subscribe("IsRunning", OnRunningChanged);
#endif
        }

        private static void OnRunningChanged()
        {
            _syncManager.IsEnabled = _bluetoothServer.IsRunning;

            if (_bluetoothServer.IsRunning)
            {
                var ctx = Mvx.Resolve<Context>();
                ctx.StartService(new Intent(ctx, typeof(AndroidForegroundSyncMangerService)));

                if (_modifiedManager == null)
                    _modifiedManager = Mvx.IocConstruct<SyncOnDataChangedManager>();

                if (_notepadListenerTasks == null)
                    _notepadListenerTasks = new AndroidSyncOnContentProviderChanged(ctx, Mvx.Resolve<IAccountsStorage>(),
                                                    _syncManager, SyncAccountType.NonsenseAppsNotePad, NpContract.UriTask);
                if (_notepadListenerLists == null)
                    _notepadListenerLists = new AndroidSyncOnContentProviderChanged(ctx, Mvx.Resolve<IAccountsStorage>(),
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

                var ctx = Mvx.Resolve<Context>();
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