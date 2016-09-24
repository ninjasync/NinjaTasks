using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Cirrious.MvvmCross.Plugins.Messenger;
using NinjaSync.Model.Journal;
using NinjaTasks.Core.Messages;
using NinjaTasks.Core.Reusable;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools;
using NinjaTools.MVVM;
using NinjaTools.Npc;
using NinjaTools.WeakEvents;

namespace NinjaTasks.Core.ViewModels
{
    [DebuggerDisplay("Id={Task.Id} IsCompleted={IsCompleted} SortPosition={SortPosition} Description={Task.Description} ")]
    public class TodoTaskViewModel : BaseViewModel, ISortableElement
    {
        private readonly ITodoStorage _storage;
        private readonly IMvxMessenger _messenger;
        private readonly Guard _savingGuard = new Guard();
        private WeakEventHandler _weakHandler;
        private TokenBag _listBinding = new TokenBag();

        public TodoTask Task { get; private set; }
        public bool IsNewTask { get; set; }

        public bool IsCompleted { get { return Task.Status == Status.Completed; } set { Task.Status = value ? Status.Completed : Status.Pending; }}
        public bool IsPriority { get { return Task.Priority == Priority.High; } set { Task.Priority = value ? Priority.High : Priority.Normal;}}

        public int SortPosition { get { return Task.SortPosition; } set { Task.SortPosition = value; } }
        public bool IsEditingDescription { get; set; }

        public TaskListViewModel List { get; private set; }
        public string ListDescription { get; private set; }
        
        public TodoTaskViewModel(TodoTask task, TaskListViewModel list, ITodoStorage storage, IMvxMessenger messenger)
        {
            _storage = storage;
            _messenger = messenger;

            SetList(list);
            SetTask(task);
        }

        public void SetTask(TodoTask todoTask)
        {
            if(_weakHandler != null)
                _weakHandler.Dispose();

            Task = todoTask;

            _weakHandler = PropertyChangedWeakEventHandler.Register(Task, this,
                (me, sender, args) => me.OnTaskChanged(sender, args));
            
            RaiseAllPropertiesChanged();
        }

        public string CreatedOrCompletedText
        {
            get
            {
                if (IsCompleted && Task.CompletedAt != null)
                    return "Completed " + Task.CompletedAt.Value.ToString("d");
                return "Created " + Task.CreatedAt.ToString("d");
            }
        }


        private void OnTaskChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsNewTask || _savingGuard.InUse) return;

            using (_savingGuard.Use())
            {
                _storage.RunInTransaction(() =>
                {
                    List<string> props = null;

                    if (!e.PropertyName.IsNullOrEmpty())
                        props = new List<string> { e.PropertyName, TrackableProperties.ColModifiedAt };

                    // set completed time if status changed to completed.
                    if (e.PropertyName == TodoTask.ColStatus && Task.Status == Status.Completed)
                    {
                        Task.CompletedAt = DateTime.UtcNow;

                        if(props != null) props.Add(TodoTask.ColCompletedAt);
                    }
                        
                    // Save Task.
                    Task.ModifiedAt = DateTime.UtcNow;
                    _storage.Save(Task,props);

                    // send notification message
                    _messenger.Publish(new TrackableStoreModifiedMessage(this, ModificationSource.UserInterface));
                });
                
            }

            if (e.PropertyName == TodoTask.ColStatus)
                _messenger.Publish(new TaskModifiedMessage(this, ModificationTyp.Status, this));
            else if (e.PropertyName == TodoTask.ColPriority)
                _messenger.Publish(new TaskModifiedMessage(this, ModificationTyp.Priority, this));
            else if (e.PropertyName.IsNullOrEmpty())
                RaisePropertyChanged();
        }

        public void SetList(TaskListViewModel listVm)
        {
            _listBinding.Clear();
            List = listVm;
#if !DOT42
            _listBinding += List.BindToWeak(p => p.Description, this, p=>p.ListDescription);
#else
            _listBinding += List.BindToWeak("Description", this, "ListDescription");
#endif
        }
    }
}
