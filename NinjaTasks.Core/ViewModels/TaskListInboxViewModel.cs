
using System.Diagnostics;
using Cirrious.MvvmCross.Plugins.Messenger;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;

namespace NinjaTasks.Core.ViewModels
{
    public class TaskListInboxViewModel : TaskListViewModel
    {
        public override string Description { get { return "Inbox"; } set { } }
        
        public override bool AllowReorder { get { return false; } }
        public override bool AllowRename { get { return false; } }
        public override bool AllowDeleteList { get { return false; } }

        public TaskListInboxViewModel(TodoListWithCount list, ITodoStorage storage, IMvxMessenger messenger) : base(list, storage, messenger)
        {
            Debug.Assert(string.IsNullOrEmpty(list.Description));
        }
    }
}
