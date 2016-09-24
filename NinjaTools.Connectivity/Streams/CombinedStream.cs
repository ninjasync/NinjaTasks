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
        protected Stream _read;
        protected Stream _write;
        private readonly IDisposable _parent;

        /// <param name="read"></param>
        /// <param name="write"></param>
        /// <param name="parent">if not null, will be disposed when we are disposed.</param>
        public CombinedStream(Stream read, Stream write, IDisposable parent = null)
        {
            _read = read;
            _write = write;
            _parent = parent;
        }

#if !DOT42
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _read.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _write.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _read.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _write.FlushAsync(cancellationToken);
        }
#endif

        private bool wasDisposed = false;
        protected override void Dispose(bool disposing)
        {
            
            if (disposing && !wasDisposed)
            {
                wasDisposed = true;

                _read.Dispose();
                _write.Dispose();

                if(_parent != null)
                    _parent.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            if (wasDisposed) return;
            _write.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            _read.Seek(offset, origin);
            return _write.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _write.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _read.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _write.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return _read.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _read.CanSeek && _write.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _write.CanWrite; }
        }

        public override long Length
        {
            get { return _read.Length; }
        }

        public override bool CanTimeout
        {
            get { return _read.CanTimeout || _write.CanTimeout; }
        }

        public override int ReadTimeout { get { return _read.ReadTimeout; } set { _read.ReadTimeout = value; } }
        public override int WriteTimeout { get { return _write.WriteTimeout; } set { _write.WriteTimeout = value; } }

        public override long Position { get { return _read.Position; } set { _read.Position = value; _write.Position = value; } }
    }
}
