using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;

namespace NinjaTools.Connectivity.Bluetooth._32Feet
{
    public class BluetoothDiscoverRemoteDevicesService : IDiscoverRemoteEndpoints, IDiscoverBluetoothRemoteEndpoints
    {
        private readonly Guard _scanGuard = new Guard();

        private Guid _serviceGuid;

        public bool IsServiceEnabled => BluetoothStreamSubsystem.CheckBluetoothRadioOn();

        public BluetoothDiscoverRemoteDevicesService(Guid serviceGuid)
        {
            _serviceGuid = serviceGuid;
        }
        public IScanContext Scan(Action<Endpoint> deviceFound)
        {
            return new BluetoothScanContext(this);
        }

        public IStreamConnector Create(Endpoint deviceInfo)
        {
            if (deviceInfo.DeviceType != EndpointType.Bluetooth)
                throw new Exception("can only create bluetooth devices.");
            return new BluetoothStreamSubsystem().GetConnector(deviceInfo);
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
