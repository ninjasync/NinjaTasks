using System.IO;
using System.Threading.Tasks;

namespace NinjaTools.Connectivity.Connections
{
    public interface IStreamConnector 
    {
        Task<Stream> ConnectAsync();

        /// <summary>
        /// returns true if a connection can be established.
        /// </summary>
        bool IsAvailable { get; }
    }
}