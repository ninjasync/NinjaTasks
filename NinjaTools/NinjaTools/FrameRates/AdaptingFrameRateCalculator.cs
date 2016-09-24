using System;
using System.Threading;

namespace NinjaTools.FrameRates
{
    /// <summary>
    /// Calculates the Current FrameRate for the last 'Window' seconds.
    /// </summary>
    public class AdaptingFrameRateCalculator : IFrameRateCalculator
    {
        private readonly IStopwatch _watch;

        public TimeSpan Window { get; set; }
        
        private TimeSpan currentBin = TimeSpan.MinValue;

        private const int BIN_LEN = 20;
        private int[] binsFrame = new int[BIN_LEN];
        private long[] binsBytes = new long[BIN_LEN];
        private int binIdx = 0;

        private int totalFrames = 0;

        public AdaptingFrameRateCalculator(IStopwatch watch)
        {
            _watch = watch;
            Window = TimeSpan.FromSeconds(4);
        }

        public void Start()
        {
            Reset();
            _watch.Restart();
        }

        private void Reset()
        {
            for (int i = 0; i < binsFrame.Length; ++i)
            {
                binsBytes[i] = -1;
                binsFrame[i] = -1;
            }
            binIdx = 0;
            totalFrames = 0;

        }

        public void AddFrame(long addedBytes)
        {
            if(!_watch.IsRunning)
                Start();

            Interlocked.Increment(ref totalFrames);

            lock (binsFrame)
            {
                UpdateBins();
                binsFrame[binIdx] += 1;
                binsBytes[binIdx] += addedBytes;
            }
        }

        private void UpdateBins()
        {
            lock (binsFrame)
            {
                TimeSpan now = _watch.Elapsed;
                bool isFirst = currentBin == TimeSpan.MinValue;
                double binLength = Window.TotalSeconds/BIN_LEN;
                double posInBin = isFirst ? 0 : (now - currentBin).TotalSeconds;


                if (isFirst || posInBin > binLength)
                {
                    int nextBin = Math.Min((int) Math.Floor(posInBin/binLength), BIN_LEN);
                    // fill skipped bins with zero
                    for (; nextBin > 0; --nextBin)
                    {
                        binIdx = (binIdx + 1)%BIN_LEN;
                        binsFrame[binIdx] = 0;
                        binsBytes[binIdx] = 0;
                    }

                    // start a new bin
                    currentBin = now;
                }
            }
        }

        public double FrameRateHz
        {
            get
            {
                if(currentBin == TimeSpan.MinValue) return 0;
                lock (binsFrame)
                {
                    UpdateBins();

                    int partitialBinFrames = binsFrame[binIdx];
                    double binLength = Window.TotalSeconds / BIN_LEN;
                    double partitialLength = (_watch.Elapsed - currentBin).TotalSeconds;
                    int fullBinFrames = 0;
                    int fullBins = 0;
                    for (int i = 0; i < binsFrame.Length; ++i)
                    {
                        if (binsFrame[i] < 0 || i == binIdx) continue;
                        fullBinFrames += binsFrame[i];
                        fullBins += 1;
                    }

                    //int totalFrames = fullBinFrames + partitialBinFrames;
                    //double totalBins = fullBins + partitialLength/binLength;
                    int totalFrames = fullBinFrames;
                    double totalBins = fullBins;
                    if (Math.Abs(totalBins) < 0.02) return 0;
                    return totalFrames / Window.TotalSeconds * (BIN_LEN) / totalBins;
                }
            }
        }

        public double BytePerSecond
        {
            get
            {
                if (currentBin == TimeSpan.MinValue) return 0;
                lock (binsFrame)
                {
                    UpdateBins();

                    long partitialBinFrames = binsBytes[binIdx];
                    double binLength = Window.TotalSeconds/BIN_LEN;
                    double partitialLength = (_watch.Elapsed - currentBin).TotalSeconds;
                    long fullBinBytes = 0;
                    int fullBins = 0;
                    for (int i = 0; i < binsBytes.Length; ++i)
                    {
                        if (binsBytes[i] < 0 || i == binIdx) continue;
                        fullBinBytes += binsBytes[i];
                        fullBins += 1;
                    }

                    long totalBytes = fullBinBytes + partitialBinFrames;
                    double totalBins = fullBins + partitialLength/binLength;

                    if (System.Math.Abs(totalBins) < 0.02) return 0;
                    return totalBytes/Window.TotalSeconds*BIN_LEN/totalBins;
                }
            }
        }

        public int Frames
        {
            get { return totalFrames; }
        }
    }
}
