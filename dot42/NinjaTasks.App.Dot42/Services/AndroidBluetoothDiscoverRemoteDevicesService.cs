using System;
using Android.Bluetooth;
using Android.Content;
using Dot42;
using NinjaTools;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;

namespace NinjaTasks.App.Droid.Services
{
    /// <summary>
    /// returns only bonded devices at the moment.
    /// </summary>
    public class AndroidBluetoothDiscoverRemoteDevicesService : IDiscoverRemoteDevices
    {
        private readonly Context _ctx;
        private readonly Guard _scanGuard = new Guard();
        private readonly BluetoothAdapter _bluetoothAdapter;

        //private static Guid Guid { get { return SqliteSyncServiceFactory.BluetoothGuid; } }

        public bool IsServiceEnabled { get; set; }

        public AndroidBluetoothDiscoverRemoteDevicesService(Context ctx)
        {
            _ctx = ctx;
            IsServiceEnabled = true;
            _scanGuard.InUseChanged += UpdateScanStatus;
            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        }

        public IScanContext Scan(Action<RemoteDeviceInfo> deviceFound)
        {
            if (_bluetoothAdapter == null) return new GuardBasedScanContext(this, _scanGuard);

            foreach(var dev in _bluetoothAdapter.BondedDevices.AsEnumerable())
                deviceFound(new RemoteDeviceInfo(RemoteDeviceInfoType.Bluetooth, dev.Name, dev.Address));

            return new GuardBasedScanContext(this, _scanGuard);
        }

        public IStreamConnector Create(RemoteDeviceInfo deviceInfo)
        {
            if (deviceInfo.DeviceType != RemoteDeviceInfoType.Bluetooth)
                throw new Exception("can only create bluetooth devices.");
            return new AndroidBluetoothFactory(_ctx).GetConnector(deviceInfo);
        }

        public event EventHandler ServiceEnabledChanged;

        public bool IsScanning { get; private set; }

        private void UpdateScanStatus(object sender, EventArgs eventArgs)
        {
            if (IsScanning && !_scanGuard.InUse)
            {
                // stop scanning
                StopScanning();
            }
            else if(!IsScanning && _scanGuard.InUse)
            {
                // start scanning
                StartScanning();
                
            }
        }

        private void StopScanning()
        {
            if (_bluetoothAdapter == null) return;

        }

        private void StartScanning()
        {
            if (_bluetoothAdapter == null) return;
            //_bluetoothAdapter.StartDiscovery();
        }

        public void Dispose()
        {
            StopScanning();
        }
    }
}