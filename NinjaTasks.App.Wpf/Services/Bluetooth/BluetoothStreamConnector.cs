using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InTheHand.Net;
using InTheHand.Net.Sockets;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.Streams;

namespace NinjaTools.Connectivity.Bluetooth._32Feet
{
    internal class BluetoothStreamConnector : IStreamConnector
    {
        public bool UseBufferedStream { get; }
        private readonly Endpoint _device;
        private BluetoothClient _client;


        public BluetoothStreamConnector(Endpoint device, bool useBufferedStream = true)
        {
            UseBufferedStream = useBufferedStream;
            _device = device;
        }

        public async Task<Stream> ConnectAsync(CancellationToken cancel)
        {
            var addr     = BluetoothAddress.Parse(_device.Address);
            var endpoint = new BluetoothEndPoint(addr, Guid.Parse(_device.Port));
            var client   = _client = new BluetoothClient();

            using(cancel.Register(client.Close))
                await Task.Factory.FromAsync((a, state) => client.BeginConnect(endpoint, a, state), client.EndConnect, null);
            _client = null;

            Stream stream = client.GetStream();

            if (UseBufferedStream)
                return new DuplexBufferedStream(stream, client);

            return new StreamAdapter(stream, client);
        }

        public bool IsAvailable => BluetoothStreamSubsystem.CheckBluetoothRadioOn();
        public bool WasCancelled => false;

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
