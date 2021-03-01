using System.Linq;
using MvvmCross.Plugin.Messenger;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTools;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.ViewModels.Messages;

namespace NinjaTasks.Core.Services
{
    public class AccountCreationManager 
    {
        private readonly IAccountsStorage _storage;
        private readonly ISyncManager _syncManager;
        private readonly TokenBag _bag = new TokenBag();

        public AccountCreationManager(IMvxMessenger messenger, IAccountsStorage storage, ISyncManager syncManager)
        {
            _storage = storage;
            _syncManager = syncManager;
            _bag += messenger.SubscribeOnMainThread<RemoteDeviceSelectedMessage>(OnDeviceSelected);
        }

        public void OnDeviceSelected(RemoteDeviceSelectedMessage obj)
        {
            if (obj.Device == null) return;

            var destType = obj.Device.DeviceType == EndpointType.Bluetooth?SyncAccountType.BluetoothP2P
                          :obj.Device.DeviceType == EndpointType.TcpIp?SyncAccountType.TcpIpP2P
                          :SyncAccountType.Unknown;

            if (destType == SyncAccountType.Unknown)
            {
                // don't know what to do.
                return;
            }

            string address = obj.Device.Address;
            if (!obj.Device.Port.IsNullOrEmpty())
                address = address + ":" + obj.Device.Port;

            if (_storage.GetAccounts().Any(p => p.Address == address && p.Type == destType))
                return;

            SyncAccount s = new SyncAccount {Address = address, Name = obj.Device.Name};
            s.Type = destType;

            _storage.SaveAccount(s);
            _syncManager.RefreshAccounts();
        }
    }
}
