using System;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTools.Threading
{
    /// <summary>
    /// Will execute a command delayed by delayMs. When called multiple times
    /// before execution kicks in, only the last command will be executed.
    /// </summary>
    public class DelayedCommand
    {
        private readonly object _sync = new object();

        private Action _a;
        private Func<Task> _aTask;

        private Task _scheduledTask;
        private readonly int _delayMs;
        private readonly bool _reschedule;
        private CancellationTokenSource _rescheduler;

        private readonly TaskScheduler _scheduler;
        private readonly SynchronizationContext _context;

        /// <summary>
        /// when not specifying a scheduler, one will be created from current
        /// synchronization context.
        /// if reschedule = true, each additional call to Schedule will further delay the execution
        /// of the action.
        /// </summary>
        public DelayedCommand(int delayMs = 50, bool reschedule = true, TaskScheduler scheduler = null)
        {
            _delayMs = delayMs;
            _reschedule = reschedule;
            // http://stackoverflow.com/questions/6800705/why-is-taskscheduler-current-the-default-taskscheduler
            _scheduler = scheduler;
            _context = scheduler == null ? SynchronizationContext.Current : null;
        }

        public Task Schedule(Action a)
        {
            return Schedule(a, null);
        }

        //public Task Schedule(Func<Task> a)
        //{
        //    return Schedule(null, a);
        //}


        private Task Schedule(Action a, Func<Task> aTask)
        {
            lock (_sync)
            {
                _a = a;
                _aTask = aTask;

                if (_scheduledTask != null && !_reschedule)
                    return _scheduledTask;

                if (_reschedule)
                {
                    _rescheduler?.Cancel();
                    _rescheduler = new CancellationTokenSource();
                }

                var delay = Task.Delay(_delayMs);
                
                if (_scheduler != null)
                { 
                    _scheduledTask = delay.ContinueWith(ExecuteAction, _rescheduler?.Token ?? CancellationToken.None,
                                               TaskContinuationOptions.OnlyOnRanToCompletion,
                                               _scheduler);
}
                else
                {
                    _scheduledTask = delay.ContinueWith(ExecuteActionOnContext, _rescheduler?.Token ?? CancellationToken.None,
                                               TaskContinuationOptions.OnlyOnRanToCompletion,
                                               TaskScheduler.Default);
                }
                return _scheduledTask;
            }
            
        }

        private void ExecuteActionOnContext(Task obj)
        {
            Action a; Func<Task> t;
            PopCurrentAction(out a, out t);

            if (a != null)
                _context.Post(_ => a(), null);
            if (t != null)
                _context.Post(_ => t(), null);

        }

        private void ExecuteAction(Task obj)
        {
            Action a; Func<Task> t;
            PopCurrentAction(out a, out t);


            a?.Invoke();
            t?.Invoke();
        }

        private void PopCurrentAction(out Action a, out Func<Task> t)
        {
            lock (_sync)
            {
                a = _a;
                t = _aTask;

                _scheduledTask = null;
                _a = null;
                _aTask = null;
            }
        }

    }
}
