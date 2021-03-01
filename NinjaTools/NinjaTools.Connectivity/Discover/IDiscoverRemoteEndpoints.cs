using System;
using NinjaTools.Connectivity.Connections;

namespace NinjaTools.Connectivity.Discover
{
    public interface IDiscoverRemoteEndpoints : IDisposable
    {
        bool IsServiceEnabled { get;  }
        bool IsScanning { get;  }

        IScanContext Scan(Action<Endpoint> deviceFound);
        IStreamConnector Create(Endpoint deviceInfo);

        event EventHandler ServiceEnabledChanged;
    }
}