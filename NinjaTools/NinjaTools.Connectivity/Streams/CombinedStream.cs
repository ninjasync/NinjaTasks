using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTools.Connectivity.Streams
{
    /// <summary>
    /// combines a read and a write stream to a single stream.
    /// </summary>
    public class CombinedStream : Stream
    {
        public readonly Stream ReadStream;
        public readonly Stream WriteStream;

        private protected readonly IDisposable Parent;
        private protected bool WasDisposed = false;

        /// <param name="read"></param>
        /// <param name="write"></param>
        /// <param name="parent">if not null, will be disposed when we are disposed.</param>
        public CombinedStream(Stream read, Stream write, IDisposable parent = null)
        {
            ReadStream  = read;
            WriteStream = write;
            Parent      = parent;
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return ReadStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return WriteStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return ReadStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return WriteStream.FlushAsync(cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !WasDisposed)
            {
                WasDisposed = true;

                try { ReadStream.Dispose(); }
                catch {}

                if(!ReferenceEquals(ReadStream, WriteStream))
                {
                    try { WriteStream.Dispose(); }
                    catch {}
                }

                try { Parent?.Dispose(); }
                catch {}
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            if (WasDisposed) return;
            WriteStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ReadStream.Seek(offset, origin);
            return WriteStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            WriteStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteStream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return ReadStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return ReadStream.CanSeek && WriteStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return WriteStream.CanWrite; }
        }

        public override long Length
        {
            get { return ReadStream.Length; }
        }

        public override bool CanTimeout
        {
            get { return ReadStream.CanTimeout || WriteStream.CanTimeout; }
        }

        public override int ReadTimeout { get { return ReadStream.ReadTimeout; } set { ReadStream.ReadTimeout = value; } }
        public override int WriteTimeout { get { return WriteStream.WriteTimeout; } set { WriteStream.WriteTimeout = value; } }

        public override long Position { get { return ReadStream.Position; } set { ReadStream.Position = value; WriteStream.Position = value; } }
    }
}
