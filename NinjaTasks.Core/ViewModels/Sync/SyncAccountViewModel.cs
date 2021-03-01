using System;
using System.Collections.Generic;
using MvvmCross.Plugin.Messenger;
using NinjaTasks.Core.Services;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity.ViewModels.Messages;
using NinjaTools.Connectivity.ViewModels.ViewModels;

namespace NinjaTasks.Core.ViewModels.Sync
{
    public class SyncAccountViewModel : RemoteDeviceViewModel
    {
        private readonly IAccountsStorage _storage;
        private readonly ISyncManager _sync;
        private readonly IMvxMessenger _messenger;
        public DateTime LastSync { get { return Account.LastSuccessfulSync; } }
        public SyncAccount Account { get; private set; }

        public bool IsSyncActive { get; internal set; }
        public string LastSyncError { get { return Account.LastSyncError; } }

        

        public SyncAccountViewModel(SyncAccount account, 
                                    IAccountsStorage storage, 
                                    ISyncManager sync,
                                    IMvxMessenger messenger)
                                :base(new Endpoint(EndpointType.Unknown, account.Name, account.Address))
        {
            _storage = storage;
            _sync = sync;
            _messenger = messenger;
            Account = account;
        }

        private readonly IList<TimeSpan> _supportedTimespans = new []
        {
             TimeSpan.Zero,
             TimeSpan.FromMinutes(2),
             TimeSpan.FromMinutes(5),
             TimeSpan.FromMinutes(10),
             TimeSpan.FromMinutes(20),
             TimeSpan.FromMinutes(30),
             TimeSpan.FromMinutes(60),
             TimeSpan.FromMinutes(120),
        };

        public int UpdateInterval
        {
            get
            {
                if (Account.SyncInterval == default(TimeSpan)) return 0;
                int idx = 0;
                foreach (var timeSpan in _supportedTimespans)
                {
                    if (Account.SyncInterval <= timeSpan)
                        return idx;
                    ++idx;
                }
                return _supportedTimespans.Count - 1;
            }
            set
            {
                if (value <= 0) 
                    Account.SyncInterval = TimeSpan.Zero;
                else
                    Account.SyncInterval = _supportedTimespans[value];
                
                _storage.SaveAccount(Account, nameof(Account.SyncInterval));
            }
        }

        public bool IsSyncOnDataChanged
        {
            get { return Account.IsSyncOnDataChanged; }
            set
            {
                Account.IsSyncOnDataChanged = value;

                _storage.SaveAccount(Account, nameof(Account.IsSyncOnDataChanged));
            }
        }

        public void Remove()
        {
            _storage.Delete(Account);
            _sync.RefreshAccounts();
            _messenger.Publish(new RemoteDeviceSelectedMessage(this, null, null));
        }

        public void SyncAccount()
        {
            _sync.SyncNowAsync(Account);
        }
    }
}