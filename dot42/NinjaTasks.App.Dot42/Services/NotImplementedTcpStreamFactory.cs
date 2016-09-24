using System;
using System.ComponentModel;
using NinjaTasks.Core.Services;
using NinjaTools.Connectivity.Connections;
using NinjaTools.Connectivity.Discover;

namespace NinjaTasks.App.Droid.Services
{
    public class NotImplementedTcpStreamFactory : ITcpStreamFactory
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public string ServiceName { get { return "(Not Implemented)"; } }
        
        public IStreamConnector GetConnector(RemoteDeviceInfo endpoint)
        {
            throw new NotImplementedException();
        }

        public IStreamListener GetListener(RemoteDeviceInfo endpoint)
        {
            throw new NotImplementedException();
        }

        public bool IsActivated { get { return false; } }
        public bool IsAvailableOnDevice { get { return false; } }
    }
}
