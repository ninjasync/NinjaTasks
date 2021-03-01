using System;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTools.Threading
{
    /// <summary>
    /// Timer, based on System.Threading.Task
    /// </summary>
    public class TaskTimer : IDisposable
    {
        public event EventHandler Tick;

        private bool _isActive;
        
        private readonly bool _tickOnCapturedSynchronizationContext;
        private CancellationTokenSource _cancel;
        private readonly object _sync = new object();
        private TimeSpan _tickDuration;

        public TaskTimer(TimeSpan tickDuration, bool tickOnCapturedSynchronizationContext=false)
        {
            _tickOnCapturedSynchronizationContext = tickOnCapturedSynchronizationContext;
            _tickDuration = tickDuration;
        }

        public TaskTimer(long milliseconds, bool tickOnCapturedSynchronizationContext = false)
        {
            _tickOnCapturedSynchronizationContext = tickOnCapturedSynchronizationContext;
            _tickDuration = TimeSpan.FromMilliseconds(milliseconds);
        }

        public bool IsEnabled
        {
            get { return _isActive; } 
            set { if (value) Start(); else Stop(); }
        }

        public TimeSpan TickDuration
        {
            get { return _tickDuration; }
            set
            {
                _tickDuration = value;
                if (_isActive)
                {
                    Stop();
                    Start();
                }
            }
        }

        public void Start()
        {
            lock (_sync)
            {
                if (_isActive)
                    return;
                _isActive = true;
                _cancel = new CancellationTokenSource();
                RunTimer(_cancel.Token);
            }
        }

        private void Stop()
        {
            lock (_sync)
            {
                if (!_isActive)
                    return;
                _isActive = false;
                _cancel.Cancel();
            }
        }

        private async void RunTimer(CancellationToken cancel)
        {
            try
            {
                while (!cancel.IsCancellationRequested)
                {
                    var delay = Task.Delay(TickDuration, cancel);

                    if (_tickOnCapturedSynchronizationContext)
                        await delay;
                    else
                        await delay.ConfigureAwait(false);

                    if (cancel.IsCancellationRequested)
                        return;

                    // Note that we only restart the timer once all
                    // tick handlers have finished.
                    // this might not be optimal.
                    FireTick();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void Dispose()
        {
            Stop();
        }

        protected virtual void FireTick()
        {
            var handler = Tick;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
