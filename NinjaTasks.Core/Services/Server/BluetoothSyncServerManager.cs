using Cirrious.MvvmCross.Plugins.Messenger;
using NinjaSync.Storage;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Npc;

namespace NinjaTasks.Core.Services.Server
{
    public class BluetoothSyncServerManager : SyncServerManager
    {
        private readonly INinjaTasksConfigurationService _cfg;
        private readonly TokenBag _bag = new TokenBag();


        public BluetoothSyncServerManager(IBluetoothStreamFactory streamFactory,
            INinjaTasksConfigurationService cfg,
            ISyncStoragesFactory storages,
            IMvxMessenger msg)
            : base(streamFactory, storages, msg)
        {
            _cfg = cfg;
#if !DOT42
            _bag += _cfg.Cfg.SubscribeWeak(p => p.RunBluetoothServer, OnRunStatusChanged);
#else
            _bag += _cfg.Cfg.SubscribeWeak("RunBluetoothServer", OnRunStatusChanged);
#endif

            ShouldBeActive = _cfg.Cfg.RunBluetoothServer;
        }

        private void OnRunStatusChanged()
        {
            ShouldBeActive = _cfg.Cfg.RunBluetoothServer;
        }

        protected override RemoteDeviceInfo GetListenAddress()
        {
            var info = new RemoteDeviceInfo(RemoteDeviceInfoType.Bluetooth, "NinjaTasks", null)
            {
                Port = SqliteSyncServiceFactory.BluetoothGuid.ToString()
            };
            return info;
        }

    }
}
