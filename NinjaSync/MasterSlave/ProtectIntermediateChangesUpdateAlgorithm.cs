using System;
using System.Collections.Generic;
using System.Linq;
using NinjaSync.Journaling;
using NinjaSync.Model.Journal;
using NinjaTools.Logging;

namespace NinjaSync.MasterSlave
{

    /// <summary>
    /// This algorithm is used when synchronizing with an upstream server that does the real 
    /// merging. Here we make sure that changes to the local repository between the point 
    /// we uploaded the last changes and the point of where we get results back from the 
    /// server are not overwritten but preserved. These changes can than later be uploaded
    /// to the server, who can do the proper merging.
    /// <para/>
    /// On first access, this class will collect all changes since the specified CommitId. 
    /// Then it will suppress all changes to conflicting modified columns.
    /// <para/>
    /// remote modifications to locally deleted objects are "dropped".
    /// </summary>
    internal class ProtectIntermediateChangesUpdateAlgorithm : IUpdateAlgorithm
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly IModificationAssembler _tracker;
        private readonly string _protectChangesSinceButExcludingCommitId;

        private Commit _intermediateChanged;

        private HashSet<TrackableId> _intermediateDeleted;
        private HashSet<ModificationKey> _intermediateModified;

        private readonly HashSet<TrackableId> _protectFromDeletion=new HashSet<TrackableId>();

        public ProtectIntermediateChangesUpdateAlgorithm(IModificationAssembler tracker,
                                                         string protectChangesSinceButExcludingCommitId)
        {
            _tracker = tracker;
            _protectChangesSinceButExcludingCommitId = protectChangesSinceButExcludingCommitId;
        }

        public string ProtectChangesSinceButExcludingCommitId
        {
            get { return _protectChangesSinceButExcludingCommitId; }
        }

        public void InitializeUpdate()
        {
            if (_intermediateChanged != null) return;

            // get list of changes since we assembled our local-to-remote modification list.
            _intermediateChanged = _tracker.GetCommits(ProtectChangesSinceButExcludingCommitId).Flatten();
            var hasIntermediateChanges = _intermediateChanged.Deleted.Count > 0 ||
                                         _intermediateChanged.Modified.Count > 0;
            if (hasIntermediateChanges)
                Log.Info("protecting local changes since last sync: {0}/{1} modifications/deletions",
                    _intermediateChanged.Modified.Count, _intermediateChanged.Deleted.Count);

            _intermediateDeleted = new HashSet<TrackableId>(_intermediateChanged.Deleted.Select(p => p.Key));

            _intermediateModified = new HashSet<ModificationKey>();

            foreach (var mod in _intermediateChanged.Modified)
            {
                var journalKey = new TrackableId(mod.Object);
                var props = mod.ModifiedProperties;
                if (props == null) props = mod.Object.Properties;
                foreach (var prop in props)
                    _intermediateModified.Add(new ModificationKey(journalKey, prop));
            }
        }

        public ICollection<string> GetUpdatableColumns(TrackableId localTarget, ITrackable remoteSource,
                                                       ICollection<string> defaultUpdatedColumns)
        {
            InitializeUpdate();

            // do nothing on new objects without an existing ID.
            if (localTarget == null)
                return defaultUpdatedColumns;

            if (_intermediateDeleted.Contains(localTarget))
            {
                Log.Info("dropping remote changes to local deleted object {0}", localTarget);
                return new string[0];
            }

            HashSet<string> allowed = new HashSet<string>();
            List<string> protect = new List<string>();

            foreach (var c in defaultUpdatedColumns)
            {
                if (!_intermediateModified.Contains(new ModificationKey(localTarget, c)))
                    allowed.Add(c);
                else
                    protect.Add(c);
            }

            if (Log.IsInfoEnabled && protect.Count > 0 )
                Log.Info("protecting local intermediate changes of {0}: {1}", localTarget, string.Join(",", protect));

            return allowed;
        }

        public IEnumerable<TrackableId> GetDeletable(IEnumerable<TrackableId> localScheduledForDeletion)
        {
            foreach (var localKey in localScheduledForDeletion)
            {
                // initialize in loop, so we don't 
                // initialize if list is empty.
                InitializeUpdate();

                if (_intermediateDeleted.Contains(localKey) || _protectFromDeletion.Contains(localKey))
                {
                    // no reason to do a double-deletion, also protect from deletion.
                    continue;
                }

                yield return localKey;
            }
        }

        public void ProtectFromDeletion(IEnumerable<TrackableId> protectedFromDeletion)
        {
            _protectFromDeletion.UnionWith(protectedFromDeletion);
        }

        public struct ModificationKey
        {
            public readonly TrackableId Key;
            public readonly string Property;

            public ModificationKey(TrackableId key, string property)
            {
                Key = key;
                Property = property;
            }

            #region Equals & Hashcode

            public bool Equals(ModificationKey other)
            {
                return Equals(Key, other.Key) && string.Equals(Property, other.Property);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ModificationKey && Equals((ModificationKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ (Property != null ? Property.GetHashCode() : 0);
                }
            }

            #endregion
        }
    }
}