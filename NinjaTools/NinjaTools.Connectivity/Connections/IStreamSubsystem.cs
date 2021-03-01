using System;
using System.ComponentModel;
using NinjaTools.Connectivity.Discover;

namespace NinjaTools.Connectivity.Connections
{
    public interface IStreamSubsystem : INotifyPropertyChanged, IDisposable
    {
        string ServiceName { get; }

        IStreamConnector GetConnector(Endpoint endpoint);
        IStreamListener GetListener(Endpoint endpoint);

        bool IsActivated { get; }
        bool IsAvailableOnDevice { get; }
    }
}
