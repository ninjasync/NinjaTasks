using System;
using System.IO;
using System.Threading.Tasks;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.Streams;

namespace NinjaTasks.App.Wpf.Services.Bluetooth
{
    public class BluetoothConnector : IStreamConnector
    {
        private readonly RemoteDeviceInfo _device;
        private readonly BluetoothFactory _factory;

        public BluetoothConnector(RemoteDeviceInfo device, BluetoothFactory factory)
        {
            _device = device;
            _factory = factory;
        }

        public async Task<Stream> ConnectAsync()
        {
            var addr = BluetoothAddress.Parse(_device.Address);
            var endpoint = new BluetoothEndPoint(addr, Guid.Parse(_device.Port));
            var client = new BluetoothClient();

            await Task.Factory.FromAsync((a, state) => client.BeginConnect(endpoint, a, state), client.EndConnect, null);
            return new StreamAdapter(client.GetStream(), client);
        }

        public bool IsAvailable { get { return _factory.IsActivated; } }
        public bool WasCancelled { get { return false; } }
    }
}
