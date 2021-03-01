using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using MvvmCross.Plugin.Messenger;
using NinjaTasks.Core.Messages;
using NinjaTasks.Core.Services;
using NinjaTasks.Core.Services.Server;
using NinjaTasks.Db.MvxSqlite;
using NinjaTasks.Model.Storage;
using NinjaTasks.Sync.ImportExport;
using NinjaTools;
using NinjaTools.GUI.Wpf.Services;
using NinjaTools.GUI.MVVM.Services;
using System.Collections.Generic;
using System.Windows;
using MvvmCross.Plugin.Share;
using MvvmCross.Platforms.Wpf.Core;
using MvvmCross.ViewModels;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.Views;
using MvvmCross;
using NinjaTools.Sqlite;
using NinjaTools.Connectivity.ViewModels.ViewModels;
using NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro;
using NinjaTools.Connectivity.Bluetooth._32Feet;
using NinjaTools.Connectivity;
using NinjaTools.Connectivity.SystemNet;
using NinjaTools.Connectivity.Discover;
using MvvmCross.Platforms.Wpf.Presenters;
using System.Windows.Controls;
using UnconventionalProxy.UI.Wpf.Views;
using NinjaTools.Sqlite.SqliteNetPCL;

namespace NinjaTasks.App.Wpf
{
    public class Setup : MvxWpfSetup
    {
        private TokenBag _keep = new TokenBag();
        private AccountCreationManager _accountsCreator;
        private ISyncManager _syncManager;

        private BluetoothSyncServerManager _bluetoothSyncServer;
        private TcpIpSyncServerManager  _tcpIpSyncServer;

        private SyncOnDataChangedManager _syncOnDatabaseChange;

        static Setup()
        {
            //string sqlitebin = Path.GetFullPath(Environment.Is64BitProcess ? "x64" : "x86");
            //Environment.SetEnvironmentVariable("PATH", sqlitebin + ";" + Environment.GetEnvironmentVariable("PATH"));
            //NinjaTools.Sqlite.MvxBaseSQLiteConnectionFactory(sqlitebin);
        }

        protected override IMvxNameMapping CreateViewToViewModelNaming()
        {
            return new MvxPostfixAwareViewToViewModelNameMapping("NativeView", "View", "Control", "Dlg");
        }

        protected override IMvxWpfViewsContainer CreateWpfViewsContainer()
        {
            var ret = new MvxToCaliburnMicroWpfViewsContainer();
            ret.Add<SelectRemoteDeviceViewModel, SelectBluetoothRemoteDeviceNativeView>();

            Mvx.IoCProvider.RegisterSingleton<IMvxViewFinder>(ret);
            Mvx.IoCProvider.RegisterSingleton<IMvxViewsContainer>(ret);
            
            return ret;
        }
        
        protected override IMvxWpfViewPresenter CreateViewPresenter(ContentControl root)
        {
            return new MyWpfPresenter(root);
        }

        protected override void InitializeFirstChance()
        {
            NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro.SetupCaliburn.SetupIoC();

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

            var disoverBluetooth = new BluetoothDiscoverRemoteDevicesService(SqliteSyncServiceFactory.BluetoothGuid);
            Mvx.IoCProvider.RegisterSingleton<IDiscoverRemoteEndpoints>(disoverBluetooth);
            Mvx.IoCProvider.RegisterSingleton<IDiscoverBluetoothRemoteEndpoints>(disoverBluetooth);


            Mvx.IoCProvider.RegisterSingleton<IBluetoothStreamSubsystem>(Mvx.IoCProvider.IoCConstruct<BluetoothStreamSubsystem>);
            Mvx.IoCProvider.RegisterSingleton<ITcpStreamSubsystem>(() => Mvx.IoCProvider.IoCConstruct<TcpStreamSubsystem>());

            Mvx.IoCProvider.RegisterSingleton(Mvx.IoCProvider.IoCConstruct<BluetoothSyncServerManager>);
            Mvx.IoCProvider.RegisterSingleton(Mvx.IoCProvider.IoCConstruct<TcpIpSyncServerManager>);
            Mvx.IoCProvider.RegisterSingleton(Mvx.IoCProvider.IoCConstruct<SyncOnDataChangedManager>);

            Mvx.IoCProvider.RegisterSingleton<ISQLiteConnectionFactoryEx>(Mvx.IoCProvider.IoCConstruct<SqLiteNetPCLConnectionFactory>);
            Mvx.IoCProvider.RegisterSingleton(() => new SQLiteFactory(Mvx.IoCProvider.Resolve<ISQLiteConnectionFactoryEx>(), DatabasePath, UseDatabaseEncryption));
            Mvx.IoCProvider.RegisterSingleton<ISQLiteConnection>(()=>Mvx.IoCProvider.Resolve<SQLiteFactory>().Get("gui"));
            Mvx.IoCProvider.RegisterSingleton<ITodoStorage>(Mvx.IoCProvider.IoCConstruct<MvxSqliteTodoStorage>);
            //Mvx.LazyConstructAndRegisterSingleton<ISyncService, SynchronizationService>();
            Mvx.IoCProvider.RegisterSingleton<IMvxShareTask>(() => new ShareToClipboard());


            base.InitializeFirstChance();

        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();

            SQLitePCL.Batteries.Init();

            // initialize caliburn.
            NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro.ShortcutParser.InitializeShortcuts();

            IMvxMessenger m = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
            _keep += m.SubscribeOnMainThread<TaskModifiedMessage>(OnTaskModified);


            _accountsCreator       = Mvx.IoCProvider.IoCConstruct<AccountCreationManager>();
            _syncManager           = Mvx.IoCProvider.GetSingleton<ISyncManager>();
            _syncManager.IsEnabled = true;

            _bluetoothSyncServer   = Mvx.IoCProvider.GetSingleton<BluetoothSyncServerManager>();
            _tcpIpSyncServer       = Mvx.IoCProvider.GetSingleton<TcpIpSyncServerManager>();

            _syncOnDatabaseChange  = Mvx.IoCProvider.GetSingleton<SyncOnDataChangedManager>();

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
                    string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    dir = Path.Combine(appdata, publisher, appname);
                    Directory.CreateDirectory(dir);
                }

                string filename = UseDatabaseEncryption ? "config" : (appname + ".sqlite");
                return Path.GetFullPath(Path.Combine(dir, filename));
            }
        }

        public bool UseDatabaseEncryption
        {
            get => Directory.Exists("empty");
        }

        public override IEnumerable<Assembly> GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies()
                       .Concat(new[]{
                                        typeof (SelectRemoteDeviceViewModel).Assembly,
                                        //GetType().Assembly,
                                    })
                       .Distinct();
        }

        protected override IMvxApplication CreateApp()
        {
            return new NinjaTasks.Core.App();
        }
    }

    public class ShareToClipboard : IMvxShareTask
    {
        public void ShareShort(string message)
        {
            Clipboard.SetText(message);
        }

        public void ShareLink(string title, string message, string link)
        {
            Clipboard.SetText(link);
        }
    }
}
