using System;
using System.Diagnostics;
using System.IO;

namespace NinjaTools.IPC.Console
{
    /// <summary>
    /// Liefert Asyncron Daten einer bestimmten Länge;
    /// Diese ChunkSize kann zunächst noch auf 0 gelassen werden, dann werden bis zu 20 MB 
    /// Daten gecached.
    /// </summary>
    /// 
    public class AsyncChunkReader
    {
        private readonly int _bufferLen;
        private readonly AsyncStreamReader _reader;
        private int _chunkSize;

        private MemoryStream _startBuffer = new MemoryStream();
        private byte[] _buffer;
        private int _bufpos;

        public int ChunkSize
        {
            get { return _chunkSize; }
            set
            {
                lock (_startBuffer)
                {
                    if (_buffer != null)
                        _startBuffer.Write(_buffer, 0, _bufpos);
                    _bufpos = 0;
                    _chunkSize = value;
                    if (_chunkSize == 0)
                        _buffer = null;
                    else
                    {
                        _buffer = new byte[_chunkSize + _bufferLen];
                        SendStartBuffer();
                    }
                }
            }
        }

        private void SendStartBuffer()
        {
            _startBuffer.Seek(0, SeekOrigin.Begin);
            while (true)
            {
                int read = _startBuffer.Read(_buffer, _bufpos, _buffer.Length - _bufpos);
                //Debug.Print("SendStartBuffer: read={0}, len={1}, pos={2}, ");
                if (read == 0) break;

                _bufpos += read;
                SendChunks();
            }

            _startBuffer.SetLength(0);
        }

        private void SendChunks()
        {
            lock (_startBuffer)
            {
                if (_chunkSize == 0) return;
                int pos = 0;
                // send all senable chunks in buffer
                while (pos + _chunkSize < _bufpos)
                {
                    if (ChunkRead != null)
                        ChunkRead(_buffer, _chunkSize);
                    pos += _chunkSize;
                }
                if (pos > 0 && _bufpos > pos)
                    Array.Copy(_buffer, pos, _buffer, 0, _bufpos - pos);
                _bufpos -= pos;
            }
        }

        public AsyncChunkReader(AsyncStreamReader streamReader)
        {
            _bufferLen = streamReader.BufferLen;
            _reader = streamReader;
            _reader.DataRead += _reader_DataRead;
        }

        void _reader_DataRead(byte[] read, int len)
        {
            if (read == null)
            {
                if (ChunkRead != null) ChunkRead(null, 0);
                return;
            }
            lock (_startBuffer)
            {
                if (_buffer == null)
                {
                    // bei 200MB ist Schluss...
                    if (_startBuffer.Position > 1000 * 1000 * 200)
                    {
                        Debug.WriteLine("warning: AsyncChunkReader overflow...");
                        return;
                    }
                    _startBuffer.Write(read, 0, len);
                }
                else
                {
                    Array.Copy(read, 0, _buffer, _bufpos, len);
                    _bufpos += len;
                    SendChunks();
                }
            }
        }

        public void Begin()
        {
            _reader.Begin();
        }

        public void End()
        {
            _reader.End();
        }

        public event Action<byte[], int> ChunkRead;
    }
}
