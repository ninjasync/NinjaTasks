using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Plugin.Share;
using NinjaSync.Model.Journal;
using NinjaTasks.Core.Messages;
using NinjaTasks.Core.Reusable;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools;
using NinjaTools.GUI.MVVM;
using NinjaTools.Npc;
using NinjaTools.WeakEvents;
using NinjaTools.Threading;

namespace NinjaTasks.Core.ViewModels
{
    [DebuggerDisplay("Id={Task.Id} IsCompleted={IsCompleted} SortPosition={SortPosition} Description={Task.Description} ")]
    public class TodoTaskViewModel : BaseViewModel, ISortableElement
    {
        private readonly ITodoStorage _storage;
        private readonly IMvxMessenger _messenger;
        private readonly IMvxShareTask _share;
        private readonly Guard _savingGuard = new Guard();
        private WeakEventHandler _weakHandler;
        private TokenBag _listBinding = new TokenBag();

        private List<string> _modifiedProperties = new List<string>();

        private DelayedCommand _delayedSave;

        public TodoTask Task { get; private set; }
        public bool IsNewTask { get; set; }

        public bool IsCompleted { get { return Task.Status == Status.Completed; } set { Task.Status = value ? Status.Completed : Status.Pending; }}
        public bool IsPriority { get { return Task.Priority == Priority.High; } set { Task.Priority = value ? Priority.High : Priority.Normal;}}

        public int SortPosition { get { return Task.SortPosition; } set { Task.SortPosition = value; } }
        public bool IsEditingDescription { get; set; }

        public bool HasAttachments { get => Task.AdditionalProperties.Contains("Attachment"); }
        public string AttachmentName { get => Task.GetProperty("AttachmentFileName")?.ToString(); }


        public TaskListViewModel List { get; private set; }
        public string ListDescription { get; private set; }
        
        public TodoTaskViewModel(TodoTask task, TaskListViewModel list, ITodoStorage storage,
                                 IMvxMessenger messenger, IMvxShareTask share)
        {
            _storage = storage;
            _messenger = messenger;
            _share = share;

            SetList(list);
            SetTask(task);

            _delayedSave = new DelayedCommand();
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
                    return "Completed " + Task.CompletedAt.Value.ToString("yyyy-MM-dd HH:mm");
                return "Created " + Task.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            }
        }

        public string ModifiedText
        {
            get
            {
                return "Modified " + Task.ModifiedAt.ToString("yyyy-MM-dd HH:mm");
            }
        }

        public void SetAttachment(string fileName, byte[] data)
        {
            Task.SetProperty("AttachmentFileName", data == null ? null : fileName);
            Task.SetProperty("Attachment", data == null ? null : Convert.ToBase64String(data));
        }

        public bool CanDeleteAttachment => HasAttachments;
        public void DeleteAttachment()
        {
            Task.SetProperty("AttachmentFileName", null);
            Task.SetProperty("Attachment", null);
        }

        public (string, byte[]) GetAttachment()
        {
            string data = Task.GetProperty("Attachment")?.ToString();
            if (data == null) return (null, null);
            return (Task.GetProperty("AttachmentFileName")?.ToString(), Convert.FromBase64String(data));
        }

        private void OnTaskChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsNewTask || _savingGuard.InUse) return;

            if (e.PropertyName == null)
                _modifiedProperties = null;
            if (_modifiedProperties != null && e.PropertyName != null)
                _modifiedProperties.Add(e.PropertyName);

            if (e.PropertyName == TodoTask.ColStatus)
                _messenger.Publish(new TaskModifiedMessage(this, ModificationTyp.Status, this));
            else if (e.PropertyName == TodoTask.ColPriority)
                _messenger.Publish(new TaskModifiedMessage(this, ModificationTyp.Priority, this));
            else if (e.PropertyName == "Attachment" || e.PropertyName == "AttachmentFileName")
            {
                RaisePropertyChanged(nameof(HasAttachments));
                RaisePropertyChanged(nameof(AttachmentName));
                RaisePropertyChanged(nameof(CanDeleteAttachment));
            }
            else if (e.PropertyName.IsNullOrEmpty())
            {
                RaisePropertyChanged();
            }

            _delayedSave.Schedule(SaveImpl);
        }

        private void SaveImpl()
        {
            if (_modifiedProperties != null && _modifiedProperties.Count == 0)
                return; // should not happend

            using (_savingGuard.Use())
            {
                _storage.RunInTransaction(() =>
                {
                    List<string> props = _modifiedProperties;
                    _modifiedProperties = new List<string>();

                    if (props != null)
                        props.Add(TrackableProperties.ColModifiedAt);

                    // set completed time if status changed to completed.
                    if (props?.Contains(TodoTask.ColStatus) == true && Task.Status == Status.Completed)
                    {
                        Task.CompletedAt = DateTime.UtcNow;
                        props.Add(TodoTask.ColCompletedAt);
                    }

                    // Save Task.
                    Task.ModifiedAt = DateTime.UtcNow;
                    _storage.Save(Task, props);

                    // send notification message
                    _messenger.Publish(new TrackableStoreModifiedMessage(this, ModificationSource.UserInterface));
                });
            }

            RaisePropertyChanged(nameof(CreatedOrCompletedText));
            RaisePropertyChanged(nameof(ModifiedText));
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

        public void Delete()
        {
            if (List?.SelectedTasks != null)
            {
                if (List?.SelectedTasks?.Cast<TodoTaskViewModel>().SingleOrDefault() != this)
                    return;
            }
            else if (List?.SelectedPrimaryTask != this)
            { 
                return;
            }

            List.DeleteSelectedTasks();
        }

        public void Share()
        {
            _share.ShareShort(Task.Description);
        }
    }
}
