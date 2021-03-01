using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Plugin.Share;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools.Collections;
using NinjaTools.GUI.MVVM;
using PropertyChanged;

namespace NinjaTasks.Core.ViewModels
{
    public abstract class TasksViewModelBase : BaseViewModel, ITasksViewModel, IDeactivate, IActivate
    {
        protected readonly ITodoStorage Storage;
        protected readonly IMvxShareTask Share;
        protected readonly IMvxMessenger Messenger;

        public abstract string Description { get; set; }
        public abstract int SortPosition { get; set; }

        public bool ShowCompletedTasks { get; protected set; }
        public ObservableCollection<TodoTaskViewModel> Tasks { get; protected set; }
        public TodoTaskViewModel SelectedPrimaryTask { get; set; }

        [DoNotCheckEquality]
        public IList SelectedTasks { get; set; }

        public bool IsMultipleSelection { get { return SelectedTasks != null && SelectedTasks.Count > 1; } }

        public bool IsEmpty { get { return PendingTasksCount > 0 || (ShowCompletedTasks && CompletedTasksCount > 0); } }

        public virtual bool AllowDeleteList { get { return false; } }
        public virtual bool AllowRename { get { return false; } }
        public virtual bool AllowReorder { get { return false; } }
        public virtual bool AllowAddItem { get { return false; } }
        public virtual bool HasMultipleLists { get { return true; } }
        
        public abstract int PendingTasksCount { get; protected set; }
        public abstract int CompletedTasksCount { get; protected set; }

        public abstract void Refresh();

        public abstract void MoveInto(IList<TodoTaskViewModel> data, ITasksViewModel previous);

        public TasksViewModelBase(ITodoStorage storage, IMvxMessenger messenger, IMvxShareTask share)
        {
            Storage = storage;
            Messenger = messenger;
            Share = share;

            ShowCompletedTasks = true;
            AddToAutoBundling(() => ShowCompletedTasks);
            Tasks = new ObservableCollection<TodoTaskViewModel>();
        }


        /// <summary>
        /// special care is taken to provide a sensitive next-selection.
        /// </summary>
        public void DeleteSelectedTasks()
        {
            if (SelectedTasks == null) return;

            int idx = -1;
            int nextSelection = -1;
            var selection = SelectedTasks.Cast<TodoTaskViewModel>().ToList();

            foreach (TodoTaskViewModel tvm in selection) 
            {
                ++idx;

                if (tvm == SelectedPrimaryTask)
                    nextSelection = idx + 1;

                RemoveTask(tvm);
                Storage.DeleteTask(tvm.Task);

                // update idxs.
                --idx;
                if (nextSelection > 0)
                    --nextSelection;
            }

            if (nextSelection > Tasks.Count)
                --nextSelection;
            if (nextSelection >= 0 && Tasks.Count > 0)
                SelectedPrimaryTask = Tasks[nextSelection];
        }

        public void ShareSelectedTasks()
        {
            if (SelectedTasks == null) return;
            var selection = SelectedTasks.Cast<TodoTaskViewModel>()
                                         .ToList();
            var msg = string.Join("\n",
                selection.Where(s => !string.IsNullOrWhiteSpace(s.Task.Description)).Select(s => s.Task.Description));
            if(!string.IsNullOrWhiteSpace(msg))
                Share.ShareShort(msg);
        }


        public void RemoveTask(TodoTaskViewModel data)
        {
            int idx = Tasks.IndexOf(data);
            if (idx == -1) return;
            
            DetachTask(Tasks[idx]);
            Tasks.RemoveAt(idx);
        }

        protected virtual void DetachTask(TodoTaskViewModel task)
        {
            task.PropertyChanged -= OnTaskPropertyChanged;
        }

        protected void DetachAllTasks()
        {
            if(Tasks != null)
                foreach (var vm in Tasks)
                    DetachTask(vm);
        }

        protected void AttachTask(TodoTaskViewModel newTask)
        {
            newTask.PropertyChanged += OnTaskPropertyChanged;
        }

        protected virtual void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "IsPriority")
                OnTaskPriorityChanged((TodoTaskViewModel)sender);
            if (e.PropertyName == "IsCompleted")
                OnTaskCompletedChanged((TodoTaskViewModel)sender);
          
        }

        protected virtual void OnTaskCompletedChanged(TodoTaskViewModel vm)
        {
        }

        protected virtual void OnTaskPriorityChanged(TodoTaskViewModel vm)
        {
        }

        public virtual void OnDeactivate()
        {
        }

        public virtual void OnActivate()
        {
            // start fresh, to fix wpf not updating sort order
            Tasks = new ObservableCollection<TodoTaskViewModel>();
        }
        public virtual void OnDeactivated(bool destroying)
        {
            DetachAllTasks();
            Tasks.Clear();
        }

        protected void ReplaceTasks(List<TodoTask> newTasks, Func<TodoTask, TaskListViewModel> getTaskList)
        {
            int selectedIdx = -1;

            var targetList = Tasks;

            foreach (var taskvm in targetList.ToList())
            {
                if (newTasks.All(t => t.Id != taskvm.Task.Id))
                {
                    if (SelectedPrimaryTask == taskvm)
                        selectedIdx = targetList.IndexOf(taskvm);
                    targetList.Remove(taskvm);
                    DetachTask(taskvm);
                }
            }

            int nextIdx = 0;
            foreach (var todoTask in newTasks)
            {
                int oldIdx = targetList.FindIndex(tvm => tvm.Task.Id == todoTask.Id);
                
                if (oldIdx != -1)
                {
                    var taskvm = targetList[oldIdx];

                    if (oldIdx != nextIdx)
                        targetList.Move(oldIdx, nextIdx);

                    // don't listen to any changes now.
                    DetachTask(taskvm);
                    taskvm.SetTask(todoTask);
                    AttachTask(taskvm);
                }
                else
                {
                    var taskvm = new TodoTaskViewModel(todoTask, getTaskList(todoTask), 
                                                       Storage, Messenger, Share);
                    targetList.Insert(nextIdx, taskvm);
                    AttachTask(taskvm);
                }

                ++nextIdx;
            }

            if (selectedIdx != -1 && Tasks.Count > 0)
            {
                // don't set a new selection.
                //SelectedPrimaryTask = null;
            }

        }

        /// <summary>
        /// Get text that can be copyied to a clipboard.
        /// </summary>
        public string GetCopyText()
        {
            StringBuilder bld = new StringBuilder();
            foreach (TodoTaskViewModel sel in SelectedTasks)
            {
                if (sel.IsCompleted) bld.Append("X");
                if (sel.IsPriority) bld.Append("!");
                bld.Append("\t");
                bld.AppendLine(sel.Task.Description);
            }
            return bld.ToString();
        }
    }
}