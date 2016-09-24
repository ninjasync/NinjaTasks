using Cirrious.MvvmCross.Plugins.Messenger;
using NinjaTools.Connectivity.Discover;

namespace NinjaTools.Connectivity.ViewModels.Messages
{
    public class RemoteDeviceSelectedMessage : MvxMessage
    {
        public string Id { get; set; }
        public RemoteDeviceInfo Device { get; set; }

        public RemoteDeviceSelectedMessage(object sender, string id, RemoteDeviceInfo device) 
            : base(sender)
        {
            Id = id;
            Device = device;
        }
    }
}
