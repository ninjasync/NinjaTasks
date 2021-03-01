using System;
using System.Diagnostics;
using System.Globalization;

namespace NinjaTools.Logging
{
    /// <summary>
    /// Helper for profiling performance.
    /// </summary>
    public static class Profiler 
    {
        /// <summary>
        /// Start a timer and call the given delegate with the elapsed time when the returned object is disposed.
        /// </summary>
        public static IDisposable Profile(Action<TimeSpan> onDone)
        {
            return new ProfileInstance(onDone);
        }

        public static IDisposable DebugProfile(string msg, bool isSummary = false)
        {
            return Profile(x => Debug.WriteLine("{2}{0} ms {1}", x.TotalMilliseconds.ToString("#,000", CultureInfo.InvariantCulture).PadLeft(6), msg, isSummary ? "------\n" : ""));
        }

        private sealed class ProfileInstance : IDisposable
        {
            private readonly Action<TimeSpan> onDone;
            private readonly DateTime startTime;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ProfileInstance(Action<TimeSpan> onDone)
            {
                this.onDone = onDone;
                startTime = DateTime.Now;
            }

            public void Dispose()
            {
                var timespan = DateTime.Now.Subtract(startTime);
                onDone(timespan);
            }
        }
    }
}
