using Cirrious.MvvmCross.Plugins.Messenger;
using NinjaTasks.Core.ViewModels;

namespace NinjaTasks.Core.Messages
{
    public enum ModificationTyp
    {
        Status,
        Priority
    }

    public class TaskModifiedMessage : MvxMessage
    {
        public ModificationTyp Mod { get; private set; }
        public TodoTaskViewModel Task { get; private set; }

        public TaskModifiedMessage(object sender, ModificationTyp mod, TodoTaskViewModel task) : base(sender)
        {
            Mod = mod;
            Task = task;
        }
    }
}
