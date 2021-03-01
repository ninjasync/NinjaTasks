using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Plugin.Share;
using NinjaSync.Storage;
using NinjaTasks.Core.Messages;
using NinjaTasks.Core.Reusable;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools;
using NinjaTools.GUI.MVVM;
using NinjaTools.GUI.MVVM.Services;
using NinjaTools.Npc;

namespace NinjaTasks.Core.ViewModels
{
    public class TodoListsViewModel : BaseViewModel, IActivate
    {
        private readonly ITodoStorage _storage;
        private readonly IShowMessageService _message;
        private readonly IMvxMessenger _messenger;
        private readonly IMvxShareTask _share;
        private TokenBag _keepSaveOnChanges = new TokenBag();

        public ObservableCollection<ITasksViewModel> Lists { get; private set; }

        public ITasksViewModel SelectedList { get; set; }

        private int SelectedListId { get; set; }

        public TodoListsViewModel(ITodoStorage storage, IShowMessageService message, 
                                  IMvxMessenger messenger, IMvxShareTask share)
        {
            _storage = storage;
            this._message = message;
            _messenger = messenger;
            _share = share;
#if !DOT42
            AddToAutoBundling(()=>SelectedListId);
#else
            AddToAutoBundling("SelectedListId");
#endif
            Lists = new ObservableCollection<ITasksViewModel>();
        }

        public void AddList()
        {
            var listvm = AddList("New List");
            SelectedList = listvm;
            listvm.IsEditingDescription = true;
        }

        public TaskListViewModel AddList(string desciption)
        {
            var taskList = new TodoListWithCount
            {
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Description = desciption,
                SortPosition = Lists.Count,
            };

            var listvm = new TaskListViewModel(taskList, _storage, _messenger, _share);

            _storage.Save(taskList);

            Lists.Add(listvm);
            RegisterSaveOnChanges(listvm);
            return listvm;
        }


        public void ToggleEditDescription()
        {
            var tvm = SelectedList as TaskListViewModel;
            if (tvm != null)
                tvm.IsEditingDescription = !tvm.IsEditingDescription;
        }

        public void RenameSelectedList()
        {
            var tvm = SelectedList as TaskListViewModel;
            if (tvm != null)
                tvm.IsEditingDescription = true;
        }

        public bool CanDeleteSelectedList { get { return SelectedList is TaskListViewModel && ((TaskListViewModel)SelectedList).AllowDeleteList; }}

        public void DeleteSelectedList()
        {
            if (!CanDeleteSelectedList) return;
            _ = DeleteListAsync((TaskListViewModel)SelectedList);
        }

        public async Task<bool> DeleteListAsync(TaskListViewModel list)
        {
            if (!list.AllowDeleteList) 
                return false;

            int idx = Lists.IndexOf(list);
            if (idx == -1) return true; // already deleted. ignore second invocation

            if (list.CompletedTasksCount + list.PendingTasksCount > 0)
                if (!await _message.ConfirmDelete("Delete List", "Are you sure you like to delete the list and all its {0} tasks?", list.CompletedTasksCount + list.PendingTasksCount))
                    return false;

            bool isSelectedList = list == SelectedList;

            // update index.
            idx = Lists.IndexOf(list);
            if (idx == -1) return true; // already deleted. ignore second invocation
            

            _storage.DeleteList(list.List);
            Lists.RemoveAt(idx);

            if (isSelectedList)
            {
                if (Lists.Count > 0 && idx > Lists.Count - 1)
                    idx = Lists.Count - 1;
                SelectedList = idx >= Lists.Count ? null : Lists[idx];
            }
            return true;
        }

