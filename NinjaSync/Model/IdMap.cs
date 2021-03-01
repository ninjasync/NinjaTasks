using NinjaTools.Sqlite;
using NinjaSync.Model.Journal;

namespace NinjaSync.Model
{
    public class IntIdMap : IdMap<int>
    {
    }

    public class StringIdMap : IdMap<string>
    {
    }

    /// <summary>
    /// this is used to indicate that actually no id mapping is required.
    /// </summary>
    public class NullIdMap : StringIdMap
    {
    }


    public abstract class IdMap<T>
    {
        [PrimaryKey]
        public string LocalId { get; set; }

        [PrimaryKey, Unique(Name = "RemoteCommitId")]
        public TrackableType ObjectType { get; set; }

        [Unique(Name = "RemoteCommitId"), NotNull]
        public T RemoteId { get; set; }
    }

}
