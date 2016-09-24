using System;

namespace NinjaSync.Exceptions
{
    public class SyncTryLaterException : Exception
    {
        public SyncTryLaterException(TimeSpan delay, Exception innerException=null)
            :base("try later", innerException)
        {
            DelayRetry = delay;
        }

        public SyncTryLaterException(string msg)
            : base(msg)
        {
            DelayRetry = default(TimeSpan);
        }

        public TimeSpan DelayRetry { get; set; }
    }
}