        public void Reload()
        {
            Lists.CollectionChanged -= OnListCollectionChanged;
            _keepSaveOnChanges.Clear();   // remove all save requests for now.
            int selectedIdx = -1;
            
            var newLists = _storage.GetLists()
                               .OrderBy(p => p.SortPosition)
                               .ThenBy(p => p.CreatedAt)
                               .ThenBy(p => p.Description)
                               .ToList();

            var inbox = newLists.FirstOrDefault(b => string.IsNullOrEmpty(b.Description));
            if (inbox != null)
                newLists.Remove(inbox);
            else
            {
                // add an inbox.
                inbox = new TodoListWithCount();
                _storage.Save(inbox);
            }

            // create special list.
            var inboxVm = Lists.OfType<TaskListInboxViewModel>().SingleOrDefault();
            if (inboxVm != null && inboxVm.List.Id != inbox.Id)
            {
                Lists.Remove(inboxVm);
                inboxVm = null;
            }
            if (inboxVm == null)
            {
                inboxVm = new TaskListInboxViewModel(inbox, _storage, _messenger, _share);
                Lists.Insert(0, inboxVm);
            }
            
            // insert high priority view model.
            var highPriority = Lists.OfType<TaskListPriorityViewModel>().SingleOrDefault();
            if (highPriority == null)
            {
                Lists.Insert(1, new TaskListPriorityViewModel(_storage, this, inboxVm, _messenger, _share));    
            }

            foreach (var listvm in Lists.OfType<TaskListViewModel>().ToList())
            {
                // only handle plain lists.
                if(listvm.GetType() != typeof(TaskListViewModel)) continue;
                if (newLists.All(l => l.Id != listvm.List.Id))
                {
                    if (SelectedList == listvm)
                        selectedIdx = Lists.IndexOf(listvm);
                    Lists.Remove(listvm);
                }
            }

            const int numSpecialLists = 2;
            int nextIdx = numSpecialLists;

            foreach (var l in newLists)
            {
                var listvm = Lists.OfType<TaskListViewModel>().FirstOrDefault(lvm => lvm.List.Id == l.Id);

                if (listvm != null)
                {
                    int oldIdx = Lists.IndexOf(listvm);
                    if(oldIdx != nextIdx)
                        Lists.Move(oldIdx, nextIdx);

                    listvm.SetList(l);
                }
                else
                {
                    listvm = new TaskListViewModel(l, _storage, _messenger, _share);
                    Lists.Insert(nextIdx, listvm);
                }
                ++nextIdx;
            }

            foreach (var l in Lists.OfType<TaskListViewModel>())
                RegisterSaveOnChanges(l);
            Lists.CollectionChanged += OnListCollectionChanged;



            if(SelectedList == null && selectedIdx == -1 || Lists.Count == 0)
               SelectedList = Lists.FirstOrDefault();
            else if (SelectedList == null)
                SelectedList = Lists[selectedIdx >= Lists.Count ? Lists.Count - 1 : selectedIdx];
            else
                SelectedList.Refresh();

        }

        public void OnActivate()
        {
            Reload();
        }

        private void RegisterSaveOnChanges(TaskListViewModel listvm)
        {
#if !DOT42
            _keepSaveOnChanges += listvm.List.SubscribeWeak(p => p.Description, SaveList);
            _keepSaveOnChanges += listvm.List.SubscribeWeak(p => p.SortPosition, SaveList);
#else
            _keepSaveOnChanges += listvm.List.SubscribeWeak("Description", SaveList);
            _keepSaveOnChanges += listvm.List.SubscribeWeak("SortPosition", SaveList);
#endif
        }


        private void SaveList(TodoList list)
        {
            list.ModifiedAt = DateTime.UtcNow;
            _storage.Save(list);

            _messenger.Publish(new TrackableStoreModifiedMessage(this, ModificationSource.UserInterface));
        }

        private void OnListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Move ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                _storage.RunInTransaction(() =>
                {
                    var moved = e.NewItems.Cast<TaskListViewModel>().ToList();
                    var ttvmList = TodoLists;
                    var newStartingIndex = ListsIndexToTodoListsIndex(e.NewStartingIndex); 

                    new SortableElementIdCalculator<TaskListViewModel>()
                          .UpdateAfterMove(ttvmList, moved, newStartingIndex);
                });
                    
                if (SelectedList == null)
                    SelectedList = Lists[e.NewStartingIndex];
            }
        }

        public List<TaskListViewModel> TodoLists
        {
            get
            {
                return Lists.OfType<TaskListViewModel>().ToList();
            }
        }

        private int ListsIndexToTodoListsIndex(int listsIndex)
        {
            if (listsIndex > 0) return listsIndex - 1;
            return listsIndex;
        }


        //private void OnSelectedListChanged()
        //{
        //    if(SelectedList != null)
        //        SelectedListId = SelectedList.List.Id;
        //}
    }
    
    

}
