using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTools.Connectivity.Connections
{
    public interface IStreamConnector : IDisposable
    {
        Task<Stream> ConnectAsync(CancellationToken cancel = default);

        /// <summary>
        /// returns true if a connection can be established.
        /// </summary>
        bool IsAvailable { get; }
    }
}