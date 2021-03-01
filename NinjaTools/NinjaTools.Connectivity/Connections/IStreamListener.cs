using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTools.Connectivity.Connections
{
    public interface IStreamListener : IDisposable
    {
        Task<Stream> ListenAsync(CancellationToken token = default);

        /// <summary>
        /// returns true if a listening is possible
        /// </summary>
        bool IsAvailable { get; }
    }
}
