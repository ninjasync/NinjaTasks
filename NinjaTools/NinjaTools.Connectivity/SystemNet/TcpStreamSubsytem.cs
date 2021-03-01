using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.Streams;

namespace NinjaTools.Connectivity.SystemNet
{
   public class TcpStreamSubsystem : IStreamSubsystem, ITcpStreamSubsystem
    {
        private readonly int _defaultPort;
        public string ServiceName { get { return "TCP/IP"; } }


        public TcpStreamSubsystem()
        {
        }

        public TcpStreamSubsystem(int defaultPort)
        {
            _defaultPort = defaultPort;
        }

        public IStreamConnector GetConnector(Endpoint endpoint)
        {
            int port = string.IsNullOrEmpty(endpoint.Port) ? _defaultPort : int.Parse(endpoint.Port);
            return new TcpStreamListener.TcpClientConnector(endpoint.Address, port);
        }

        public IStreamListener GetListener(Endpoint endpoint)
        {
            IPAddress ipAddress = string.IsNullOrEmpty(endpoint?.Address)?IPAddress.Any:IPAddress.Parse(endpoint.Address);
            int port = string.IsNullOrEmpty(endpoint?.Port) ? _defaultPort : int.Parse(endpoint.Port);
            return new TcpStreamListener(ipAddress, port);
        }

        public bool IsActivated { get { return true; }}
        public bool IsAvailableOnDevice { get { return true; } }

        private class TcpStreamListener : IStreamListener
        {
            private readonly TcpListener _tcpListener;
            private bool isStarted;

            public TcpStreamListener(IPAddress ipAddress, int port)
            {
                _tcpListener = new TcpListener(ipAddress, port);
            }

            public async Task<Stream> ListenAsync(CancellationToken cancel)
            {
                if(!isStarted)
                    _tcpListener.Start();

                var task = Task<TcpClient>.Factory.FromAsync(_tcpListener.BeginAcceptTcpClient,
                                                            _tcpListener.EndAcceptTcpClient, null);
                using (cancel.Register(_tcpListener.Stop))
                {
                    var client = await task;
                    return new TcpStream(client);
                }
            }

            public bool IsAvailable => true;

            public void Dispose()
            {
                _tcpListener.Stop();
                isStarted = false;
            }

            private class TcpStream : NetworkStreamAdapter, IAbortableStream
            {
                public TcpClient Client { get; set; }

                public TcpStream(TcpClient c) : base(c.GetStream())
                {
                    Client = c;
                }

                public void Abort()
                {
                    if (Client.Connected)
                        Client.Client.LingerState = new LingerOption(true, 0);
                    Client.Client.Close();
                }
            }

            public class NetworkStreamAdapter : StreamAdapter, INetworkStream
            {
                public string LocalIP { get; private set; }
                public string LocalPort { get; private set; }
                public string RemoteAddress { get; private set; }
                public NetworkStream Stream { get; private set; }

                public NetworkStreamAdapter(NetworkStream stream)
                    : base(stream)
                {
                    Stream = stream;
                }

                public NetworkStreamAdapter(NetworkStream stream, string localIP, string localPort, string remoteAddress)
                    : base(stream)
                {
                    LocalIP = localIP;
                    LocalPort = localPort;
                    RemoteAddress = remoteAddress;
                }
            }

            public class TcpClientConnector : IStreamConnector
            {
                private readonly string _host;
                private readonly int _port;

                public TcpClientConnector(string host, int port)
                {
                    _host = host;
                    _port = port;
                }

                public async Task<Stream> ConnectAsync(CancellationToken cancel)
                {
                    var client = new TcpClient();

                    cancel.ThrowIfCancellationRequested();

                    using (cancel.Register(client.Close)) 
                    {
                        try
                        {
                            cancel.ThrowIfCancellationRequested();
                            await client.ConnectAsync(_host, _port).ConfigureAwait(false);

                        } 
                        catch (NullReferenceException) when (cancel.IsCancellationRequested) 
                        {
                            cancel.ThrowIfCancellationRequested();
                        }
                    }

                    return FromTcpClient(client);
                }

                public bool IsAvailable { get { return true; } }

                public static NetworkStreamAdapter FromTcpClient(TcpClient c)
                {
                    var stream = c.GetStream();

                    var localIP = c.Client.LocalEndPoint.ToString().Split(':')[0];
                    var localPort = c.Client.LocalEndPoint.ToString().Split(':')[1];
                    var remoteAddress = c.Client.RemoteEndPoint.ToString().Split(':')[0];

                    var con = new NetworkStreamAdapter(stream, localIP, localPort, remoteAddress);
                    return con;
                }

                public void Dispose()
                {
                }
            }
        }

        #pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore CS0067 
        public void Dispose()
        {
        }
    }
}
