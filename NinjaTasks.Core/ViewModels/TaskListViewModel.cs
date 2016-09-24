using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cirrious.MvvmCross.Plugins.Messenger;
using NinjaSync.Storage;
using NinjaTasks.Core.Reusable;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools;
using NinjaTools.MVVM;

namespace NinjaTasks.Core.ViewModels
{
    public class TaskListViewModel : TasksViewModelBase, IActivate
    {
        //private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ITodoStorage _storage;
        private readonly IMvxMessenger _messenger;
        public TodoListWithCount List { get; private set; }

        public override int SortPosition { get { return List.SortPosition; } set { List.SortPosition = value; } }
        public override string Description { get { return List.Description.IsNullOrEmpty()?"(Inbox)" : List.Description; } set { List.Description = value; }}

        public bool IsEditingDescription { get; set; }

        public TodoTaskViewModel NewTask { get; private set; }

        private readonly Guard _guardCompleted = new Guard();
        private readonly SortableElementIdCalculator<TodoTaskViewModel> _sortUpdater = new SortableElementIdCalculator<TodoTaskViewModel>();

        public override bool AllowRename { get { return true; } }
        public override bool AllowReorder { get { return true; } }
        public override bool AllowDeleteList { get { return true; } }
        public override bool AllowAddItem { get { return true; } }
        public override bool HasMultipleLists { get { return false; } }

        public override int PendingTasksCount { get; protected set; }
        public override int CompletedTasksCount { get; protected set; }

        public TaskListViewModel(TodoListWithCount list, ITodoStorage storage, IMvxMessenger messenger)
            :base(storage, messenger)
        {
            _storage = storage;
            _messenger = messenger;
            SetList(list);
        }

        public void SetList(TodoListWithCount list)
        {
            List = list;
            CompletedTasksCount = list.CompletedTasksCount;
            PendingTasksCount = list.PendingTasksCount;
        }

