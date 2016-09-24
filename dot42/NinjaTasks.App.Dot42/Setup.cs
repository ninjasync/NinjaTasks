using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Android.Content;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Plugins;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.Plugins.Visibility.Dot42;
using Cirrious.MvvmCross.ViewModels;
using NinjaSync.MasterSlave;
using NinjaTasks.App.Droid.AndroidServices;
using NinjaTasks.App.Droid.Services;
using NinjaTasks.App.Droid.Views;
using NinjaTasks.App.Droid.Views.CustomBindings;
using NinjaTasks.Core.Services;
using NinjaTasks.Core.Services.Server;
using NinjaTasks.Core.ViewModels.Sync;
using NinjaTasks.Db.MvxSqlite;
using NinjaTasks.Model.Storage;
using NinjaTools.Connectivity.ViewModels.ViewModels;
using NinjaTools.Droid;
using NinjaTools.Droid.Services;
using NinjaTools.MVVM.Services;
using PluginLoader = Cirrious.MvvmCross.Plugins.Messenger.PluginLoader;

namespace NinjaTasks.App.Droid
{
    public class MessengerPluginBootstrap : MvxPluginBootstrapAction<PluginLoader> { }
    public class MethodBindingPluginBootstrap : MvxPluginBootstrapAction<Cirrious.MvvmCross.Plugins.MethodBinding.PluginLoader> { }

    public class VisibilityPluginDot42Bootstrap : MvxLoaderPluginBootstrapAction<Cirrious.MvvmCross.Plugins.Visibility.PluginLoader, Plugin> { }
    public class SqlitePluginDot42Bootstrap : MvxLoaderPluginBootstrapAction<Cirrious.MvvmCross.Community.Plugins.Sqlite.PluginLoader, Cirrious.MvvmCross.Community.Plugins.Sqlite.Dot42.Plugin> { }

    public class Setup : MvxAndroidSetup
    {
        private ISyncManager _syncManager;
        private AccountCreationManager _accountsCreator;
        private SyncOnDataChangedManager _syncOnDatabaseChange;

        public Setup(Context applicationContext) : base(applicationContext)
        {
            UnhandledExceptionHandlers.Initialize(applicationContext);
            NinjaToolsLoggerToLogCat.Register();    
        }
      
        protected override IMvxApplication CreateApp()
        {
            //Task.Run(() =>
            //{
            //    new TestSync().TestSyncToAndFroSync("LocalToP2P_Pipes");
            //    Log.Info("after TestSyncToAndFroSync");
            //    //new TestSync().TestSyncToAndFroSync("LocalToP2P_Direct");
            //});
            //Log.Warn("in main thread");

            //new TestSync().TestSyncToAndFroSync("LocalToP2P_Direct");
            //Log.Warn("after TestSyncToAndFroSync - direct");

            //new TestSync().TestSyncToAndFroSync("LocalToP2P_Pipes");
            //Log.Error("XXXXXXXXXXXXXXXXXXXXXXXXXXX");
            //Log.Error("XXXXXXXXXXXXXXXXXXXXXXXXXXX");
            //new TestSync().TestFirstTimeSyncUploadLocals("LocalToP2P_Pipes");
            //Log.Error("XXXXXXXXXXXXXXXXXXXXXXXXXXX");
            //Log.Error("XXXXXXXXXXXXXXXXXXXXXXXXXXX");
            //new TestSync().TestMergeConflictsByModificationDate("LocalToP2P_Pipes");
            ////Log.Error("XXXXXXXXXXXXXXXXXXXXXXXXXXX");
            ////Log.Error("XXXXXXXXXXXXXXXXXXXXXXXXXXX");
            ////new TestSync().TestProtectIntermediateChanges("LocalToP2P_Pipes");
            //Log.Error("XXXXXXXXXXXXXXXXXXXXXXXXXXX");
            //Log.Error("XXXXXXXXXXXXXXXXXXXXXXXXXXX");
            //new TestSync().TestOnlyUploadChangesAfterNewSync("LocalToP2P_Pipes");
            //Log.Error("XXXXXXXXXXXXXXXXXXXXXXXXXXX");
            //Log.Error("XXXXXXXXXXXXXXXXXXXXXXXXXXX");

            //Log.Warn("after TestSyncToAndFroSync- pipes");

            //var app = new NinjaTasks.Core.App();
            //return new SyncApp();
            return new Core.App();
        }

        //protected override IMvxAndroidViewPresenter CreateViewPresenter()
        //{
        //    var presenter = Mvx.IocConstruct<DroidPresenter>();
        //    Mvx.RegisterSingleton<IMvxAndroidViewPresenter>(presenter);

        //    return presenter;
        //}

        protected override Assembly[] GetViewModelAssemblies()
        {
            return base.GetViewModelAssemblies()
                    .Concat(new[]
                    {
                        typeof(TaskWarriorAccountViewModel).Assembly,
                        typeof(SelectRemoteDeviceViewModel).Assembly,
                    })
                    .Distinct()
                    .ToArray();
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();

            AndroidForegroundSyncMangerService.Initialize("NinjaTasks", "P2P sync in background",
                                    R.Drawable.ic_launcher, typeof(ConfigureAccountsView));

            //IMvxMessenger m = Mvx.GetSingleton<IMvxMessenger>();
            //_keep += m.SubscribeOnMainThread<TaskModifiedMessage>(OnTaskModified);

            //_accountsCreator = Mvx.IocConstruct<AccountCreationManager>();
            //_syncManager = Mvx.GetSingleton<ISyncManager>();
            //_syncManager.IsEnabled = true;

            //_bluetoothSyncServer = Mvx.GetSingleton<BluetoothSyncServerManager>();
            //_tcpIpSyncServer = Mvx.GetSingleton<TcpIpSyncServerManager>();

            _syncOnDatabaseChange = Mvx.IocConstruct<SyncOnDataChangedManager>();
        }

        protected override void InitializeIoC()
        {
            base.InitializeIoC();
            
            Mvx.RegisterSingleton(typeof(Context), ApplicationContext);
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

            Mvx.RegisterSingleton(() => new SQLiteFactory(Mvx.Resolve<ISQLiteConnectionFactoryEx>()));
            Mvx.RegisterSingleton<ISQLiteConnection>(() => Mvx.Resolve<SQLiteFactory>().Get("gui"));
            Mvx.LazyConstructAndRegisterSingleton<ITodoStorage>(Mvx.IocConstruct<MvxSqliteTodoStorage>);

            //Mvx.LazyConstructAndRegisterSingleton(Mvx.IocConstruct<AndroidAccountsStorageService>);
            //Mvx.LazyConstructAndRegisterSingleton<ISyncManager>(Mvx.IocConstruct<AndroidSyncManagerService>);
        }

        protected override void InitializeBindingBuilder()
        {
            base.InitializeBindingBuilder();

            var registry = Mvx.Resolve<IMvxTargetBindingFactoryRegistry>();
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
                toReturn["My"] = "ninjaTasks_App_Dot42.NinjaTasks.App.Droid.Views.Controls";
                return toReturn;
            }
        }
    }
}
