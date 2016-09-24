using System;
using NinjaTasks.Core.Services;
using NinjaTools;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;

namespace NinjaTasks.App.Wpf.Services.Bluetooth
{
    public class BluetoothDiscoverRemoteDevicesService : IDiscoverRemoteDevices
    {
        private readonly Guard _scanGuard = new Guard();

        private static Guid Guid { get { return SqliteSyncServiceFactory.BluetoothGuid; } }


        public bool IsServiceEnabled { get; set; }

        public BluetoothDiscoverRemoteDevicesService()
        {
            IsServiceEnabled = true;
        }
        public IScanContext Scan(Action<RemoteDeviceInfo> deviceFound)
        {
            return new BluetoothScanContext(this);
        }

        public IStreamConnector Create(RemoteDeviceInfo deviceInfo)
        {
            if (deviceInfo.DeviceType != RemoteDeviceInfoType.Bluetooth)
                throw new Exception("can only create bluetooth devices.");
            return new BluetoothFactory().GetConnector(deviceInfo);
        }

        public event EventHandler ServiceEnabledChanged;

        public bool IsScanning { get; private set; }

        private void UpdateScanStatus()
        {
            if (IsScanning && !_scanGuard.InUse)
            {
                // stop scanning.
                IsScanning = _scanGuard.InUse;
            }
            else if (!IsScanning && _scanGuard.InUse)
            {
                // start scanning
                IsScanning = true;
            }
        }

        public class BluetoothScanContext : IScanContext 
        {
            private readonly BluetoothDiscoverRemoteDevicesService _parent;
            private IDisposable _token;

            public BluetoothScanContext(BluetoothDiscoverRemoteDevicesService parent)
            {
                _parent = parent;
                RestartScanning();
            }

            public bool IsScanning { get { return _parent.IsScanning; }}
            public void StopScanning()
            {
                if (_token == null) return;
                _token.Dispose();
                _parent.UpdateScanStatus();
                _token = null;
            }

            public void RestartScanning()
            {
                if (_token != null) return;
                _token = _parent._scanGuard.Use();
                _parent.UpdateScanStatus();
            }
            public void Dispose()
            {
                StopScanning();
            }
        }

        public void Dispose()
        {
            
        }
    }
}
