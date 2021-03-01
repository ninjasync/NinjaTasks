using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTools.Connectivity.Streams
{
    public class StreamAdapter : CombinedStream, IStreamAdapter
    {
        public Stream BaseStream => ReadStream;

        public StreamAdapter(Stream s) : base(s, s)
        {
        }

        public StreamAdapter(Stream s, IDisposable keepReference)
            : base(s, s, keepReference)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !WasDisposed)
            {
                WasDisposed = true;

                try { ReadStream.Dispose(); }
                catch {}

                try { Parent?.Dispose(); }
                catch {}
            }
        }
    }
}
