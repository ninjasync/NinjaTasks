using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NinjaTools.Performance
{
    public class FrameRateCalculator : IFrameRateCalculator
    {
        private IStopwatch watch;
        private long bytes;
        private int frames;

        public FrameRateCalculator(IStopwatch watch)
        {
            this.watch = watch;
        }

        public virtual void Start()
        {
            watch.Restart();
            bytes = 0;
        }

        public virtual void AddFrame(long addedBytes)
        {
            if(!watch.IsRunning)
                watch.Restart();

            Interlocked.Add(ref bytes, addedBytes);
            Interlocked.Increment(ref frames);
        }

        public double FrameRateHz { get { return frames / watch.Elapsed.TotalSeconds; } }
        public double BytePerSecond { get { { return bytes / watch.Elapsed.TotalSeconds; } } }
        public int Frames { get { return frames; } }
        public double Bytes { get { return bytes; } }
    }
}
