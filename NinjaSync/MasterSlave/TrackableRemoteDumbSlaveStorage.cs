using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTools.Progress;

namespace NinjaSync.MasterSlave
{
    /// <summary>
    /// gives the ITrackableRemoteSlaveStorage interface to an ITrackableStorage. 
    /// When used with a local ITrackableStorage, it simulates a "dumb" remote, 
    /// that always sends all data. Usefull for testing the sync-algorithms.
    /// </summary>
    public class TrackableRemoteDumbSlaveStorage : ITrackableRemoteSlaveStorage
    {
        private readonly ITrackableStorage _storage;

        public TrackableRemoteDumbSlaveStorage(ITrackableStorage storage)
        {
            _storage = storage;
        }

        public CommitList GetModifications(string commitsNewerButExcludingCommitId, IProgress p)
        {
            Debug.Assert(commitsNewerButExcludingCommitId == null);

            var ret = new Commit();
            CommitList retlist = new CommitList();
            retlist.Commits.Add(ret);
            
            ret.Modified.AddRange(_storage.GetTrackable().Select(l => new Modification(l)));
          

            p.Progress = 1;
            return retlist;
        }

        public CommitList SaveModifications(CommitList list, IProgress progress)
        {
            Commit commit = list.Flatten();
            _storage.Delete(SelectionMode.SelectSpecified, commit.Deleted.Select(p => p.Key).ToArray());

            foreach (var m in commit.Modified)
                _storage.Save(m.Object, m.ModifiedProperties);
           
            return list;
        }

        public CommitList SaveModificationsForIds(CommitList commits, IProgress p)
        {
            SaveModifications(commits, p);
            return new CommitList();
        }

        public TrackableType[] SupportedTypes { get { return new[] { TrackableType.List, TrackableType.Task }; } }
        public IList<string> GetMappedColums(TrackableType type) { return null; }
    }
}
