using System;
using System.Threading;

namespace NinjaTools
{
    public class Guard
    {
        private int _counter;

        public GuardToken Use()
        {
            bool useChanged = Interlocked.Increment(ref _counter) == 1;
            if(useChanged) OnInUseChanged();
            return new GuardToken(this);
        }

        public GuardToken TryUse()
        {
            if (Interlocked.Increment(ref _counter) == 1)
            {
                OnInUseChanged();
                return new GuardToken(this);
            }
            else
            {
                Done();
                return null;
            }
        }

        internal bool Done()
        {
            int val = Interlocked.Decrement(ref _counter);
            if(val == 0) OnInUseChanged();
            return val == 0;
        }

        public bool InUse { get { return _counter > 0; } }

        public void GuardExecution(Action action)
        {
            using (var use = TryUse())
            {
                if(use != null)
                    action();
            }
        }

        public event EventHandler InUseChanged;

        protected virtual void OnInUseChanged()
        {
            EventHandler handler = InUseChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
