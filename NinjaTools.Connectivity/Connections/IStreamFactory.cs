using System.ComponentModel;
using NinjaTools.Connectivity.Discover;

namespace NinjaTools.Connectivity.Connections
{
    public interface IStreamFactory : INotifyPropertyChanged
    {
        string ServiceName { get; }

        IStreamConnector GetConnector(RemoteDeviceInfo endpoint);
        IStreamListener GetListener(RemoteDeviceInfo endpoint);

        bool IsActivated { get; }
        bool IsAvailableOnDevice { get; }

        
    }

    public static class StreamFactoryProperties
    {
        public const string IsActivated = "IsActivated";
        public const string AvailableOnDevice = "IsAvailableOnDevice";
    }
}
