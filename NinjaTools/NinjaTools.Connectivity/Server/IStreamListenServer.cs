using System;
using System.Threading.Tasks;

namespace NinjaTools.Connectivity.Server
{
    public interface IStreamListenServer : IDisposable
    {
        bool IsActive { get; }
        bool IsListening { get; }

        bool HasListeningErrors { get; }
        string LastError { get;  }
        
        void StartServing();
        Task StopServing();
    }
}