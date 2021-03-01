using MvvmCross;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Plugin.Share;
using MvvmCross.ViewModels;
using NinjaTasks.Core.Messages;
using NinjaTasks.Core.Services;
using NinjaTasks.Core.ViewModels.Sync;
using NinjaTasks.Model.Storage;
using NinjaTools;
using NinjaTools.GUI.MVVM;
using NinjaTools.GUI.MVVM.Services;
using NinjaTools.Npc;

namespace NinjaTasks.Core.ViewModels
{
    public class AppViewModel : BaseViewModel
    {
        private readonly IShowMessageService _messageService;
        private readonly ISyncManager _syncManager;
        private readonly IMvxNavigationService _nav;

        public TodoListsViewModel Lists { get; set; }

        public ITasksViewModel SelectedList { get; private set; }
        public TasksSearchViewModel SearchViewModel { get; private set; }

        private ITasksViewModel _previousSelection;
        private readonly TokenBag _keep = new TokenBag();

        public bool IsSynching { get; private set; }

        public AppViewModel(ITodoStorage storage, 
                            IMvxMessenger messenger,
                            IShowMessageService messageService,
                            ISyncManager syncManager,
                            IMvxShareTask share,
                            IMvxNavigationService nav
            )
        {
            _messageService = messageService;
            _syncManager = syncManager;
            _nav = nav;
            Lists = Mvx.IoCProvider.IoCConstruct<TodoListsViewModel>();
            SearchViewModel = new TasksSearchViewModel(storage, Lists, messenger, share);
#if !DOT42
            Lists.Subscribe(l => l.SelectedList, OnListsSelectedListChanged);
            SearchViewModel.Subscribe(l => l.SearchText, OnSearchTextChanged);
#else
            Lists.Subscribe("SelectedList", OnListsSelectedListChanged);
            SearchViewModel.Subscribe("SearchText", OnSearchTextChanged);
#endif

            _keep += messenger.SubscribeOnMainThread<TrackableStoreModifiedMessage>(OnDatabaseModified);
            _keep += messenger.SubscribeOnMainThread<SyncFinishedMessage>(OnSyncMessage);

            IsSynching = syncManager.ActiveSyncs > 0;
        }

        

        private void OnSearchTextChanged()
        {
            if (!string.IsNullOrWhiteSpace(SearchViewModel.SearchText))
            {
                if (SelectedList != SearchViewModel)
                {
                    SelectedList = SearchViewModel;
                    if (Lists.SelectedList != null)
                        _previousSelection = Lists.SelectedList;
                    Lists.SelectedList = null;
                }
            }
            else
            {
                if (Lists.SelectedList == null)
                    Lists.SelectedList = _previousSelection;
                else
                    SelectedList = Lists.SelectedList;
            }
        }

        private void OnListsSelectedListChanged()
        {
            if (Lists.SelectedList == null) return;
            if (!string.IsNullOrWhiteSpace(SearchViewModel.SearchText))
                SearchViewModel.SearchText = "";
            else 
                SelectedList = Lists.SelectedList;
        }

        public void SetupSync()
        {
            _nav.Navigate<ConfigureAccountsViewModel>();
        }

        public void Import()
        {
            var ttlvm = SelectedList as TaskListViewModel;
            _nav.Navigate<ImportExportViewModel>(new
            {
                SelectedListId = ttlvm == null ? null : ttlvm.List.Id,
                IsExport = false
            });
        }

        public void Export()
        {
            var ttlvm = SelectedList as TaskListViewModel;

            _nav.Navigate<ImportExportViewModel>(new
            {
                SelectedListId = ttlvm == null ? null : ttlvm.List.Id,
                IsExport = true
            });
        }

        public void Sync()
        {
            IsSynching = true;
            _syncManager.SyncNowAsync();
        }

        private void OnDatabaseModified(TrackableStoreModifiedMessage obj)
        {
            if(obj.Source != ModificationSource.UserInterface)
                Lists.Reload();
        }
        
        private void OnSyncMessage(SyncFinishedMessage obj)
        {
            IsSynching = obj.TotalSyncActive > 0;
            if(obj.IsManualSync && !obj.SyncError.IsNullOrEmpty())
                _messageService.ShowError(obj.SyncError);

            if(obj.WasSuccesfullSync)
                Lists.Reload();
        }
        //private static Storages SetupStorages(SQLiteFactory factory)
        //{
        //    var storage = new Storages();
        //    storage.JournalStorage = new MvxSqliteJournalStorage(factory);
        //    storage.SyncStorage = new MvxSqliteTaskWarriorSyncStorageService(factory);
        //    storage.TodoStorage = new MvxSqliteTodoStorage(factory); ;
        //    return storage;
        //}

        //private async Task TestBackgroundOperation()
        //{
        //    using (var newFactory = _sqliteFactory.Clone())
        //    {
        //        var storage = new MvxSqliteTodoStorage(newFactory);
        //        int idx = 0;

        //        while (true)
        //        {
        //            await Task.Run(() =>
        //            {
        //                ++idx;
        //                ((IProgress) Progress).Progress = (idx%10)/10f;

        //                storage.RunInImmediateTransaction(() =>
        //                {
        //                    var task = storage.GetTasks().First();
        //                    for (int x = 0; x < 1000; ++x)
        //                        storage.SaveTask(task);
        //                    Task.Delay(100).Wait();
        //                });
        //            });
        //        }
        //    }
        //}
    }
}
