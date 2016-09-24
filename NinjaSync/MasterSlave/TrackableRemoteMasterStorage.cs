using System.Collections.Generic;
using System.Linq;
using NinjaSync.Journaling;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTools;
using NinjaTools.Logging;
using NinjaTools.Progress;

namespace NinjaSync.MasterSlave
{
    /// <summary>
    /// gives the ITrackableRemoteMasterStorage interface to an ITrackableJournalStorage. 
    /// This is the smart wrapper, that acts as a merging master and 
    /// performs conflict merges supported by commits and modification date.
    /// <para/>
    /// has a different storing paradigma than P2P,and should probably not be used in
    /// conjunction with it in the same database. [on the other hand, why not?]
    /// </summary>
    public class TrackableRemoteMasterStorage : ITrackableRemoteMasterStorage
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ITrackableJournalStorage _storage;
        private readonly ModificationAssembler _mod;
        private CommitList _reply;

        public TrackableRemoteMasterStorage(ITrackableJournalStorage storage)
        {
            _storage = storage;
            _mod = new ModificationAssembler(_storage);
        }

        /// <summary>
        /// here a conflict merging takes place.
        /// </summary>
        public CommitList MergeModifications(CommitList remoteCommits, IProgress progress)
        {
            string commonAncestorCommit = remoteCommits.RemoteCommitId;

            Log.Info("merging request on commit '{0}'", remoteCommits.RemoteCommitId);

            if (remoteCommits.IsDummy)
            {
                // special case: check if the remote commit is a "dummy commit", only there to
                // record the BasedOnCommitId.
                // if so, do not try to apply the commit.
                remoteCommits.Commits.Clear();
            }

            _storage.RunInTransaction(() =>
            {
                var localCommits = _mod.GetCommits(commonAncestorCommit);
                CommitList accepted;
                Commit keptFromLocal;
                var merger = new CommitListMerger();

                // we want to rebase our commits on top of remoteCommits.
                // accepted means, we will overwrite with accepted, after applying remote commits.
                // rejected means, remoteCommits can overwrite us.
                merger.CreateDiff(localCommits, remoteCommits, out accepted, out keptFromLocal);

                foreach (var commit in accepted.Commits.Where(c=>!c.IsEmpty))
                    SaveCommit(commit);

                // commit all changes.
                string replyCommitId = _storage.CommitChanges();


                // If we had any previous changes force a new virtual 
                // commit, so that our commitId differs from the client's.
                if (keptFromLocal.IsEmpty)
                {
                    _reply = new CommitList(new Commit {BasedOnCommitId = replyCommitId, CommitId = replyCommitId});
                    Log.Info("sending back empty dummy commit list on latest commit-id '{0}'", replyCommitId);
                }
                else if (accepted.IsEmpty)
                {
                    _reply = localCommits;

                    if (Log.IsInfoEnabled)
                    {
                        Log.Info("sending back {0}/{1} mod(s)/del(s) in {2} commit(s) on latest commit-id '{3}'",
                            localCommits.ModificationCount,
                            localCommits.DeletionCount,
                            localCommits.Commits.Count, replyCommitId);
                    }
                }
                else
                {
                    keptFromLocal.BasedOnCommitId = localCommits.BasedOnCommitId;
                    keptFromLocal.CommitId = replyCommitId;

                    _reply = new CommitList();
                    _reply.Commits.Add(keptFromLocal);

                    Log.Info("sending back {0}/{1} mod(s)/del(s) on new commit '{2}'. ",
                        keptFromLocal.Modified.Count, keptFromLocal.Deleted.Count, replyCommitId);
                }

                _reply.RemoteCommitId = replyCommitId;
            });

            return _reply;
        }

        private void SaveCommit(Commit commit)
        {
            // delete all
            _storage.Delete(SelectionMode.SelectSpecified,
                            commit.Deleted.Select(d => d.Key).ToArray());
            // apply changes.
            foreach (var mod in commit.Modified)
            {
                if(Log.IsInfoEnabled)
                    Log.Info("saving {0} properties: {1}", mod.Key ?? new TrackableId(mod.ObjectType, "(new)"), mod.ModifiedProperties==null?"(all)":string.Join(",", mod.ModifiedProperties));

                _storage.Save(mod.Object, mod.ModifiedProperties);
            }
        }

        public CommitList SaveModificationsForIds(CommitList commits, IProgress p)
        {
            foreach (var obj in commits.Commits.SelectMany(m=>m.Modified).Where(o => o.Object.IsNew))
                obj.Object.Id = SequentialGuid.NewGuidString();
            return commits;
        }

        public TrackableType[] SupportedTypes { get { return new[] { TrackableType.List, TrackableType.Task }; } }
        public IList<string> GetMappedColums(TrackableType type) { return null; }
    }
}
