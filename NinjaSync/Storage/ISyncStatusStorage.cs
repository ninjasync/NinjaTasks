using System.Threading;
using NinjaSync.Model;

namespace NinjaSync.Storage
{
    public interface ISyncStatusStorage
    {
        /// <summary>
        /// this will create a new status, if not already in database
        /// </summary>
        SyncStatus GetStatus(string accountId);

        /// <summary>
        /// will return null if not in database
        /// </summary>
        SyncStatus GetStatusByRemoteStorageId(string remoteStorageId);

        void SaveStatus(SyncStatus status);

        // this can be used to save the external represantation of an object.
        SyncRemoteRepresentation GetRepresentation(string accountId, string uuid);
        void SaveRepresentation(SyncRemoteRepresentation mapping);

        SemaphoreSlim GetAccountSemaphore(string accountId);
    }
}