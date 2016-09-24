using System;
using System.Threading;

namespace NinjaTools.Connectivity
{
    public static class WithTimeout
    {
        /// <summary>
        /// where timeoutAction must ensure that func is actually cancelled.
        /// </summary>
        public static T Run<T>(int timeoutMs, Action timeoutAction, Func<T> func)
        {
            if (timeoutMs <= 0)
                return func();

            CancellationTokenSource cancel = new CancellationTokenSource();
            cancel.CancelAfter(timeoutMs);

            try
            {
                using (cancel.Token.Register(timeoutAction))
                    return func();
            }
            catch (Exception)
            {
                if (cancel.IsCancellationRequested)
                    throw new TimeoutException("read timeout");
                throw;
            }
        }

        /// <summary>
        /// where timeoutAction must ensure that action is actually cancelled.
        /// </summary>
        public static void Run(int timeoutMs, Action timeoutAction, Action action)
        {
            if (timeoutMs <= 0)
            {
                action();
            }

            CancellationTokenSource cancel = new CancellationTokenSource();
            cancel.CancelAfter(timeoutMs);

            try
            {
                using (cancel.Token.Register(timeoutAction))
                    action();
            }
            catch (Exception)
            {
                if (cancel.IsCancellationRequested)
                    throw new TimeoutException("read timeout");
                throw;
            }
        }
    }
}
