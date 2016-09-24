namespace NinjaTools.FrameRates
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