using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTools.Performance
{
    public class Stopwatch : IStopwatch
    {
        System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
        public void Restart()
        {
            w.Restart();
        }

        public void Stop()
        {
            w.Stop();
        }

        public bool IsRunning { get { return w.IsRunning; } }
        public TimeSpan Elapsed { get { return w.Elapsed; } }
    }
}
