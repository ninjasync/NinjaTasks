using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.Streams;
using NinjaTools.Logging;

namespace NinjaTools.Connectivity.Connections
{
    /// <summary>
    /// For testing network functionality it is often desirable to tests the data exchange 
    /// without actually having to setup a physical network, possibly even on different 
    /// devices. This class provides the typical Network interfaces (Listen and Connect). 
    /// All connections are transferred over PipeStreams.
    /// <para/>
    /// Usually, when testing, you want to have both sides of the connection on different threads, 
    /// even when using the async pattern, since there is no real dispatcher.
    /// </summary>
    public class PipeStreamSubsystem: IStreamListener, IStreamConnector, IStreamSubsystem
    {
        public int BufferSize { get; set; }
        public string ServiceName { get; private set; }

        private readonly ILogger Log;

        private readonly LinkedList<Tuple<Stream, Stream, SemaphoreSlim, int>> _connectableStreams = new LinkedList<Tuple<Stream, Stream, SemaphoreSlim,int>>();
        private readonly SemaphoreSlim _waitConnect = new SemaphoreSlim(0);

        public PipeStreamSubsystem(int bufferSize = 1024, string name = null)
        {
            BufferSize = bufferSize;
            ServiceName = name ?? "PipeStreams";
            Log = LogManager.GetLogger(typeof (PipeStreamSubsystem).FullName + (name == null ? "" : "." + name));
        }

        public async Task<Stream> ConnectAsync(CancellationToken cancel = default)
        {
            int id = Unique.Create();
            Log.Trace("Connect: " + id);
            await _waitConnect.WaitAsync(cancel).ConfigureAwait(false);
            lock (_connectableStreams)
            {
                var ret = _connectableStreams.First;
                _connectableStreams.RemoveFirst();

                Log.Trace("Connection established from {0} to {1}.", id, ret.Value.Item4);

                // release the listener.
                var connectedEvent = ret.Value.Item3;
                connectedEvent.Release();
                return new CombinedStream(ret.Value.Item1, ret.Value.Item2);
            }
        }

        public async Task<Stream> ListenAsync(CancellationToken token)
        {
            int id = Unique.Create();
            Log.Trace("Listen: " + id);

            // create a read and a write end. 
            Stream pWeWrite = new MonitorStream(new PipeStream(BufferSize), "pipe.", true);
            Stream pWeRead = new MonitorStream(new PipeStream(BufferSize), "pipe.", true);

            SemaphoreSlim connectedEvent = new SemaphoreSlim(0);
            lock (_connectableStreams)
                _connectableStreams.AddLast(new Tuple<Stream, Stream, SemaphoreSlim, int>(pWeWrite, pWeRead, connectedEvent, id));
            
            // release one client
            _waitConnect.Release();
            await connectedEvent.WaitAsync(token).ConfigureAwait(false); ;
            connectedEvent.Dispose();
            Log.Trace("Listen Successful: " + id);
            return new CombinedStream(pWeRead, pWeWrite);
        }

        IStreamConnector IStreamSubsystem.GetConnector(Endpoint device)
        {
            return this;
        }

        public bool IsActivated { get { return true; }}
        public bool IsAvailableOnDevice { get { return true; } }

        public IStreamListener GetListener(Endpoint deviceInfo)
        {
            return this;
        }
        
        public bool IsAvailable { get { return true; }  }
        
        public void Dispose()
        {
            _waitConnect.Dispose();

            lock(_connectableStreams)
                foreach (var cc in _connectableStreams)
                {
                    cc.Item1.Dispose();
                    cc.Item2.Dispose();
                }
        }

        #pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore CS0067
    }
}
