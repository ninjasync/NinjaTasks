using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Plugin.Share;
using NinjaTasks.Core.Messages;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools;
using NinjaTools.GUI.MVVM;
using NinjaTools.Npc;

namespace NinjaTasks.Core.ViewModels
{
    public class TaskListPriorityViewModel : TasksViewModelBase, IActivate
    {
        private readonly TodoListsViewModel _taskLists;
        private readonly TaskListViewModel _inbox;
        public override string Description { get { return "Important"; } set { } }
        
        public override bool AllowAddItem { get { return _inbox != null; } }
        public TodoTaskViewModel NewTask { get; private set; }

        private readonly TokenBag _bag = new TokenBag();

        public TaskListPriorityViewModel(ITodoStorage storage, TodoListsViewModel taskLists, 
                                        TaskListViewModel inbox, IMvxMessenger messenger, IMvxShareTask share)
                                        : base(storage, messenger, share)
        {
            _taskLists = taskLists;

            _inbox = inbox;
            if (_inbox != null)
            {
#if !DOT42
                _inbox.BindTo(i => i.NewTask, this, t => t.NewTask);
#else
                _inbox.BindTo("NewTask", this, "NewTask");
#endif
            }

            _bag += messenger.SubscribeOnMainThread<TrackableStoreModifiedMessage>((s) => RefreshCount());

            RefreshCount();
        }

        //public override bool AllowReorder { get { return false; } }
        
        public override int PendingTasksCount { get; protected set; }
        public override int CompletedTasksCount { get; protected set; }

        public override void MoveInto(IList<TodoTaskViewModel> data, ITasksViewModel previous)
        {
            foreach (var task in data)
                task.IsPriority = true;
        }

        public override void Refresh()
        {
            List<TodoTask> newTasks = Storage.GetTasks(onlyHighPriority: true, includeComplete: false)
                                              .ToList();
            var lists = _taskLists.TodoLists;
            ReplaceTasks(newTasks, t => lists.FirstOrDefault(l => l.List.Id == t.ListFk));
        }

        private void RefreshCount()
        {
            PendingTasksCount = Storage.CountTasks(includeComplete: false, onlyHighPriority: true);
            int totalHigh = Storage.CountTasks(includeComplete: true, onlyHighPriority: true);
            CompletedTasksCount = PendingTasksCount - totalHigh;
        }

        public override int SortPosition { get; set; }

        public override void OnActivate()
        {
            base.OnActivate();
            Refresh();
        }

        public void AddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTask.Task.Description)) 
                return;

            NewTask.IsPriority = true;
            var task = NewTask;

            _inbox.AddTask();

            Tasks.Add(task);
            AttachTask(task);
        }

        protected override void OnTaskPriorityChanged(TodoTaskViewModel obj)
        {
            base.OnTaskPriorityChanged(obj);

            if (!obj.IsPriority)
                RemoveTask(obj);
        }
    }
}
