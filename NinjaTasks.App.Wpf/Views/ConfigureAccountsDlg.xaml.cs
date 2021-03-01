using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Plugin.Messenger;
using NinjaSync;
using NinjaTasks.Core.Services;
using NinjaTasks.Core.Services.Server;
using NinjaTasks.Core.ViewModels.Sync;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage.Mocks;
using NinjaTasks.Model.Sync;

namespace NinjaTasks.App.Wpf.Views
{
    public partial class ConfigureAccountsDlg 
    {
        public ConfigureAccountsDlg()
        {
            InitializeComponent();
        }
    }

    public class MockConfigureAccountsViewModel : ConfigureAccountsViewModel
    {
        public MockConfigureAccountsViewModel()
            : base(new MockAccountsStorage(), new MockSyncManager(), new MvxMessengerHub(), new MockNinjaTasksConfigurationService(), null, null, null)
        {
            OnActivate();
        }

        public class MockSyncServerManager : ISyncServerManager
        {
            #pragma warning disable CS0067
            public event PropertyChangedEventHandler PropertyChanged;
            #pragma warning restore CS0067

            public void Dispose()
            {
            }

            public DateTime LastSync { get; set; }
            public string LastError { get; private set; }
            public bool IsRunning { get; set; }
            public bool IsSyncActive { get; private set; }
            public bool IsAvailable { get { return true; } }
            public bool IsAvailableOnDevice { get { return true; } }
        }

        public class MockSyncManager : ISyncManager
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            #pragma warning disable CS0067
            public event PropertyChangedEventHandler PropertyChanged;
            #pragma warning restore CS0067

            public bool IsEnabled { get; set; }
            public int ActiveSyncs { get; private set; }


            public Task SyncNowAsync(SyncAccount account, CancellationToken cancel = new CancellationToken(), bool isManualSync = true)
            {
                throw new NotImplementedException();
            }

            public Task SyncNowAsync(CancellationToken cancel)
            {
                throw new NotImplementedException();
            }

            public Task SyncNowAsync(SyncAccount account, ISyncProgress progress, bool isManualSync)
            {
                throw new NotImplementedException();
            }

            public bool IsSyncActive(SyncAccount account)
            {
                return account.Id == 1;
            }

            public void RefreshAccounts()
            {
            }
        }

        
    }

    public class MockNinjaTasksConfigurationService : INinjaTasksConfigurationService
    {
        public MockNinjaTasksConfigurationService()
        {
            Cfg = new NinjaTasksConfiguration();
        }
        public NinjaTasksConfiguration Cfg { get; private set; }

        public void Save()
        {
        }

        public bool GetConfigValue(string name, Type type, object defaultVal, out object value)
        {
            // TODO: implement
            value = defaultVal;
            return false;
        }

        public void SetConfigValue(string name, Type type, object value)
        {
            // TODO: implement
        }
    }
}
