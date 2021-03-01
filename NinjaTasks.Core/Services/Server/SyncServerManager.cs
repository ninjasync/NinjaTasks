using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Plugin.Messenger;
using NinjaSync.P2P;
using NinjaSync.P2P.Serializing;
using NinjaSync.Storage;
using NinjaTasks.Core.Messages;
using NinjaTools;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Npc;

namespace NinjaTasks.Core.Services.Server
{
    public abstract class SyncServerManager : ISyncServerManager
    {
        public DateTime LastSync { get; private set; }
        public string LastError { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsSyncActive { get; private set; }

        public bool IsAvailable { get; private set; }
        public bool IsAvailableOnDevice { get { return _streamFactory.IsAvailableOnDevice; }}
        
        private readonly IStreamSubsystem _streamFactory;
        private readonly ISyncStoragesFactory _storages;

        private readonly IMvxMessenger _msg;
        private P2PServer _p2pServer;

        private CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly object _sync = new object();

        protected abstract Endpoint GetListenAddress();
        protected bool ShouldBeActive { get; set; }

        private bool _isStartup;
        private TokenBag _bag = new TokenBag();

        public SyncServerManager(IStreamSubsystem streamFactory,
                                 ISyncStoragesFactory storages,
                                 IMvxMessenger msg)
        {
            _streamFactory = streamFactory;
            _storages = storages;

            _msg = msg;

            _bag += _streamFactory.BindToWeak("IsActivated", this, "IsAvailable");
        }

        private void OnShouldBeActiveChanged()
        {
            if (_isStartup)
            {
                _isStartup = false;
                DelayedUpdateActiveState();
            }
            else
                UpdateActiveState();
        }

        private async void DelayedUpdateActiveState()
        {
            await Task.Delay(2500);
            UpdateActiveState();
        }

        private void UpdateActiveState()
        {
            lock (_sync)
            {
                bool shouldRunServer = ShouldBeActive;


                if (_p2pServer != null && !shouldRunServer)
                {
                    // Stop the server
                    _p2pServer.Dispose();
                    _p2pServer.DataDownloaded -= OnDataDownloaded;
                    _p2pServer.ActiveRequestsChanged -= OnActiveRequestChange;
                    _p2pServer = null;
                    _cancel.Cancel();
                    IsRunning = false;
                    IsSyncActive = false;
                }
                else if (_p2pServer == null && shouldRunServer)
                {
                    _cancel = new CancellationTokenSource();

                    var listener = _streamFactory.GetListener(GetListenAddress());
                    _p2pServer = new P2PServer(listener, _streamFactory, _storages, new JsonNetModificationSerializer(new TodoTrackableFactory()));
                    _p2pServer.DataDownloaded += OnDataDownloaded;
                    _p2pServer.ActiveRequestsChanged += OnActiveRequestChange;

                    IsRunning = true;
                    IsSyncActive = _p2pServer.ActiveRequests > 0;
                    _p2pServer.StartServing();
                    RunUpdateErrorStatus(_cancel.Token);
                }
            }
        }

        private void OnActiveRequestChange(object sender, EventArgs e)
        {
            IsSyncActive = _p2pServer.ActiveRequests > 0;
        }

        private void OnDataDownloaded(object sender, EventArgs e)
        {
            LastSync = DateTime.UtcNow;
            _msg.Publish(new TrackableStoreModifiedMessage(this, ModificationSource.P2PExchange));
        }

        private async void RunUpdateErrorStatus(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var bt = _p2pServer;
                    if (bt == null) return;
                    LastError = bt.LastError;
                    await Task.Delay(1000, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }


        #pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore CS0067


        public void Dispose()
        {
            _cancel.Cancel();
            var bt = _p2pServer;
            if (bt != null)
                bt.StopServing();
        }
    }
}
