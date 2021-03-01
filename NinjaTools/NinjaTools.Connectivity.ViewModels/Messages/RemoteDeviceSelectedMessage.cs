using MvvmCross.Plugin.Messenger;
using NinjaTools.Connectivity.Discover;

namespace NinjaTools.Connectivity.ViewModels.Messages
{
    public class RemoteDeviceSelectedMessage : MvxMessage
    {
        public string Id { get; set; }
        public Endpoint Device { get; set; }

        public RemoteDeviceSelectedMessage(object sender, string id, Endpoint device) 
            : base(sender)
        {
            Id = id;
            Device = device;
        }
    }
}
