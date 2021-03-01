using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Android.Content;
using NinjaTools.Sqlite;
using NinjaSync.MasterSlave;
using NinjaTasks.App.Droid.AndroidServices;
using NinjaTasks.App.Droid.Services;
using NinjaTasks.App.Droid.Views;
using NinjaTasks.Core.Services;
using NinjaTasks.Core.Services.Server;
using NinjaTasks.Core.ViewModels.Sync;
using NinjaTasks.Db.MvxSqlite;
using NinjaTools.Connectivity.ViewModels.ViewModels;
using NinjaTools.Droid;
using NinjaTools.Droid.Services;
using NinjaTools.GUI.MVVM.Services;
using NinjaTasks.Model.Storage;
using AndroidX.DrawerLayout.Widget;
using NinjaTasks.App.Droid.Views.Controls;
using MvvmCross.Binding.Bindings.Target.Construction;
using NinjaTasks.App.Droid.Views.CustomBindings;
using System.Collections.Generic;
using MvvmCross.Platforms.Android.Core;
using NinjaTools.Droid.Logging;
using MvvmCross.ViewModels;
using MvvmCross;
using NinjaTools.Connectivity.Discover;
using NinjaTools.Connectivity;
using NinjaTools.Connectivity.SystemNet;
using NinjaTasks.App.Droid.MvvmCross;
using MvvmCross.Binding;
using NinjaTools.Sqlite.SqliteNetPCL;
using AndroidX.SwipeRefreshLayout.Widget;

namespace NinjaTasks.App.Droid
{
    public class Setup : MvxAndroidSetup
    {
        private SyncServerManager _bluetoothManager;
        //private SyncManager _syncManager;
        private SyncOnDataChangedManager _syncOnDatabaseChange;


        public Setup() : base()
        {
            NinjaToolsLoggerToLogCat.Register();
            UnhandledExceptionHandlers.Initialize(ApplicationContext);

#if DEBUG
            Trace.Listeners.Add(new DebugBreakTraceListener());
#endif

        }

        protected override IMvxApplication CreateApp()
        {
            return new NinjaTasks.Core.App();
        }

        //protected override IMvxAndroidViewPresenter CreateViewPresenter()
        //{
        //    var presenter = Mvx.IoCProvider.IoCConstruct<DroidPresenter>();
        //    Mvx.IoCProvider.RegisterSingleton<IMvxAndroidViewPresenter>(presenter);

        //    return presenter;
        //}

        protected override MvxBindingBuilder CreateBindingBuilder()
        {
            return new MyBindingBuilder();
        }

        public override IEnumerable<Assembly> GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies()
                    .Concat(new[]
                    {
                        typeof(TaskWarriorAccountViewModel).Assembly,
                        typeof(SelectRemoteDeviceViewModel).Assembly
                    })
                    .Distinct()
                    .ToArray();
        }

        public override IEnumerable<Assembly> GetViewAssemblies()
        {
            return base.GetViewAssemblies()
                   .Concat(new[]
                   {
                       typeof(SwipeRefreshLayout).Assembly,
                       typeof(ClickThroughDrawerLayout).Assembly,
                   })
                   .Distinct()
                   .ToArray();
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();

            SQLitePCL.Batteries_V2.Init();

            _bluetoothManager = Mvx.IoCProvider.IoCConstruct<BluetoothSyncServerManager>();

            AndroidForegroundSyncMangerService.Initialize("NinjaTasks", "P2P sync in background",
                                    Resource.Drawable.ic_launcher, typeof(ConfigureAccountsView));

            //Mvx.RegisterType<ITaskWarriorSyncService, TaskWarriorSyncService>();

            //IMvxMessenger m = Mvx.GetSingleton<IMvxMessenger>();
            //_keep += m.SubscribeOnMainThread<TaskModifiedMessage>(OnTaskModified);

            //_accountsCreator = Mvx.IoCProvider.IoCConstruct<AccountCreationManager>();
            //_syncManager = Mvx.GetSingleton<ISyncManager>();
            //_syncManager.IsEnabled = true;

            //_bluetoothSyncServer = Mvx.GetSingleton<BluetoothSyncServerManager>();
            //_tcpIpSyncServer = Mvx.GetSingleton<TcpIpSyncServerManager>();

            _syncOnDatabaseChange = Mvx.IoCProvider.IoCConstruct<SyncOnDataChangedManager>();

        }

