using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MvvmCross.ViewModels;
using NinjaTasks.Core.Reusable;

namespace NinjaTasks.Core.ViewModels
{
    public interface ITasksViewModel : ISortableElement, IMvxViewModel
    {
        string Description { get; set; }
        bool ShowCompletedTasks { get; }

        ObservableCollection<TodoTaskViewModel> Tasks { get; }
        TodoTaskViewModel SelectedPrimaryTask { get; set; }
        IList SelectedTasks { get; set; }

        bool IsMultipleSelection { get; }

        bool IsEmpty { get; }

        bool AllowDeleteList { get; }
        bool AllowReorder { get; }
        bool AllowRename { get; }
        int PendingTasksCount { get; }
        int CompletedTasksCount { get; }
        bool AllowAddItem { get; }
        bool HasMultipleLists { get; }

        void MoveInto(IList<TodoTaskViewModel> data, ITasksViewModel previous);
        void RemoveTask(TodoTaskViewModel data);

        void Refresh();

        /// <summary>
        /// special care is taken to provide a sensitive next-selection.
        /// </summary>
        void DeleteSelectedTasks();
    }
}