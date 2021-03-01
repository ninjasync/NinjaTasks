using System.Linq;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTasks.Sync.TaskWarrior;
using NinjaTools.Connectivity.ViewModels.Messages;
using NinjaTools.GUI.MVVM;

namespace NinjaTasks.Core.ViewModels.Sync
{
    public class TaskWarriorAccountViewModel : BaseViewModel
    {
        public TaskWarriorAccount Account { get; private set; }
        
        public int? ServerPort { get { return Account.ServerPort == 0 ? (int?) null : Account.ServerPort; } set{ Account.ServerPort = value ?? 0; }}

        private readonly ITaskWarriorAccountsStorage _storage;
        private readonly IMvxMessenger _messenger;
        private readonly IMvxNavigationService _nav;

        public TaskWarriorAccountViewModel(ITaskWarriorAccountsStorage storage,
                                           IMvxMessenger messenger,
                                           IMvxNavigationService nav)
        {
            _storage = storage;
            _messenger = messenger;
            _nav = nav;
        }

        public string ClientCertificateAndKeyPfxFile
        {
            get
            {
                return !string.IsNullOrEmpty(Account.ClientCertificateAndKeyPem)
                        ? "(set through config file)"
                        : Account.ClientCertificateAndKeyPfxFile;
                ;
            }
            set
            {
                if (value != null && value.StartsWith("(")) return;
                Account.ClientCertificateAndKeyPem = null;
                Account.ClientCertificateAndKeyPfxFile = value;
            }
        }

        public string ServerCertificateCrtFile
        {
            get
            {
                return !string.IsNullOrEmpty(Account.ServerCertificatePem)
                        ? "(set through config file)"
                        : Account.ServerCertificateCrtFile;
                ;
            }
            set
            {
                if (value != null && value.StartsWith("(")) return;
                Account.ServerCertificatePem = null;
                Account.ServerCertificateCrtFile = value;
            }
        }

        public void ImportTaskdConfig(string fileContents, bool importCertificates)
        {
            var cfg = TaskdConfigFile.Parse(fileContents);
            
            Account.Key = cfg.Key;
            Account.Org = cfg.Org;
            Account.ServerHostname = cfg.ServerHostname;
            Account.ServerPort = cfg.ServerPort;
            Account.User = cfg.Username;

            if (importCertificates)
            {
                // this is only supported on android.
                Account.ServerCertificatePem = cfg.RootCaCertificate;
                Account.ClientCertificateAndKeyPem = cfg.ClientCertificateAndKey;
            }
            RaiseAllPropertiesChanged();
        }

        public void Save()
        {
             _storage.SaveAccount(Account);
            _messenger.Publish(new RemoteDeviceSelectedMessage(this, null, null));
            _nav.Close(this);
        }

        public override void Start()
        {
            var account = _storage.GetTaskWarriorAccounts().FirstOrDefault();
            if (account == null)
                account = new TaskWarriorAccount();
            Account = account;
            base.Start();
        }
    }
}
