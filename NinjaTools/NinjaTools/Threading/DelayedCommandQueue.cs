using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NinjaTools.Collections;
using NinjaTools.Progress;

namespace NinjaTools.Threading
{
    /// <summary>
    /// Schedules task to be run not before a certain delay. Good to accumulate UI-Updates.
    /// Only one command will be run at any time.
    /// </summary>
    public class DelayedCommandQueue
    {
        private class Entry
        {
            public Delegate Action;
            public string Klasse;
        }

        private readonly PriorityQueue<DateTime, Entry> _commands = new PriorityQueue<DateTime, Entry>();
        private readonly HashSet<string> _enquedClassed = new HashSet<string>();

        private Task _commandTask;
        private CancellationTokenSource _cancelSource;
        private volatile bool _wasCompleted;
        private readonly AutoResetEvent _queueChangedEvent = new AutoResetEvent(false);

        private readonly TimeSpan _defaultDelay;
        private readonly SynchronizationContext _ctx;

        public DelayedCommandQueue(bool callOnCapturedSynchronizationContext = false)
            :this(TimeSpan.Zero, callOnCapturedSynchronizationContext)
        {
        }

        public DelayedCommandQueue(TimeSpan defaultDelay, bool callOnCapturedSynchronizationContext = false)
        {
            _defaultDelay = defaultDelay;

            if(callOnCapturedSynchronizationContext)
                _ctx = SynchronizationContext.Current;
        }

        /// <summary>
        /// returns true if there are items enqueued. Note that this does include currently executing commands.
        /// </summary>
        public bool IsEmpty { get { lock (_commands) return _commands.IsEmpty; } }

        [DebuggerNonUserCode]
        private void OnBackgroundTask(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                TimeSpan delay;
                
                Monitor.Enter(_commands);

                if (_commands.Count == 0) { _wasCompleted = true; Monitor.Exit(_commands); return; }
                var next = _commands.Peek().Key;
                delay = next - DateTime.Now;
                
                if(delay > TimeSpan.Zero)
                {
                    Monitor.Exit(_commands);
                    _queueChangedEvent.WaitOne(delay);
                    continue; // reevaluate.
                }

                var command = _commands.DequeueValue();
                if (command.Klasse != null) _enquedClassed.Remove(command.Klasse);

                Monitor.Exit(_commands);

                // execute Action.
                var action0 = command.Action as Action;
                if (action0 != null)
                {
                    if(_ctx != null)
                        _ctx.Send(ignore=>action0(), null);
                    else
                        action0();
                }
                else
                {
                    var action1 = command.Action as Action<IProgress>;
                    Debug.Assert(action1 != null);

                    if (_ctx != null)
                        _ctx.Send(ignore => action1(new CancelableProgress(token)), null);
                    else
                        action1(new CancelableProgress(token));
                }
            }

        }

        public void Cancel(bool waitForCompletion = true)
        {
            lock (_commands)
            {
                _commands.Clear();
                if (_cancelSource != null) _cancelSource.Cancel();
                _queueChangedEvent.Set();
            }

            var task = _commandTask;
            if (waitForCompletion && task != null)
                _commandTask.Wait();
        }

        
        [DebuggerHidden]
        private void Add(Delegate action, TimeSpan delayExecution, string klasse = null)
        {
            lock (_commands)
            {
                bool handled = false;
                if (klasse != null && _enquedClassed.Contains(klasse))
                {
                    var a = _commands.FirstOrDefault(x => x.Value.Klasse == klasse);
                    if (a.Value != null)
                    {
                        a.Value.Action = action;
                        handled = true;
                    }
                }

                if (!handled)
                {
                    var e = new Entry { Action = action, Klasse = klasse };
                    if (klasse != null) _enquedClassed.Add(klasse);
                    _commands.Enqueue(DateTime.Now + delayExecution, e);
                }
                
                bool needsRestart = (_commandTask == null || _wasCompleted || _commandTask.IsCompleted);

                if (needsRestart)
                {
                    _cancelSource = new CancellationTokenSource();
                    var token = _cancelSource.Token;
                    _commandTask = new Task(() =>
                        {
                            try { OnBackgroundTask(token); }
                            catch (OperationCanceledException) { }
                        }, token);

                    _wasCompleted = false;
                    _commandTask.Start();
                }
                else
                {
                    _queueChangedEvent.Set();
                }
            }
        }

        /// <summary>
        /// If 'klasse' is set, the command will replace any commands belonging to
        /// the same 'klasse', and delayExecution will be ignored.
        /// <para>
        /// Fully threadsafe:  can be called from any thread.
        /// </para>
        /// </summary>
        public void Add(Action action, string klasse = null)
        {
            Add((Delegate)action, _defaultDelay, klasse);
        }
        /// <summary>
        /// If klasse is set, the command will replace any commands belonging to
        /// the same class, and delayExecution will be ignored.
        /// <para>
        /// Fully threadsafe:  can be called from any thread.
        /// </para>
        /// </summary>
        public void Add(Action<IProgress> action, string klasse = null)
        {
            Add((Delegate)action, _defaultDelay, klasse);
        }

        /// <summary>
        /// If klasse is set, the command will replace any commands belonging to
        /// the same class, and delayExecution will be ignored.
        /// <para>
        /// Fully threadsafe:  can be called from any thread.
        /// </para>
        /// </summary>
        public void Add(Action action, TimeSpan delay, string klasse = null)
        {
            Add((Delegate)action, delay, klasse);
        }
        /// <summary>
        /// If klasse is set, the command will replace any commands belonging to
        /// the same class, and delayExecution will be ignored.
        /// <para>
        /// Fully threadsafe:  can be called from any thread.
        /// </para>
        /// </summary>
        public void Add(Action<IProgress> action, TimeSpan delay, string klasse = null)
        {
            Add((Delegate)action, delay, klasse);
        }
    }
}
