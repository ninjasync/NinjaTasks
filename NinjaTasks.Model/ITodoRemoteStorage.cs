using System;
using NinjaSync.Storage;

namespace NinjaTasks.Model
{
    [Flags]
    public enum TrackableRemoteStorageType
    {
        /// <summary>
        /// None of the oder values. In particular, the remote
        /// does not provide conflict merging capabilities 
        /// (IsMasterUptesAfterSave).
        /// </summary>
        Default = 0,
        /// <summary>
        /// if true, the remote has no first class list, but a list-name 
        /// directly attached to a task. this mainly means that the remote 
        /// can not delete lists, and that list-mapping is done by 
        /// list-description.
        /// </summary>
        HasOnlyImplicitLists = 0x02,
        ///// <summary>
        ///// TODO: maybe one could further differentiate between a
        ///// dump slave and a "smart" slave, the latter being one
        ///// that provides reliable change tracking data on indiviual
        ///// columns.
        ///// </summary>
        /// <summary>
        /// if true, sort position mapping is forced.
        /// </summary>
        RequiresSortPositionMapping = 0x08,

        ///
        RequiresIdMapping,
    }

    public interface ITodoRemoteStorageInfo : ITrackableRemoteStorageInfo
    {
        TrackableRemoteStorageType StorageType { get; }

    }


    public interface ITodoRemoteSlaveStorage : ITrackableRemoteSlaveStorage, ITodoRemoteStorageInfo
    {
    }

    public interface ITodoRemoteMasterStorage : ITrackableRemoteMasterStorage, ITodoRemoteStorageInfo
    {
    }



    public static class TrackableRemoteStorageExtensions
    {
        public static bool HasOnlyImplicitLists(this ITodoRemoteStorageInfo s)
        {
            return (s.StorageType & TrackableRemoteStorageType.HasOnlyImplicitLists) == TrackableRemoteStorageType.HasOnlyImplicitLists;
        }

        public static bool RequiresSortPositionMapping(this ITodoRemoteStorageInfo s)
        {
            return (s.StorageType & TrackableRemoteStorageType.RequiresSortPositionMapping) == TrackableRemoteStorageType.RequiresSortPositionMapping;
        }

        public static bool RequiresIdMapping(this ITodoRemoteStorageInfo s)
        {
            return (s.StorageType & TrackableRemoteStorageType.RequiresIdMapping) == TrackableRemoteStorageType.RequiresIdMapping;
        }

    }
}
