using System;

namespace NinjaTools.Connectivity.Streams
{
    /// <summary>
    /// a fixed size buffer, that keeps read and write positions
    /// </summary>
    public class CyclicBuffer
    {
        private readonly byte[] _buffer;
        private int _writePosition;
        private int _readPosition;
        private bool _overflow;

        public CyclicBuffer(int bufferSize)
        {
            _buffer = new byte[bufferSize];
        }

        public int WriteAvailable
        {
            get
            {
                if (_writePosition >= _readPosition && !_overflow)
                    return _buffer.Length - _writePosition + _readPosition;
                return _readPosition - _writePosition;
            }
        }

        public int ReadAvailable
        {
            get
            {
                if (_readPosition <= _writePosition && !_overflow)
                    return _writePosition - _readPosition;
                return _buffer.Length - _readPosition + _writePosition;
            }
        }

        public int Length { get { return _buffer.Length; } }

        public void Write(byte[] data, int offset, int length)
        {
            if (length > WriteAvailable)
                throw new ArgumentException("CyclicBuffer filled.");

            int startLength = Math.Min(length, _buffer.Length - _writePosition);
            Buffer.BlockCopy(data, offset, _buffer, _writePosition, startLength);

            _writePosition += startLength;
            if (_writePosition == _buffer.Length)
                _writePosition = 0;
            length -= startLength;
            if (length > 0)
                Buffer.BlockCopy(data, offset + startLength, _buffer, _writePosition, length);
            _writePosition += length;

            if (_writePosition == _readPosition)
                _overflow = true;
        }

        public void Read(byte[] data, int offset, int length)
        {
            if (length > ReadAvailable)
                throw new ArgumentException("not enough data.");
            int startLength = Math.Min(length, _buffer.Length - _readPosition);

            Buffer.BlockCopy(_buffer, _readPosition, data, offset, startLength);
            _readPosition += startLength;
            if (_readPosition == _buffer.Length)
                _readPosition = 0;
            length -= startLength;
            if (length > 0)
                Buffer.BlockCopy(_buffer, _readPosition, data, offset + startLength, length);
            _readPosition += length;

            _overflow = false;
        }
        
        public void Clear()
        {
            _writePosition = 0;
            _readPosition = 0;
            _overflow = false;
        }
    }
}