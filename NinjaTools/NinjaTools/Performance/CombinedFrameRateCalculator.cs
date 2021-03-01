using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTools.Performance
{
    public class CombinedFrameRateCalculator : FrameRateCalculator
    {
        public AdaptingFrameRateCalculator Adaptive { get; private set; }
        
        public CombinedFrameRateCalculator()
            : base(new Stopwatch())
        {
            Adaptive = new AdaptingFrameRateCalculator(new Stopwatch());
            Adaptive.Window = TimeSpan.FromSeconds(4);
        }

        public override void Start()
        {
            base.Start();
            Adaptive.Start();
        }

        public override void AddFrame(long addedBytes)
        {
            base.AddFrame(addedBytes);
            Adaptive.AddFrame(addedBytes);
        }
    }
}