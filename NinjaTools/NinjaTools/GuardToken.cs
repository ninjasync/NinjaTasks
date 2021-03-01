using System;

namespace NinjaTools
{
    public class GuardToken : IDisposable
    {
        private Guard _guard;

        public GuardToken(Guard guard)
        {
            this._guard = guard;
        }

        /// <summary>
        /// returns true if the Guard Counter becomes zero, i.e. the 
        /// guard becomes unused.
        /// </summary>
        /// <returns></returns>
        public bool Done()
        {
            if (_guard == null)
                throw new Exception("guard already done with.");
            var ret = _guard.Done();
            _guard = null;
            return ret;
        }

        public void Dispose()
        {
            if (_guard == null)
                return;
            Done();
        }
    }
}