        protected override void InitializeFirstChance()
        {
            Mvx.IoCProvider.RegisterSingleton<IMvxTargetBindingFactoryRegistry>(() => new MyTargetBindingFactoryRegistry());

            base.InitializeFirstChance();

            Mvx.IoCProvider.RegisterSingleton(typeof(Context), ApplicationContext);
            //Mvx.ConstructAndRegisterSingleton<IFragmentTypeLookup, FragmentTypeLookup>();

            var assemlies = new[] {
                    //typeof (ImportExportFactory).Assembly,
                    typeof (SyncBetweenSlaveAndMasterService).Assembly,
                    typeof (ShowMessagesService).Assembly,
                    typeof (DisplayMessageService).Assembly,
                    typeof (MvxSqliteTodoStorage).Assembly,
                    typeof (IWeakTimerService).Assembly,
                    typeof (SyncServerManager).Assembly,
                    typeof (SyncManager).Assembly,
                    GetType().Assembly,
                }
                .Distinct()
                .ToList();

            foreach (var assembly in assemlies)
                Core.App.RegisterTypesWithIoC(assembly);


            Mvx.IoCProvider.RegisterSingleton<ISQLiteConnectionFactoryEx>( () => new SqLiteNetPCLConnectionFactory() );
            string filename = "ninjatasks.sqlite";
            Mvx.IoCProvider.RegisterSingleton(() => new SQLiteFactory(Mvx.IoCProvider.Resolve<ISQLiteConnectionFactoryEx>(), filename));
            Mvx.IoCProvider.RegisterSingleton<ISQLiteConnection>(() => Mvx.IoCProvider.Resolve<SQLiteFactory>().Get("gui"));
            Mvx.IoCProvider.RegisterSingleton<ITodoStorage>(Mvx.IoCProvider.IoCConstruct<MvxSqliteTodoStorage>);

            Mvx.IoCProvider.RegisterSingleton<IDiscoverBluetoothRemoteEndpoints>(
               () => new AndroidBluetoothDiscoverRemoteDevicesService(ApplicationContext));

            Mvx.IoCProvider.RegisterSingleton<IBluetoothStreamSubsystem>(() =>
                      Mvx.IoCProvider.IoCConstruct<AndroidBluetoothStreamSubsystem>());
            Mvx.IoCProvider.RegisterSingleton<ITcpStreamSubsystem>(() => Mvx.IoCProvider.IoCConstruct<TcpStreamSubsystem>());

            //Mvx.IoCProvider.LazyConstructAndRegisterSingleton(Mvx.IoCProvider.IoCConstruct<AndroidAccountsStorageService>);
            //Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ISyncManager>(Mvx.IoCProvider.IoCConstruct<AndroidSyncManagerService>);
        }

        //protected override IMvxAndroidViewPresenter CreateViewPresenter()
        //{
        //    var mvxFragmentsPresenter = new MvxFragmentsPresenter();           
        //    Mvx.IoCProvider.RegisterSingleton<IMvxAndroidViewPresenter>(mvxFragmentsPresenter);
        //    return mvxFragmentsPresenter;
        //}

        protected override void InitializeBindingBuilder()
        {
            base.InitializeBindingBuilder();

            var registry = Mvx.IoCProvider.Resolve<IMvxTargetBindingFactoryRegistry>();
            ImageViewImageResourceTargetBinding.Register(registry);
            TextViewImeActionBinding.Register(registry);
            SelectionCheckedListViewSelectedItemTargetBinding.Register(registry);
            TextViewPaintFlagsBinding.Register(registry);
        }

        protected override IDictionary<string, string> ViewNamespaceAbbreviations
        {
            get
            {
                var toReturn = base.ViewNamespaceAbbreviations;
                toReturn["My"] = "NinjaTasks.App.Droid.Views.Controls";
                return toReturn;
            }
        }
    }
}
