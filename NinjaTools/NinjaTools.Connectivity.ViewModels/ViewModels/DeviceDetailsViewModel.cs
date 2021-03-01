using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MvvmCross.Plugin.Messenger;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.ViewModels.Messages;
using NinjaTools.GUI.MVVM;
using NinjaTools.GUI.MVVM.Services;
using NinjaTools.GUI.MVVM.Zools;

namespace NinjaTools.Connectivity.ViewModels.ViewModels
{
    public class DeviceDetailsViewModel : BaseViewModel, IActivate, IDeactivate
    {
        private readonly IWeakTimerService _timer;
        private readonly IDiscoverRemoteEndpoints _factory;
        //private readonly IMeasurementConfigurationService _config;
        private IDisposable _timerToken;
        private readonly IMvxMessenger _messenger;
        private ConnectDisposeMultiuseManager _connectionManager;

        public IBluetoothLeDevice Device { get; set; }

        public string Name { get; private set; }

        public string Address { get; private set; }
        public string Id { get; private set; }

        
        public bool IsConnected { get; private set; }

        public class RowDetails : INotifyPropertyChanged
        {
            public string Detail { get; set; }
            public string Value { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        public ObservableCollection<RowDetails> Details { get; private set; }

        public DeviceDetailsViewModel(IWeakTimerService timer,
                                      IBluetoothLeDeviceFactory factory,
                                      IMvxMessenger config)
        {
            _timer = timer;
            _factory = factory;
            _messenger = config;
            Details = new ObservableCollection<RowDetails>();

            AddToAutoBundling(() => Name);
            AddToAutoBundling(() => Address);
            AddToAutoBundling(() => Id);
        }

        private void UpdateValues()
        {
            if (Device == null && string.IsNullOrEmpty(Address))
            {
                Name = "(no device selected)";
                Address = null;
                Details.Clear();
                return;
            }
            
            if (Device == null)
            {
                Device = _factory.Create(new Endpoint(Name, Address));
                Device.RetrieveDeviceInfos = true;
                _connectionManager = new ConnectDisposeMultiuseManager(Device, Device.Connect, Device.Disconnect);
                _connectionManager.Connect();
            }

            if(!string.IsNullOrEmpty(Device.Name))
                Name = Device.Name;
            if (!string.IsNullOrEmpty(Device.Address))
                Address = Device.Address;
            
            IsConnected = Device.IsConnected;
            Details = new ObservableCollection<RowDetails>(Device.DeviceInfo.Select(d => new RowDetails {Detail = d.Item1, Value = d.Item2}));
        }

        public void Ok()
        {
            Close(this);
        }

        public void Disconnect()
        {
            CloseDevice();

            _messenger.Publish(new RemoteDeviceSelectedMessage(this, Id, null));
            //_config.Cfg.SetDeviceAddress(Type, null);
            Close(this);

        }

        private void OnDeviceChanged()
        {
            UpdateValues();
        }



        public void OnActivate()
        {
            _timerToken = _timer.SubscribeWeak(UpdateValues);
            if(Device != null)
                Device.Connect();
        }
        
        public void OnDeactivate()
        {
            if (_timerToken != null)
                _timerToken.Dispose();
            CloseDevice();
        }

        private void CloseDevice()
        {
            if (Device != null)
            {
                _connectionManager.Disconnect();
                _connectionManager.Dispose();
            }
        }

        public void OnDeactivated(bool destroying)
        {
        }
    }
}
