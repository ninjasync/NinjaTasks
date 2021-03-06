﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Logging;
using NinjaTools.Npc;

namespace NinjaTools.Connectivity.Server
{
    /// <summary>
    /// This class Represents server listening for requests to be served over
    /// some stream, delivered by an IStreamListenerFactory
    /// </summary>
    public abstract class StreamListenServer  : IStreamListenServer
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IStreamListener _listen;
        private readonly IStreamSubsystem _system;

        private bool _isActive;
        public bool IsActive { get; private set; }
        public bool IsListening { get; private set; }
        public bool HasListeningErrors { get; private set; }
        public abstract string LastError { get; protected set; }

        public TimeSpan DelayRelistenOnError { get; set; } = TimeSpan.FromMilliseconds(2500);
        
        private readonly SemaphoreSlim _waitServiceAvailability = new SemaphoreSlim(0);
        private SemaphoreSlim _stop = new SemaphoreSlim(0);
        private CancellationTokenSource _cancel;

        public StreamListenServer(IStreamSubsystem subsystem, IStreamListener lister)
        {
            _listen = lister;
            _system = subsystem;
        }

        /// <summary>
        /// this is called synchronously. if the inheritor wants to serve 
        /// multiple requests at the same time, it should run the handling in
        /// a new thread.
        /// <para/>
        /// Depending on the setup, it could be dangerous to run non-async/await
        /// blocking code in the handler.
        /// </summary>
        /// <param name="stream">this stream should be closed by the called method 
        /// when done with it.
        /// </param>
        /// <param name="cancel"></param>
        protected abstract void HandleRequest(Stream stream, CancellationToken cancel);


        public async void StartServing()
        {
            Log.Info("starting to serve.");
            
            lock (_stop)
            {
                if (_isActive) return;
                _stop = new SemaphoreSlim(0);
                _cancel = new CancellationTokenSource();
                _isActive = true;
            }
            // this assignment will make Fody.PropertyChange notify any listerners.
            IsActive = _isActive;

            using (_system.Subscribe(nameof(IStreamSubsystem.IsActivated), OnServiceAvailabilityChanged))
            {
                try
                {
                    while (!_cancel.IsCancellationRequested)
                    {
                        Stream currentStream = null;
                        try
                        {
                            if (!_system.IsActivated)
                            {
                                Log.Info("service {0} not available. waiting...", _system.ServiceName);
                                LastError   = _system.ServiceName + " not available.";
                                IsListening = false;
                                await _waitServiceAvailability.WaitAsync(_cancel.Token);
                                if (_cancel.Token.IsCancellationRequested) break;
                                continue;
                            }

                            IsListening = true;

                            currentStream = await _listen.ListenAsync(_cancel.Token);
                            if (_cancel.Token.IsCancellationRequested)
                            {
                                currentStream.Dispose();
                                break;
                            }

                            HasListeningErrors = false;

                            HandleRequest(currentStream, _cancel.Token);

                            currentStream = null;
                            continue;

                        }
                        catch (ObjectDisposedException)
                        {
                            currentStream?.Dispose();
                            continue;
                        }
                        catch (OperationCanceledException)
                        {
                            currentStream?.Dispose();
                            continue;
                        }
                        catch (Exception ex)
                        {
                            Log.Error("got exception:");
                            Log.Error(ex);
                            HasListeningErrors = true;
                            LastError = ex.Message;

                            currentStream?.Dispose();
                        }

                        IsListening = false;
                        // wait upon retry to re-listen.
                        await Task.Delay(DelayRelistenOnError, _cancel.Token);
                        if (_cancel.Token.IsCancellationRequested) break;
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    _isActive = false;
                    IsListening = false;
                    
                    Log.Info("stopping service.");
                    _stop.Release(100000); // ups. maybe the semaphore wasn't the right synchronization object;
                                           // which one is the right one, that also provides WaitSync????
                    IsActive = _isActive;
                }
            }
        }

        public Task StopServing()
        {
            SemaphoreSlim wait;
            lock (_stop)
            {
                wait = _stop;
                if (!IsActive) return Task.FromResult(0);    
            }

            _cancel.Cancel();
            return wait.WaitAsync();
        }

        private void OnServiceAvailabilityChanged()
        {
            _waitServiceAvailability.Release();
        }

        public void Dispose()
        {
            StopServing();
            _listen.Dispose();
        }
    }
}
