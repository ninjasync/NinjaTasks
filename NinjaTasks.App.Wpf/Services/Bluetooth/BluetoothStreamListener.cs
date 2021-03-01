using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Streams;

namespace NinjaTools.Connectivity.Bluetooth._32Feet
{
    public class BluetoothStreamListener : IStreamListener
    {
        private readonly Guid _guid;
        private BluetoothListener _listener;

        public bool UseBufferedStream { get; set; }

        public BluetoothStreamListener(Guid guid, bool useBufferedStream = true)
        {
            UseBufferedStream = useBufferedStream;
            _guid = guid;
        }

        public void Dispose()
        {
            _listener?.Stop();
            _listener = null;
        }

        public async Task<Stream> ListenAsync(CancellationToken cancel)
        {
            var l = _listener = new BluetoothListener(_guid);

            using (cancel.Register(Dispose))
            {
                try
                {
                    l.Start();

                    var client = await Task<BluetoothClient>.Factory.FromAsync(_listener.BeginAcceptBluetoothClient,
                                                                      _listener.EndAcceptBluetoothClient, null);
                    if (_listener == null || cancel.IsCancellationRequested)
                    {
                        client.Dispose();
                        throw new OperationCanceledException();
                    }

                    Stream stream = client.GetStream();

                    if (UseBufferedStream)
                        return new DuplexBufferedStream(stream, client);

                    return new StreamAdapter(stream, client);
                }
                finally
                {
                    l.Stop();
                    Dispose();
                }
            }
        }

        public bool IsAvailable => BluetoothStreamSubsystem.CheckBluetoothRadioOn();
    }
}