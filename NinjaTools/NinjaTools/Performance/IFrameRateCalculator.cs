using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTools.Performance
{
    public interface IFrameRateCalculator
    {
        void Start();
        void AddFrame(long addedBytes);

        double FrameRateHz { get; }
        double BytePerSecond { get; }
        int Frames { get; }
    }
}