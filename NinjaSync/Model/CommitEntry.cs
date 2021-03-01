using System;
using System.Collections.Generic;
using NinjaTools.Sqlite;

namespace NinjaSync.Model
{
    public class CommitEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public string CommitId { get; set; }

        [Indexed]
        public int JournalLargerThanId { get; set; }
        [Indexed]
        public int JournalSmallerAndEqualId { get; set; }

        public DateTime Timestamp { get; set; }

        public string BasedOnCommitId { get; set; }
        public string MergeSecondCommitId { get; set; }

        [Ignore]
        public IList<JournalEntry> JournalEntries { get; set; }
    }
}
