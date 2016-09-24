using System;

namespace NinjaTools.FrameRates
{
    public interface IStopwatch
    {
        void Restart();
        void Stop();

        bool IsRunning { get; }
        TimeSpan Elapsed { get; }
    }
}
