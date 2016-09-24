using System.ComponentModel;
using NinjaTools.Connectivity.Discover;

namespace NinjaTools.Connectivity.ViewModels.ViewModels
{
    public class RemoteDeviceViewModel : INotifyPropertyChanged
    {
        public RemoteDeviceInfo Device { get; private set; }
        public string Name { get { return Device.Name; } }
        public string Address { get { return Device.Address; } }

        public RemoteDeviceViewModel(RemoteDeviceInfo dev)
        {
            Device = dev;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}