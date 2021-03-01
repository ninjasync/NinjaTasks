using System;
using NinjaSync.Storage;

namespace NinjaTasks.Model.Storage
{
    public static class TrackableStorageExtensions2
    {
        public static ITodoStorage AsTodoStorage(this ITrackableStorage storage)
        {
            if (storage is ITodoStorage)
                return (ITodoStorage)storage;
            if (storage is TrackableJournalStorageAdapter)
                return ((TrackableJournalStorageAdapter)storage).Storage as ITodoStorage;
            throw new Exception("dont now how to handle " + storage.GetType());
        }
    }
}