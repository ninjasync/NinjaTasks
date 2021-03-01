using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTools.Performance
{
    public interface IStopwatch
    {
        void Restart();
        void Stop();

        bool IsRunning { get; }
        TimeSpan Elapsed { get; }
    }
}
