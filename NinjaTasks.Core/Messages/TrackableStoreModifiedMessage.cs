using MvvmCross.Plugin.Messenger;
using NinjaTasks.Model.Sync;

namespace NinjaTasks.Core.Messages
{
    public enum ModificationSource
    {
        UserInterface,
        ImportExport,
        Sync,
        P2PExchange,
    }

    public class TrackableStoreModifiedMessage : MvxMessage
    {
        public ModificationSource Source { get; set; }
        public SyncAccount Account { get; set; }

        public TrackableStoreModifiedMessage(object sender, ModificationSource source)
            : base(sender)
        {
            Source = source;
        }
    }
}
