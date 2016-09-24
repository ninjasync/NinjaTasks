using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using NinjaTasks.Core.Messages;
using NinjaTasks.Core.Services;
using NinjaTasks.Core.Services.Server;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTools;
using NinjaTools.Connectivity.ViewModels.Messages;
using NinjaTools.Connectivity.ViewModels.ViewModels;
using NinjaTools.MVVM;
using NinjaTools.Npc;

namespace NinjaTasks.Core.ViewModels.Sync
{
    public class SyncServerViewModel : MvxNotifyPropertyChanged // inherit for automatic UI thread marshalling
    {
        public bool ShouldRun { get; set; }

        public string LastError { get; private set; }
        public bool IsSyncActive { get; private set; }
        
        public bool IsAvailable { get; private set; }
        public bool IsAvailableOnDevice { get; private set; }
    }

    public class ConfigureAccountsViewModel : BaseViewModel, IActivate
    {
        private readonly IAccountsStorage _accounts;
        private readonly ISyncManager _syncManager;
        private readonly IMvxMessenger _messenger;
        public INinjaTasksConfigurationService Config { get; private set; }
        private readonly TokenBag _bag = new TokenBag();

        public SyncServerViewModel BluetoothServer { get; private set; }
        public SyncServerViewModel TcpIpServer { get; private set; }

        public ObservableCollection<SyncAccountViewModel> Accounts { get; private set; }

        public ConfigureAccountsViewModel(IAccountsStorage accounts, 
                                          ISyncManager syncManager, 
                                          IMvxMessenger messenger,
                                          INinjaTasksConfigurationService config,
                                          BluetoothSyncServerManager bluetoothServer,
                                          TcpIpSyncServerManager tcpIpServer)
        {
            _accounts = accounts;
            _syncManager = syncManager;
            _messenger = messenger;
            Config = config;

            Accounts = new ObservableCollection<SyncAccountViewModel>();

            _bag += messenger.SubscribeOnMainThread<RemoteDeviceSelectedMessage>(OnRemoteDeviceSelected);
            _bag += messenger.SubscribeOnMainThread<SyncFinishedMessage>(OnSyncMessage);

            BluetoothServer= new SyncServerViewModel();
            TcpIpServer = new SyncServerViewModel();

#if !DOT42
            if (bluetoothServer != null)
            {
                _bag += config.Cfg.TwoWayBindWeak(p => p.RunBluetoothServer, BluetoothServer, p => p.ShouldRun);
                _bag += bluetoothServer.BindToWeak(p => p.LastError, BluetoothServer, l => l.LastError);
                _bag += bluetoothServer.BindToWeak(p => p.IsSyncActive, BluetoothServer, l => l.IsSyncActive);
                _bag += bluetoothServer.BindToWeak(p => p.IsAvailableOnDevice, BluetoothServer, l => l.IsAvailableOnDevice);
            }

            if (tcpIpServer != null)
            {
                _bag += config.Cfg.TwoWayBindWeak(p => p.RunTcpIpServer, TcpIpServer, p => p.ShouldRun);
                _bag += tcpIpServer.BindToWeak(p => p.LastError, TcpIpServer, l => l.LastError);
                _bag += tcpIpServer.BindToWeak(p => p.IsSyncActive, TcpIpServer, l => l.IsSyncActive);
                _bag += bluetoothServer.BindToWeak(p => p.IsAvailableOnDevice, TcpIpServer, l => l.IsAvailableOnDevice);
            }
#else
            if (bluetoothServer != null)
            {
                _bag += config.Cfg.TwoWayBindWeak("RunBluetoothServer", BluetoothServer, "ShouldRun");
                _bag += bluetoothServer.BindToWeak("LastError", BluetoothServer, "LastError");
                _bag += bluetoothServer.BindToWeak("IsSyncActive", BluetoothServer, "IsSyncActive");
                _bag += bluetoothServer.BindToWeak("IsAvailableOnDevice", BluetoothServer);
            }

            if (tcpIpServer != null)
            {
                _bag += config.Cfg.TwoWayBindWeak("RunTcpIpServer", TcpIpServer, "ShouldRun");
                _bag += tcpIpServer.BindToWeak("LastError", TcpIpServer, "LastError");
                _bag += tcpIpServer.BindToWeak("IsSyncActive", TcpIpServer, "IsSyncActive");
                _bag += bluetoothServer.BindToWeak("IsAvailableOnDevice", TcpIpServer);
                _bag += bluetoothServer.BindToWeak("IsAvailable", TcpIpServer);
            }

#endif
        }

        private void OnRemoteDeviceSelected(RemoteDeviceSelectedMessage obj)
        {
            // not so pretty, but makes sure the 
            // account is created we reload our accounts.
            Mvx.Resolve<AccountCreationManager>().OnDeviceSelected(obj);
            UpdateAccounts();
        }

        public void SyncAll()
        {
            _syncManager.SyncNowAsync();
        }

        private void OnSyncMessage(SyncFinishedMessage obj)
        {
            UpdateAccounts();
        }

        private void UpdateAccounts()
        {
            var accounts = _accounts.GetAccounts()
                                    .OrderBy(p => p.Name)
                                    .ToList();

            bool hadBluetooth = Accounts.Any(a => a.Account.Type == SyncAccountType.BluetoothP2P);
            bool hasBluetooth = accounts.Any(a => a.Type == SyncAccountType.BluetoothP2P);

            bool hadTcpIp = Accounts.Any(a => a.Account.Type == SyncAccountType.TcpIpP2P);
            bool hasTcpIp = accounts.Any(a => a.Type == SyncAccountType.TcpIpP2P);

            Accounts = new ObservableCollection<SyncAccountViewModel>(accounts.Select(a => 
                new SyncAccountViewModel(a, _accounts, _syncManager, _messenger)));

            if (hasBluetooth && !hadBluetooth)
                BluetoothServer.ShouldRun = true;
            if (!hasBluetooth && hadBluetooth)
                BluetoothServer.ShouldRun = false;
            if (hasTcpIp && !hadTcpIp)
                TcpIpServer.ShouldRun = true;
            if (!hasTcpIp && hadTcpIp)
                TcpIpServer.ShouldRun = false;

            foreach (var a in Accounts)
                a.IsSyncActive = _syncManager.IsSyncActive(a.Account);
        }

        public void AddBluetooth()
        {
            ShowViewModel<SelectRemoteDeviceViewModel>();
        }

        public void AddTcpIp()
        {
            ShowViewModel<SelectTcpIpHostViewModel>(new {Port=NinjaTasksConfiguration.DefaultTcpIpPort});
        }

        public void EditTaskWarrior()
        {
            ShowViewModel<TaskWarriorAccountViewModel>();
        }

        public void OnActivate()
        {
            UpdateAccounts();
        }
    }



   
}
