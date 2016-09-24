using System.IO;

namespace NinjaTools.Connectivity.Streams
{
    /// <summary>
    /// this will add timeout functionality to the blocking calls 
    /// of the stream.
    /// </summary>
    public class TimeoutStream : StreamAdapter
    {
        // TODO: provide async overrides as well.

        public TimeoutStream(Stream s) : base(s)
        {
        }

        public override bool CanTimeout { get { return true; } }

        public override int ReadTimeout { get; set; }
        public override int WriteTimeout { get; set; }

        public override void Flush()
        {
            if(WriteTimeout <= 0)
                base.Flush();
            else
                WithTimeout.Run(WriteTimeout, Dispose, () => base.Flush());
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (WriteTimeout <= 0)
                base.Write(buffer, offset, count);
            else
                WithTimeout.Run(WriteTimeout, Dispose, () => base.Write(buffer, offset, count));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (ReadTimeout <= 0)
                return base.Read(buffer, offset, count);

            return WithTimeout.Run(ReadTimeout, Dispose, () => base.Read(buffer, offset, count));
        }
    }
}
