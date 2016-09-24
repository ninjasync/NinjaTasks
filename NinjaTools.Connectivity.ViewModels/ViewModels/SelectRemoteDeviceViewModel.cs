using System;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.MvvmCross.Plugins.Messenger;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.ViewModels.Messages;
using NinjaTools.MVVM;
using NinjaTools.MVVM.Services;

namespace NinjaTools.Connectivity.ViewModels.ViewModels
{
    /// <summary>
    /// This class allows to select discoverable devices.
    /// </summary>
    public class SelectRemoteDeviceViewModel : BaseViewModel, IActivate, IDeactivate
    {
        private readonly IDiscoverRemoteDevices _service;
        private IDisposable _token;
        private IScanContext _scanContext;
        private readonly IWeakTimerService _timer;
        private readonly IMvxMessenger _messenger;

        public bool IsScanning { get; private set; }

        public string Id { get; set; }

        public ObservableCollection<RemoteDeviceViewModel> Devices { get; private set; }
        public RemoteDeviceViewModel SelectedDevice { get; set; }

        public SelectRemoteDeviceViewModel(IDiscoverRemoteDevices service, 
                                           IWeakTimerService timer,
                                           IMvxMessenger messenger)
        {
            _service = service;
            _timer = timer;
            _messenger = messenger;
            Devices = new ObservableCollection<RemoteDeviceViewModel>();
#if !DOT42
            AddToAutoBundling(()=>Id);
#else
            AddToAutoBundling("Id");
#endif
        }

        public void Select()
        {
            if (SelectedDevice == null) return;

            var rdi = new RemoteDeviceInfo(RemoteDeviceInfoType.Bluetooth, SelectedDevice.Name, SelectedDevice.Address);
            _messenger.Publish(new RemoteDeviceSelectedMessage(this, Id, rdi));
            //ShowViewModel<DeviceDetailsViewModel>(new BluetoothLeDeviceViewModelParameters { Address = SelectedDevice.Address, Id=Id, Name=SelectedDevice.Name });

            Close(this);
        }

        private void OnUpdateScanningState()
        {
            IsScanning = _scanContext != null && _scanContext.IsScanning;
        }

        public void OnActivate()
        {
            if(_token == null)
                _token = _timer.SubscribeWeak(OnUpdateScanningState);

            Refresh();

        }

        public void OnDeactivate()
        {
            if (_scanContext != null)
            {
                _scanContext.Dispose();
                _scanContext = null;
            }


            if (_token != null)
                _token.Dispose();
            _token = null;
        }

        public void OnDeactivated(bool stopped)
        {
        }


        public void Refresh()
        {
            try
            {
                if(_scanContext != null)
                    _scanContext.RestartScanning();
                else
                    _scanContext = _service.Scan(OnAddDevice);
            }
            catch (Exception ex)
            {
                _messenger.Publish(new ErrorMessage(this, ex));
            }
        }

        private void OnAddDevice(RemoteDeviceInfo obj)
        {
            var newDevice = new RemoteDeviceViewModel(obj);
            var previous = Devices.FirstOrDefault(d => d.Address == obj.Address);
            if (previous != null)
                Devices[Devices.IndexOf(previous)] = newDevice;
            else
                Devices.Add(newDevice);
        }

    }
}
