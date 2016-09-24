using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using NinjaSync.Model.Journal;

namespace NinjaSync.Model
{
    public enum ChangeType
    {
        //[Obsolete]
        Created,
        Modified,
        Deleted,
    }

    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        //public int TrackingId { get; set; }
        public DateTime Timestamp { get; set; }

        [Indexed("Modification", 0, Unique = true), NotNull]
        public ChangeType Change { get; set; }

        [Indexed("Modification", 1, Unique = true), NotNull]
        public TrackableType Type { get; set; }

        [Indexed("Modification", 2, Unique = true), NotNull]
        public string ObjectId { get; set; }

        [Indexed("Modification", 3, Unique = true), NotNull]
        public string Member { get; set; }
    }
}