        public void AddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTask.Task.Description)) return;

            NewTask.Task.Description = NewTask.Task.Description.Trim();
            
            NewTask.Task.ListFk = List.Id;
            NewTask.Task.CreatedAt = DateTime.Now;

            int newIndex = NewTask.IsPriority ? 0 : PendingTasksCount;

            if (newIndex < 0 || newIndex > Tasks.Count)
            {
                // TODO: find proper SortPosition by going to the database.
                //       this happens when we are unloaded, i.e. when ann add 
                //       happens in TaskListPriorityVM
                NewTask.SortPosition = Tasks.Select(p => p.SortPosition).DefaultIfEmpty().Max();
                Tasks.Add(NewTask);
            }
            else
            {
                Tasks.Insert(newIndex, NewTask);
                UpdateSortPositionAfterMove(new[] {NewTask}, newIndex);
            }

            Storage.Save(NewTask.Task);
            NewTask.IsNewTask = false;
            AttachTask(NewTask);

            SelectedPrimaryTask = NewTask;
            PendingTasksCount += 1;

            //SelectedTasks = new[] {NewTask};
            NewTask = new TodoTaskViewModel(new TodoTask(), this, Storage, _messenger) { IsNewTask = true };
        }


        public override void OnActivate()
        {
            base.OnActivate();
            NewTask = new TodoTaskViewModel(new TodoTask(), this, Storage, _messenger) { IsNewTask = true };
            Refresh();
        }

        public void ToggleEditDescription()
        {
            if (SelectedPrimaryTask != null)
                SelectedPrimaryTask.IsEditingDescription = !SelectedPrimaryTask.IsEditingDescription;
        }

        public void ToogleShowCompletedTasks()
        {
            ShowCompletedTasks = !ShowCompletedTasks;
            Refresh();
        }

        public override void Refresh()
        {
            List<TodoTask> newTasks = Storage.GetTasks(List, includeComplete: true)
                                             .OrderBy(t => t.Status)
                                             .ThenBy(t => t.SortPosition)
                                             .ToList();
            ReplaceTasks(newTasks, t => this);
        }

        protected override void OnTaskPriorityChanged(TodoTaskViewModel vm)
        {
            base.OnTaskPriorityChanged(vm);

            if (vm.IsPriority)
            {
                Tasks.Move(Tasks.IndexOf(vm), 0);
                UpdateSortPositionAfterMove(new []{vm}, 0);
            }
        }

        protected override void OnTaskCompletedChanged(TodoTaskViewModel vm)
        {
            base.OnTaskCompletedChanged(vm);

            if (_guardCompleted.InUse) return;

            SelectedPrimaryTask = null;

            // move to top.
            if (!vm.IsCompleted)
            {
                Tasks.Move(Tasks.IndexOf(vm), 0);
                UpdateSortPositionAfterMove(new[] { vm }, 0);
            }
            else
            {
                // move to top of lower list.
                var newIndex = Tasks.Count(p=>!p.IsCompleted);
                var oldIndex = Tasks.IndexOf(vm);
                Tasks.Move(oldIndex, newIndex);
                UpdateSortPositionAfterMove(new []{vm}, newIndex);
            }

            UpdateTasksCount();
        }

        /// <summary>
        /// change the order of items
        /// </summary>
        public void MoveToPosition(IList<TodoTaskViewModel> tasks, int newIndex, bool? setCompletedState)
        {

            if (setCompletedState == true)
                newIndex = Tasks.Count(p => !p.IsCompleted) - tasks.Count(p=>!p.IsCompleted);
            else
            {
                bool isBeforeNewIndex = tasks.Any(t => Tasks.IndexOf(t) < newIndex);
                newIndex = newIndex - (isBeforeNewIndex ? 1 : 0);
            }

            using (_guardCompleted.Use())
            {
                foreach (var task in tasks.Reverse())
                {
                    if (setCompletedState == false)
                        task.IsCompleted = false;
                    if (setCompletedState == true)
                        task.IsCompleted = true;

                    int oldIndex = Tasks.IndexOf(task);
                    
                    Tasks.Move(oldIndex, newIndex);
                }
            }

            UpdateSortPositionAfterMove(tasks, newIndex);
        }

        /// <summary>
        /// move into this list from another list.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="previous"></param>
        public override void MoveInto(IList<TodoTaskViewModel> data, ITasksViewModel previous)
        {
            // make sure tasks are loaded.
            Refresh();

            // do nothing...
            if (data.All(p => p.Task.ListFk == List.Id)) return;

            // detach from previous
            foreach (var e in data)
            {
                e.List.RemoveTask(e);
                e.List.LoadTasksCount();

                e.SetList(this);
                e.Task.ListFk = List.Id;
            }


            var completed = data.Where(d => d.IsCompleted).ToList();
            var pending = data.Where(d => !d.IsCompleted).ToList();

            for (int i = 0; i < pending.Count; i++)
                // insert at beginning.
                Tasks.Insert(i, pending[i]);

            UpdateSortPositionAfterMove(pending, 0);

            // completed
            var completedIdx = Tasks.Count(p => !p.IsCompleted);
            for (int i = 0; i < completed.Count; i++)
            {
                // insert at beginning.
                Tasks.Insert(i+completedIdx, completed[i]);
            }
            UpdateSortPositionAfterMove(completed, completedIdx);

            foreach (var task in data)
            {
                task.Task.ModifiedAt = DateTime.UtcNow;

                _storage.Save(task.Task, new[]{
                    TodoTask.ColListFk, 
                    TodoTask.ColSortPosition,
                    TodoTask.ColModifiedAt});

                AttachTask(task);
            }

            UpdateTasksCount();
        }

        private void UpdateSortPositionAfterMove(IList<TodoTaskViewModel> moved, int newIndex)
        {
            if (moved.Count == 0) return;

            // order into one of two lists, depending on the isCompleted status
            
            bool isCompleted = moved.First().IsCompleted;
            int completedPos = Tasks.Count(p => !p.IsCompleted);

            //Log.Info("UpdateSortPositionAfterMove: isCompleted={0} completedPos={1} newIndex={2}", isCompleted, completedPos,newIndex);

            Debug.Assert((isCompleted && newIndex >= completedPos)
                      || (!isCompleted && newIndex <=completedPos));

            IList<TodoTaskViewModel> subview = Tasks.Where(p=>p.IsCompleted == isCompleted).ToList();
            var newStartIndex = isCompleted ? (newIndex - completedPos) : newIndex;
            Storage.RunInTransaction(() => _sortUpdater.UpdateAfterMove(subview, moved, newStartIndex));

        }

        private void LoadTasksCount()
        {
            var list = _storage.GetLists(List.Id).FirstOrDefault();
            if(list != null)
                SetList(list);
        }

        private void UpdateTasksCount()
        {
            CompletedTasksCount = Tasks.Count(p => p.IsCompleted);
            PendingTasksCount = Tasks.Count(p => !p.IsCompleted);
        }

    }
}
