using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NinjaTools.Logging;

namespace NinjaTools.Connectivity.Streams
{
    /// <summary>
    /// Provides a Stream where all data written to can be read again. Uses an internal buffer,
    /// When the buffer gets full, the writing end is blocked. When the buffer is emtpy, the
    /// reading end is blocked.
    /// <para/>
    /// Fully supports the async pattern.
    /// <para/>
    /// To simulate a two-way network connection, you will need two PipeStreams, as i.e. created
    /// by PipeStreamFactory. Usually, when testing, you also want to have both sides of the 
    /// connection on different threads, even when using the async pattern, since there is no
    /// real dispatcher.
    /// </summary>
    public class PipeStream : Stream
    {
        private readonly ILogger _log = LogManager.GetLogger("PipeStream" + Unique.Create());

        private readonly SemaphoreSlim _dataWritten = new SemaphoreSlim(0);
        private readonly SemaphoreSlim _dataRead = new SemaphoreSlim(0);
        private readonly CyclicBuffer _buffer;
        private bool _shutdown;

        public PipeStream(int bufferSize = 4096)
        {
            _buffer = new CyclicBuffer(bufferSize);
        }
#if !DOT42
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
#else 
        public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
#endif
        {
            while (count > 0 && !_shutdown)
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool wroteData = false;
                lock (_buffer)
                {
                    int write = Math.Min(count, _buffer.WriteAvailable);
                    if (write > 0)
                    {
                        _buffer.Write(buffer, offset, write);
                        offset += write;
                        count -= write;
                        wroteData = true;
                    }
                }

                if (wroteData)
                {
                    _log.Info("Releasing reader.");
                    _dataWritten.Release();
                }

                if (count > 0)
                {
                    _log.Info("Waiting for write");
                    await _dataRead.WaitAsync(cancellationToken)
                                   .ConfigureAwait(false);
                    _log.Info("Waiting for write complee");
                }
            }

            if (_shutdown)
                throw new ObjectDisposedException("disposed while writing to pipe");

        }
#if !DOT42
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
#else
        public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
#endif

        {
            int read = 0;

            while (count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                lock (_buffer)
                {
                    read = Math.Min(count, _buffer.ReadAvailable);
                    if (read > 0)
                    {
                        _buffer.Read(buffer, offset, read);
                        offset += read;
                        count -= read;
                    }
                }

                if (read > 0 || _shutdown)
                {
                    _log.Info("Releasing writer.");
                    _dataRead.Release();
                    return read;
                }

                _log.Info("Waiting for read");
                await _dataWritten.WaitAsync(cancellationToken)
                                  .ConfigureAwait(false);
                _log.Info("Waiting for read complete");
            }

            return read;
        }

#if !DOT42
        public override Task FlushAsync(CancellationToken cancellationToken)
#else
        public Task FlushAsync(CancellationToken cancellationToken)
#endif
        {
            return Task.FromResult(0); // nothing to do...
        }

        public override void Flush()
        {
            //FlushAsync(CancellationToken.None).Wait();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;

            while (count > 0)
            {
                lock (_buffer)
                {
                    read = Math.Min(count, _buffer.ReadAvailable);
                    if (read > 0)
                    {
                        _buffer.Read(buffer, offset, read);
                        offset += read;
                        count -= read;
                    }
                }

                if (read > 0 || _shutdown)
                {
                    _log.Info("releasing writer sync.");
                    _dataRead.Release();
                    return read;
                }

                _log.Info("Waiting for read sync");
                _dataWritten.Wait();
                _log.Info("Waiting for read synccomplete");
            }

            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0 && !_shutdown)
            {
                bool wroteData = false;
                lock (_buffer)
                {
                    int write = Math.Min(count, _buffer.WriteAvailable);
                    if (write > 0)
                    {
                        _buffer.Write(buffer, offset, write);
                        offset += write;
                        count -= write;
                        wroteData = true;
                    }
                }

                if (wroteData)
                {
                    _log.Info("releasing reader sync.");
                    _dataWritten.Release();
                }

                if (count > 0)
                {
                    _log.Info("Waiting for write sync");
                    _dataRead.Wait();
                    _log.Info("Waiting for write sync complee");
                }
            }

            if (_shutdown)
                throw new ObjectDisposedException("disposed while writing to pipe");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }
        public override long Length { get { throw new NotImplementedException(); } }
        public override long Position { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (!disposing) return;

            _shutdown = true;
            _dataWritten.Release(10000);
            _dataRead.Release(10000);
        }

    }
}
