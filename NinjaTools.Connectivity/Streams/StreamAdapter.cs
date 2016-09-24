using System;
using System.IO;

namespace NinjaTools.Connectivity.Streams
{
    public class StreamAdapter : CombinedStream
    {
        public Stream BaseStream { get { return _read; } set { _read = value; _write = value; }}
        public StreamAdapter(Stream s) : base(s, s)
        {
        }

        public StreamAdapter(Stream s, IDisposable keepReference)
            : base(s, s, keepReference)
        {
        }
    }
}
