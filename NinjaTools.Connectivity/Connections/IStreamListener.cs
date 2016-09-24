using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaTools.Connectivity.Connections
{
    public interface IStreamListener : IDisposable
    {
        Task<Stream> ListenAsync(CancellationToken token);
    }
}
