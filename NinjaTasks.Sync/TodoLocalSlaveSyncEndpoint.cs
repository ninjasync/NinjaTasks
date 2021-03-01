using NinjaSync.MasterSlave;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTasks.Model;
using NinjaTools.Logging;

namespace NinjaTasks.Sync
{
    public class TodoLocalSlaveSyncEndpoint : LocalSlaveSyncEndpoint
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public TodoLocalSlaveSyncEndpoint(ITrackableRemoteStorageInfo remote, ITrackableJournalStorage storage) : base(remote, storage)
        {
        }

        protected override bool CanSaveObject(ITrackable local)
        {
            if (local.TrackableType == TrackableType.Task)
            {
                // every task got to have a valid list!
                var localListKey = new TrackableId(TrackableType.List, ((TodoTask)local).ListFk);
                if (!Storage.Exists(localListKey))
                {
                    // this could happen, if the remote can not atomically retrieve lists and tasks.
                    // then he could send tasks with no valid lists. it would be better handled
                    // at the remote end, but handle graceful for now.
                    Log.Error("task {0} has no associated list. ignoring.", local.Id);
                    return false;
                }
            }
            return true;
        }
    }
}