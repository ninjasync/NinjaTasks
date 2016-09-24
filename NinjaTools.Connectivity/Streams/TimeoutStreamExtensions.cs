using System;
using System.IO;

namespace NinjaTools.Connectivity.Streams
{
    public static class TimeoutStreamExtensions
    {
        /// <summary>
        /// If the given stream is not timeout capable by itself,
        /// it will be wrapped by a TimeoutStream.
        /// </summary>
        public static Stream EnsureTimeoutCapable(this Stream stream)
        {
            if (stream.CanTimeout)
                return stream;
            return new TimeoutStream(stream);
        }

        /// <summary>
        /// Will set the timeouts on the stream. If the stream does not nativly 
        /// support timeouts it will be wrapped in a TimeoutStream.
        /// </summary>
        public static Stream SafeSetTimeout(this Stream stream, TimeSpan readTimeout, TimeSpan writeTimeout)
        {
            return SafeSetTimeout(stream, (int)readTimeout.TotalMilliseconds, (int)writeTimeout.TotalMilliseconds);
        }

        /// <summary>
        /// Will set the timeouts on the stream. If the stream does not nativly 
        /// support timeouts it will be wrapped in a TimeoutStream.
        /// </summary>
        public static Stream SafeSetTimeout(this Stream stream, TimeSpan timeout)
        {
            return SafeSetTimeout(stream, (int)timeout.TotalMilliseconds, (int)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// Will set the timeouts on the stream. If the stream does not nativly 
        /// support timeouts it will be wrapped in a TimeoutStream.
        /// </summary>
        public static Stream SafeSetTimeout(this Stream stream, int timeoutMs)
        {
            return SafeSetTimeout(stream, timeoutMs, timeoutMs);
        }

        /// <summary>
        /// Will set the timeouts on the stream. If the stream does not nativly 
        /// support timeouts it will be wrapped in a TimeoutStream.
        /// </summary>
        public static Stream SafeSetTimeout(this Stream stream, int readTimeoutMs, int writeTimeoutMs)
        {
            stream = EnsureTimeoutCapable(stream);
            stream.ReadTimeout = readTimeoutMs;
            stream.WriteTimeout = writeTimeoutMs;
            return stream;
        }
    }
}
