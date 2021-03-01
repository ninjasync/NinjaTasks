using System.ComponentModel;
using NinjaTools.Connectivity.Discover;

namespace NinjaTools.Connectivity.ViewModels.ViewModels
{
    public class RemoteDeviceViewModel : INotifyPropertyChanged
    {
        public Endpoint Device { get; private set; }
        public string Name { get { return Device.Name; } }
        public string Address { get { return Device.Address; } }

        public RemoteDeviceViewModel(Endpoint dev)
        {
            Device = dev;
        }

        #pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore CS0067
    }
}