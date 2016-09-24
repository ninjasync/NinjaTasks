using System;
using Java.Util.Concurrent.Atomic;

namespace NinjaTools
{
    public class Guard
    {
        private readonly AtomicInteger _counter = new AtomicInteger();

        public GuardToken Use()
        {
            bool useChanged = _counter.IncrementAndGet() == 1;
            if (useChanged) OnInUseChanged();
            return new GuardToken(this);
        }

        public GuardToken TryUse()
        {
            if (_counter.IncrementAndGet() == 1)
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
            int val = _counter.DecrementAndGet();
            if (val == 0) OnInUseChanged();
            return val == 0;
        }

        public bool InUse { get { return _counter.Get() > 0; } }

        public void GuardExecution(Action action)
        {
            using (var use = TryUse())
            {
                if (use != null)
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
