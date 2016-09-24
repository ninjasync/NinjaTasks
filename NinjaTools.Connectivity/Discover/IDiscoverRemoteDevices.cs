using System;
using NinjaTools.Connectivity.Connections;

namespace NinjaTools.Connectivity.Discover
{
    public interface IDiscoverRemoteDevices : IDisposable
    {
        bool IsServiceEnabled { get;  }
        bool IsScanning { get;  }

        IScanContext Scan(Action<RemoteDeviceInfo> deviceFound);
        IStreamConnector Create(RemoteDeviceInfo deviceInfo);

        event EventHandler ServiceEnabledChanged;
    }
}