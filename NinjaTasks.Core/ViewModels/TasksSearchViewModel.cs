using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Plugin.Share;
using NinjaTasks.Model.Storage;

namespace NinjaTasks.Core.ViewModels
{
    public class TasksSearchViewModel : TasksViewModelBase
    {
        private readonly TodoListsViewModel _taskLists;
        public string SearchText { get; set; }
        
        public TasksSearchViewModel(ITodoStorage storage, TodoListsViewModel taskLists,
                                    IMvxMessenger messenger, IMvxShareTask share) 
                            : base(storage, messenger, share)
        {
            _taskLists = taskLists;
        }

        public void Search()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                Tasks = new ObservableCollection<TodoTaskViewModel>();
                return;
            }

            var tasks = Storage.FindTasks(SearchText).ToList();
            var lists = _taskLists.TodoLists;
            
            ReplaceTasks(tasks, t => lists.FirstOrDefault(l => l.List.Id == t.ListFk));

            PendingTasksCount = Tasks.Count(p => !p.IsCompleted);
            CompletedTasksCount = Tasks.Count(p => p.IsCompleted);
        }
        public override void Refresh()
        {
            Search();
        }
    
        public override string Description { get { return "Search Results"; } set { } }

        public override int SortPosition { get { return -1000000; } set {}}

        public override int PendingTasksCount { get; protected set; }
        public override int CompletedTasksCount { get; protected set; }

        public override void MoveInto(IList<TodoTaskViewModel> data, ITasksViewModel previous)
        {
            Debug.Assert(false);
        }

    }
}
