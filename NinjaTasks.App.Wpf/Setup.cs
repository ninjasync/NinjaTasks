using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Windows.Threading;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Plugins;
using Cirrious.MvvmCross.BindingEx.WindowsShared;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Wpf.Platform;
using Cirrious.MvvmCross.Wpf.Views;
using NinjaTasks.App.Wpf.MvvmCross;
using NinjaTasks.Core.Messages;
using NinjaTasks.Core.Services;
using NinjaTasks.Core.Services.Server;
using NinjaTasks.Db.MvxSqlite;
using NinjaTasks.Model.Storage;
using NinjaTasks.Sync.ImportExport;
using NinjaTools;
using NinjaTools.Connectivity.ViewModels.ViewModels;
using NinjaTools.GUI.Wpf.Services;
using NinjaTools.Logging;
using NinjaTools.MVVM.Services;
using NinjaTools.GUI.Wpf.Utils;

namespace NinjaTasks.App.Wpf
{
    public class SqlitePluginBootstrap
        : MvxPluginBootstrapAction<Cirrious.MvvmCross.Community.Plugins.Sqlite.PluginLoader>
    {
        public SqlitePluginBootstrap()
        {
            string sqlitebin = Path.GetFullPath(Environment.Is64BitProcess ? "x64" : "x86");
            Environment.SetEnvironmentVariable("PATH",
                    sqlitebin + ";" + Environment.GetEnvironmentVariable("PATH"));
        }
    }

    public class MessengerPluginBootstrap
     : MvxPluginBootstrapAction<Cirrious.MvvmCross.Plugins.Messenger.PluginLoader>
    {
    }

    public class Setup : MvxWpfSetup
    {
        private TokenBag _keep = new TokenBag();
        private AccountCreationManager _accountsCreator;
        private ISyncManager _syncManager;

        private BluetoothSyncServerManager _bluetoothSyncServer;
        private TcpIpSyncServerManager  _tcpIpSyncServer;

        private SyncOnDataChangedManager _syncOnDatabaseChange;

        public Setup(Dispatcher dispatcher, IMvxWpfViewPresenter presenter)
            : base(dispatcher, presenter)
        {
            NinjaTools2NLog.Register();
        }

        protected override IMvxNameMapping CreateViewToViewModelNaming()
        {
            return new MvxPostfixAwareViewToViewModelNameMapping("NativeView", "View", "Control", "Dlg");
        }

        protected override IMvxWpfViewsContainer CreateWpfViewsContainer()
        {
            var ret = new MvxToCaliburnMicroWpfViewsContainer();
            Mvx.RegisterSingleton<IMvxViewFinder>(ret);
            return ret;
        }

        protected override void InitializeIoC()
        {
            base.InitializeIoC();

            var assemlies = new[] {
                    typeof (ImportExportFactory).Assembly,
                    typeof (ShowMessagesService).Assembly,
                    typeof (DisplayMessageService).Assembly, 
                    typeof (MvxSqliteTodoStorage).Assembly,
                    typeof (IWeakTimerService).Assembly,
                    typeof (SyncManager).Assembly,
                    GetType().Assembly
                }
               .Distinct()
               .ToList();

            foreach (var assembly in assemlies)
                Core.App.RegisterTypesWithIoC(assembly);

            Mvx.RegisterSingleton(() => new SQLiteFactory(Mvx.Resolve<ISQLiteConnectionFactoryEx>(), DatabasePath));
            Mvx.RegisterSingleton<ISQLiteConnection>(()=>Mvx.Resolve<SQLiteFactory>().Get("gui"));
            Mvx.LazyConstructAndRegisterSingleton<ITodoStorage>(Mvx.IocConstruct<MvxSqliteTodoStorage>);
            //Mvx.LazyConstructAndRegisterSingleton<ISyncService, SynchronizationService>();

            MvxWindowsBindingBuilder bld = new MvxWindowsBindingBuilder();
            bld.DoRegistration();
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();
           
            // initialize caliburn.
            ShortcutParser.InitializeShortcuts();

            IMvxMessenger m = Mvx.GetSingleton<IMvxMessenger>();
            _keep += m.SubscribeOnMainThread<TaskModifiedMessage>(OnTaskModified);


            _accountsCreator = Mvx.IocConstruct<AccountCreationManager>();
            _syncManager = Mvx.GetSingleton<ISyncManager>();
            _syncManager.IsEnabled = true;

            _bluetoothSyncServer = Mvx.GetSingleton<BluetoothSyncServerManager>();
            _tcpIpSyncServer = Mvx.GetSingleton<TcpIpSyncServerManager>();

            _syncOnDatabaseChange = Mvx.IocConstruct<SyncOnDataChangedManager>();

            //new P2PSyncServer()
        }

       private void OnTaskModified(TaskModifiedMessage obj)
        {
            if (obj.Mod == ModificationTyp.Status && obj.Task.IsCompleted)
            {
                (new SoundPlayer(@"Sounds\DKLAND.WAV")).Play();
            }
        }

        public string DatabasePath
        {
            get
            {
                const string publisher = "Ninja";
                const string appname = "NinjaTasks";
                string dir;

                if (Directory.Exists("portable-data"))
                    dir = "portable-data";
                else
                {
                    dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), publisher,
                        appname);
                    Directory.CreateDirectory(dir);
                }

                return Path.GetFullPath(Path.Combine(dir, appname + ".sqlite"));

            }
        }

        protected override Assembly[] GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies()
                       .Concat(new[]{
                                        typeof (SelectRemoteDeviceViewModel).Assembly ,
                                    })
                       .Distinct()
                       .ToArray();
        }

        /// <summary>
        /// Creates the app.
        /// </summary>
        /// <returns>An instance of MvxApplication</returns>
        protected override IMvxApplication CreateApp()
        {
            return new NinjaTasks.Core.App();
        }
    }
}
