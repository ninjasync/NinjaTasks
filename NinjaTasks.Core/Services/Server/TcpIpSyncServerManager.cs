using Cirrious.MvvmCross.Plugins.Messenger;
using NinjaSync.Storage;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Npc;

namespace NinjaTasks.Core.Services.Server
{
    public class TcpIpSyncServerManager : SyncServerManager
    {
        private readonly INinjaTasksConfigurationService _cfg;
        private readonly TokenBag _bag = new TokenBag();

        public TcpIpSyncServerManager(ITcpStreamFactory streamFactory, 
            INinjaTasksConfigurationService cfg, 
            ISyncStoragesFactory storages, 
            IMvxMessenger msg)
            : base(streamFactory, storages, msg)
        {
            _cfg = cfg;
#if !DOT42
            _bag += _cfg.Cfg.SubscribeWeak(p => p.RunTcpIpServer, OnRunStatusChanged);
            _bag += _cfg.Cfg.SubscribeWeak(p => p.TcpIpServerPort, OnTcpIpPortChanged);
#else
            _bag += _cfg.Cfg.SubscribeWeak("RunTcpIpServer", OnRunStatusChanged);
            _bag += _cfg.Cfg.SubscribeWeak("TcpIpServerPort", OnTcpIpPortChanged);
#endif
            ShouldBeActive = _cfg.Cfg.RunTcpIpServer;
        }

        private void OnTcpIpPortChanged()
        {
            // disable, then re-enable.
            ShouldBeActive = false;
            ShouldBeActive = _cfg.Cfg.RunTcpIpServer;
        }

        private void OnRunStatusChanged()
        {
            ShouldBeActive = _cfg.Cfg.RunTcpIpServer;
        }


        protected override RemoteDeviceInfo GetListenAddress()
        {
            var port = _cfg.Cfg.TcpIpServerPort == 0 ? NinjaTasksConfiguration.DefaultTcpIpPort : _cfg.Cfg.TcpIpServerPort;
            var info = new RemoteDeviceInfo(RemoteDeviceInfoType.TcpIp, "NinjaTasks", "")
            {
                Port = port.ToStringInvariant()
            };
            return info;
        }


    }
}