using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTools.Connectivity.Streams
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads length bytes from the stream.
        /// </summary>
        public static async Task ReadBlockAsync(this Stream stream, byte[] buffer, int offset, int length, CancellationToken cancel = default(CancellationToken))
        {
            while (length > 0)
            {
                int read = await stream.ReadAsync(buffer, offset, length, cancel);
                cancel.ThrowIfCancellationRequested();
                if(read == 0)
                    throw new IOException("connection closed before block was read.");
                length -= read;
                offset += read;
            }
            return;
        }

        public static Task ReadBlockAsync(this Stream stream, byte[] buffer, CancellationToken cancel=default(CancellationToken))
        {
            return stream.ReadBlockAsync(buffer, 0, buffer.Length, cancel);
        }

        public static Task WriteAsync(this Stream stream, byte[] buffer, CancellationToken cancel)
        {
            return stream.WriteAsync(buffer, 0, buffer.Length, cancel);
        }

        public static Task WriteAsync(this Stream stream, byte[] buffer)
        {
            return stream.WriteAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// find the type/interface in the stream or any adapted base streams.
        /// </summary>
        public static T FindAdapted<T>(this Stream stream) where T : class
        {
            do
            {
                if (stream is T impl)
                    return impl;
                if (stream is IStreamAdapter adapter)
                    stream = adapter.BaseStream;
                else if (stream is CombinedStream combined)
                    return combined.ReadStream.FindAdapted<T>() ?? combined.WriteStream.FindAdapted<T>();
                else if (stream is BufferedStream buffered)
                    stream = TryGetBufferedUnderlyingStream(buffered); // some unclean reflection...
                else
                    return null;
            } 
            while (stream != null);
            return null;
        }

        // should work with MS and Mono implementations.
        private static Stream TryGetBufferedUnderlyingStream(BufferedStream buffered)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            return buffered.GetType().GetProperty("UnderlyingStream", bindingFlags)
                          ?.GetValue(buffered) as Stream
                ?? buffered.GetType().GetField("m_stream", bindingFlags)
                          ?.GetValue(buffered) as Stream
                ?? buffered.GetType().GetField("_stream", bindingFlags) // Fallback; MS's implementation should have been handled above.
                          ?.GetValue(buffered) as Stream
                ;
        }
    }
}
