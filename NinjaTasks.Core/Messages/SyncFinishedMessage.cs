using MvvmCross.Plugin.Messenger;
using NinjaTasks.Model.Sync;
using NinjaTools;

namespace NinjaTasks.Core.Messages
{
    public class SyncFinishedMessage :  MvxMessage
    {
        public SyncAccount Account { get; set; }
        public bool IsSyncActive { get; set; }

        public bool IsManualSync { get; set; }
        public string SyncError { get; set; }

        public int TotalSyncActive { get; set; }

        public bool WasSuccesfullSync { get { return !IsSyncActive && SyncError.IsNullOrEmpty(); }}
        public int UploadedChanges { get; set; }
        public int DownloadedChanges { get; set; }

        public SyncFinishedMessage(object sender, SyncAccount account) : base(sender)
        {
            Account = account;
        }
    }
}
