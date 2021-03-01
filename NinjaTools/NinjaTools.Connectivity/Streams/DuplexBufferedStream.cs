using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTools.Connectivity.Streams
{
    /// <summary>
    /// a buffered stream, allowing 
    /// </summary>
    public class DuplexBufferedStream : CombinedStream, IStreamAdapter
    {
        public Stream BaseStream { get; }

        public DuplexBufferedStream(Stream baseStream, IDisposable keepReference)
            : base(new BufferedStream(baseStream), new BufferedStream(baseStream), keepReference)
        {
            BaseStream = baseStream;
        }

        public DuplexBufferedStream(Stream baseStream) : this(baseStream, null)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !WasDisposed)
            {
                WasDisposed = true;

                try { WriteStream.Dispose(); }
                catch {}

                try { Parent?.Dispose(); }
                catch {}
            }
        }
    }
}
