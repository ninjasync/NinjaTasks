using System;
using NinjaTools.Sqlite;

namespace NinjaSync.Model
{
    /// <summary>
    /// all synchronization accounts save information about synchronization
    /// status here.
    /// </summary>
    public class SyncStatus 
    {
        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string AccountId { get; set; }

        /// <summary>
        /// P2P sync:
        /// Idenfifies the remote, so that incoming
        /// and outbound syncs can be properly matched.
        /// </summary>
        [Indexed]
        public string RemoteStorageId { get; set; }

        /// <summary>
        /// Date/Time of last successfull synchronization
        /// </summary>
        public DateTime LastSync { get; set; }
        
        /// <summary>
        /// last successfully sychronized local commit.
        /// </summary>
        public string LocalCommitId { get; set; }

        /// <summary>
        /// Master/Slave sync:
        /// The last successfully synchronized remote 
        /// commit id, if applicable.
        /// </summary>
        public string RemoteCommitId { get; set; }

        /// <summary>
        /// Master/Slave sync: 
        /// This Local CommitId sits on top of LocalCommitId,
        /// but should not be send to the server (because
        /// it came from him in the first place)
        /// </summary>
        public string RemoteExcludeLocalCommitId { get; set; }

        public override string ToString()
        {
            return string.Format("Account: {0}, LastSync: {1}, LocalCommitId: {2}, RemoteCommitId: {3}", Id, LastSync, LocalCommitId, RemoteCommitId);
        }
    }
}
