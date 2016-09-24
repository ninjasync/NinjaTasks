using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Streams;

namespace NinjaTasks.App.Wpf.Services.Bluetooth
{
    public class BluetoothStreamListener : IStreamListener
    {
        private readonly Guid _guid;
        private BluetoothListener _listener;

        public BluetoothStreamListener(Guid guid)
        {
            _guid = guid;
        }

        public void Dispose()
        {
            var l = _listener;
            if (l == null) return;

            l.Stop();
            _listener = null;
        }

        public async Task<Stream> ListenAsync(CancellationToken token)
        {
            _listener = new BluetoothListener(_guid);

            try
            {
                _listener.Start();
                token.Register(Dispose);

                var client = await Task<BluetoothClient>.Factory.FromAsync(_listener.BeginAcceptBluetoothClient,
                    _listener.EndAcceptBluetoothClient, null);
                if (_listener == null)
                {
                    client.Dispose();
                    throw new OperationCanceledException();
                }

                return new StreamAdapter(client.GetStream(), client);
            }
            finally 
            {
                Dispose();
            }
            
        }
    }
}