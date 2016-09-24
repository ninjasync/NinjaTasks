using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;

namespace NinjaTasks.Model.Sync
{
    public enum SyncAccountType
    {
        Unknown,
        TaskWarrior,
        BluetoothP2P,
        TcpIpP2P,
        WifiDirectP2P,
        NonsenseAppsNotePad,
        OrgTasks,
    }

    public class SyncAccount
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public SyncAccountType Type { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }

        public bool IsManualSyncOnly { get; set; }
        public bool IsSyncOnDataChanged { get; set; }

        public TimeSpan SyncInterval { get; set; }
        
        public int SyncFailureCount { get; set; }
        
        public DateTime LastSyncAttempt { get; set; }
        public string LastSyncError { get; set; }
        public DateTime LastSuccessfulSync { get; set; }

        public string AccountId { get { return Type + ":" + Id; } }
    }
}