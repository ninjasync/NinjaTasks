using System;
using System.ComponentModel;
using InTheHand.Net.Bluetooth;
using NinjaTasks.Core.Services;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;

namespace NinjaTasks.App.Wpf.Services.Bluetooth
{
    public class BluetoothFactory : IBluetoothStreamFactory
    {
        public string ServiceName { get { return "Bluetooth"; } }

        public IStreamConnector GetConnector(RemoteDeviceInfo deviceInfo)
        {
            return new BluetoothConnector(deviceInfo, this);
        }

        public IStreamListener GetListener(RemoteDeviceInfo deviceInfo)
        {
            return new BluetoothStreamListener(Guid.Parse(deviceInfo.Port));
        }

        public bool IsActivated { get { return CheckBluetoothRadioOn(); } }
        public bool IsAvailableOnDevice { get { return true; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public static bool CheckBluetoothRadioOn()
        {
            return BluetoothRadio.IsSupported &&
                   BluetoothRadio.PrimaryRadio.Mode == RadioMode.Connectable;
        }

    }
}
