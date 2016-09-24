using System.Collections.Generic;
using NinjaSync.Model.Journal;

namespace NinjaSync.MasterSlave
{
    public interface IUpdateAlgorithm
    {
        /// <summary>
        /// InitializeUpdate will be called before any journal relevant updates to the
        /// local data storage are made.
        /// </summary>
        void InitializeUpdate();
        /// <summary>
        /// 
        /// </summary>
        ICollection<string> GetUpdatableColumns(TrackableId localTarget, ITrackable remoteSource, ICollection<string> defaultUpdatedColumns);
        IEnumerable<TrackableId> GetDeletable(IEnumerable<TrackableId> localScheduledForDeletion);
    }

    public class DefaultUpdateAlgorithm : IUpdateAlgorithm
    {
        public void InitializeUpdate()
        {
        }

        public ICollection<string> GetUpdatableColumns(TrackableId localTarget, ITrackable remoteSource, ICollection<string> defaultUpdatedColumns)
        {
            return defaultUpdatedColumns;
        }

        public IEnumerable<TrackableId> GetDeletable(IEnumerable<TrackableId> localScheduledForDeletion)
        {
            return localScheduledForDeletion;
        }
    }
}
