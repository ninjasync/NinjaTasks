using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Android.Content;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
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
using NinjaTools.MVVM.Services;

namespace NinjaTasks.App.Droid
{
    public class Setup : MvxAndroidSetup
    {
        private SyncServerManager _bluetoothManager;
        private SyncManager _syncManager;

        public Setup(Context applicationContext) : base(applicationContext)
        {
            UnhandledExceptionHandlers.Initialize(applicationContext);

#if DEBUG
            Trace.Listeners.Add(new DebugBreakTraceListener());
#endif

            NinjaToolsLoggerToLogCat.Register();    
        }

        protected override IMvxApplication CreateApp()
        {
            //var app = new NinjaTasks.Core.App();
            return new SyncApp();
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
                        typeof(SelectRemoteDeviceViewModel).Assembly
                    })
                    .ToArray();
        }

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();

            _bluetoothManager = Mvx.IocConstruct<BluetoothSyncServerManager>();

            AndroidForegroundSyncMangerService.Initialize("NinjaTasks", "P2P sync in background",
                                    Resource.Drawable.ic_launcher, typeof(ConfigureAccountsView));
           
            //Mvx.RegisterType<ITaskWarriorSyncService, TaskWarriorSyncService>();
        }

        protected override void InitializeIoC()
        {
            base.InitializeIoC();
            
            Mvx.RegisterSingleton(typeof(Context), ApplicationContext);
            //Mvx.ConstructAndRegisterSingleton<IFragmentTypeLookup, FragmentTypeLookup>();

            var assemlies = new[] {
                    typeof (SyncBetweenSlaveAndMasterService).Assembly,
                    typeof (ShowMessagesService).Assembly,
                    typeof (DisplayMessageService).Assembly, 
                    typeof (MvxSqliteTodoStorage).Assembly,
                    typeof (IWeakTimerService).Assembly,
                    typeof (SyncServerManager).Assembly,
                    GetType().Assembly
                }
                .Distinct()
                .ToList();

            foreach (var assembly in assemlies)
                NinjaTasks.Core.App.RegisterTypesWithIoC(CreatableTypes(assembly));

            Mvx.RegisterSingleton(() => new SQLiteFactory(Mvx.Resolve<ISQLiteConnectionFactoryEx>(), "ninjatasks.sqlite"));
            
            //Mvx.LazyConstructAndRegisterSingleton(Mvx.IocConstruct<AndroidAccountsStorageService>);
            //Mvx.LazyConstructAndRegisterSingleton<ISyncManager>(Mvx.IocConstruct<AndroidSyncManagerService>);

          

        }
    }
}
