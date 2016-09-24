using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NinjaSync.Exceptions;
using NinjaSync.Journaling;
using NinjaSync.Model;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTools;
using NinjaTools.Logging;

namespace NinjaSync.P2P
{

    /// <summary>
    /// This class acts as a mergin P2P Endpoint. It performs conflict merges 
    /// supported by commits and Modification date.
    /// <para/>
    /// </summary>
    public class TrackableStorageP2PSyncEndpoint : IP2PSyncEndpoint
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ITrackableJournalStorage _storage;
        private readonly ModificationAssembler _mod;

        public TrackableStorageP2PSyncEndpoint(ITrackableJournalStorage storage)
        {
            _storage = storage;
            _mod = new ModificationAssembler(_storage);
        }

        public IList<string> GetCommitIdsSince(string lastCommonCommitId)
        {
            return _storage.GetCommitIdsSince(lastCommonCommitId);
        }

        public string StorageId { get { return _storage.StorageId; }}

        /// <summary>
        /// will modify remoteCommits at random.
        /// </summary>
        /// <returns></returns>
        public CommitList MergeCommits(CommitList remoteCommits, ISyncProgress progress)
        {
            string commonAncestorCommit = remoteCommits.BasedOnCommitId;

            Log.Info("merging request on commit '{0}'", commonAncestorCommit);

            if(!_storage.HasCommit(commonAncestorCommit))
                throw new CommitNotFoundException(commonAncestorCommit);

            // special case: check if the remote commit is a "dummy commit", 
            // then the remote doesn't have any changes, but only tries to
            // grab our newest changes.
            // if so, do not try to apply the commit.
            if (remoteCommits.IsDummy)
            {
                var localCommits = _mod.GetCommits(commonAncestorCommit);
                
                progress.LocalDeleted += localCommits.DeletionCount;
                progress.LocalModified += localCommits.ModificationCount;
                
                return localCommits;
            }

            // sanity check
            for (int i = 1; i < remoteCommits.Commits.Count; ++i)
            {
                if(remoteCommits.Commits[i].BasedOnCommitId != remoteCommits.Commits[i-1].CommitId)
                    throw new ProtocolViolationException("Commits not in logical time order.");
            }

            var merger = new CommitListMerger();

            CommitList replyCommits = null;
            Commit mergedCommit = new Commit();

            _storage.RunInTransaction(() =>
            {
                var localCommits = _mod.GetCommits(commonAncestorCommit);
                replyCommits = new CommitList
                {
                    Commits = localCommits.Commits.ToList(), 
                    StorageId = _storage.StorageId
                };

                string lastCommitId = localCommits.FinalCommitId;
                
                if(replyCommits.IsDummy)
                    replyCommits.Commits.Clear();

                HashSet<string> skippedCommits = new HashSet<string>();

                foreach (var commit in remoteCommits.Commits)
                {
                    if (_storage.HasCommit(commit.CommitId))
                    {
                        Log.Info("not applying existing commit {0}. IsPlaceholder: {1}", commit.CommitId, commit.IsPlaceholder);
                        
                        skippedCommits.Add(commit.CommitId);

                        // don't send the commit back, make a placeholder.
                        int idx = replyCommits.Commits.FindIndex(p => p.CommitId == commit.CommitId);
                        if (idx != -1)
                        {
                            replyCommits.Commits[idx] = replyCommits.Commits[idx].ToPlaceholder();
                        }
                        continue;
                    }

                    if (commit.IsPlaceholder)
                    {
                        Log.Error("remote send invalid placeholder commit: " + commit.CommitId);
                        throw new CommitNotFoundException(commit.CommitId);
                    }

                    // stanity checks
                    if (!_storage.HasCommit(commit.BasedOnCommitId)
                        || !_storage.HasCommit(commit.BasedOnCommitId2))
                    {
                        string msg = string.Format("trying to apply a commit '{0}' for which we don't have the base commit '{1}' or '{2}'",
                                                   commit.CommitId, commit.BasedOnCommitId, commit.BasedOnCommitId2);
                        Log.Error(msg);
                        throw new CommitNotFoundException(_storage.HasCommit(commit.BasedOnCommitId)?commit.BasedOnCommitId2:commit.BasedOnCommitId);
                    }

                    Log.Info("applying commit: " + commit.CommitId);

                    Commit accepted, merged;
                    merger.CreateMerge(localCommits, commit, out accepted, out merged);

                    // also apply empty commits, so that we can put 
                    // their commit ids on our commit stack.

                    if (accepted.IsMergeCommit && skippedCommits.Contains(accepted.BasedOnCommitId))
                    {
                        // we are reordering the commits.
                        // make sure we don't loose our merge history.
                        accepted.BasedOnCommitId2 = accepted.BasedOnCommitId;
                    }
                    

                    ApplyCommit(accepted);

                    progress.RemoteDeleted += accepted.Deleted.Count;
                    progress.RemoteModified += accepted.Modified.Count;

                    accepted.BasedOnCommitId = lastCommitId;
                    replyCommits.Commits.Add(accepted.ToPlaceholder());

                    mergedCommit.Deleted.AddRange(merged.Deleted);
                    mergedCommit.Modified.AddRange(merged.Modified);

                    lastCommitId = accepted.CommitId;
                }

                // if neccessary, create a merge commit.
                if (!mergedCommit.IsEmpty)
                {
                    // flatten out possible duplicates.
                    mergedCommit = new CommitList(mergedCommit).Flatten();

                    // create the commit
                    RecreateJournalEntries(mergedCommit);
                    string mergedCommitId = _storage.CommitChanges();

                    // create reply
                    mergedCommit.CommitId = mergedCommitId;
                    mergedCommit.BasedOnCommitId = lastCommitId;
                    mergedCommit.BasedOnCommitId2 = localCommits.FinalCommitId;
                    replyCommits.Commits.Add(mergedCommit);

                    Log.Info("created merge commit '{0}' with {1}/{2} mod(s)/del(s) based on '{3}' and '{4}'",
                                mergedCommit.CommitId, mergedCommit.Modified.Count, mergedCommit.Deleted.Count,
                                mergedCommit.BasedOnCommitId, mergedCommit.BasedOnCommitId2);

                    lastCommitId = mergedCommitId;

                }

                if (!replyCommits.IsEmpty)
                {
                    if (Log.IsInfoEnabled)
                    {
                        Log.Info("sending back {0}/{1} mod(s)/del(s) in {2} commit(s) on latest commit-id '{3}'",
                            replyCommits.ModificationCount,
                            localCommits.DeletionCount,
                            localCommits.Commits.Count, replyCommits.FinalCommitId);
                    }
                }
                else
                {
                    Log.Info("sending back dummy commit '{0}'", lastCommitId);

                    if (replyCommits.Commits.Count == 0)
                        replyCommits = CommitList.CreateDummy(lastCommitId, _storage.StorageId);
                }
                
            });
            
            Debug.Assert(replyCommits.BasedOnCommitId == commonAncestorCommit);

            progress.LocalDeleted += replyCommits.DeletionCount;
            progress.LocalModified += replyCommits.ModificationCount;


            return replyCommits;
        }

        private void RecreateJournalEntries(Commit keptFromLocal)
        {
            // we want to recreate journal entries for all values, 
            // even those that did not change.
            // [actually, this list should only contain those value that did not change]

            foreach (var del in keptFromLocal.Deleted)
            {
                JournalEntry e = new JournalEntry();
                e.Type = del.Key.Type;
                e.ObjectId = del.Key.ObjectId;
                e.Timestamp = del.ModifiedAt;
                e.Change = ChangeType.Deleted;
                _storage.AddOrReplaceJournalEntry(e);
            }
            foreach (var mod in keptFromLocal.Modified)
            {
                JournalEntry e = new JournalEntry();
                e.Type = mod.Key.Type;
                e.ObjectId = mod.Key.ObjectId;

                var props = mod.ModifiedPropertiesEx;

                // never "recreate" an object, because that could lead to the 
                // paradox of an object being later in time created than modified.
                if (props == null)
                    props = mod.Object.Properties.Select(c => new ModifiedProperty(c, mod.ModifiedAt))
                                                 .ToList();
                foreach (var prop in props)
                {
                    e.Id = 0;
                    e.Change = ChangeType.Modified;
                    e.Timestamp = prop.ModifiedAt;
                    e.Member = prop.Property;
                    _storage.AddOrReplaceJournalEntry(e);
                }
            }


        }

        private void ApplyCommit(Commit commit)
        {
            if(commit.CommitId.IsNullOrEmpty())
                throw new ProtocolViolationException("can only apply commit with valid commit id.");

            // apply changes.
            foreach (var mod in commit.Modified)
            {
                if(Log.IsInfoEnabled)
                    Log.Info("saving {0} properties: {1}", mod.Key ?? new TrackableId(mod.ObjectType, "(new)"), mod.ModifiedProperties==null?"(all)":string.Join(",", mod.ModifiedProperties));

                _storage.Save(mod.Object,mod.ModifiedProperties);
            }

            // delete all
            _storage.Delete(SelectionMode.SelectSpecified,
                            commit.Deleted.Select(d => d.Key).ToArray());

            _storage.CommitChanges(commit.CommitId, commit.BasedOnCommitId2);
        }


        public CommitList GetMissingCommits(string lastCommonCommitId, string remoteStorageId, IList<string> remoteCommits)
        {
            HashSet<string> skipCommits = new HashSet<string>(remoteCommits);
            var ret = _mod.GetCommits(lastCommonCommitId);

            for (int i = 0; i < ret.Commits.Count; i++)
            {
                Commit commit = ret.Commits[i];
                if (skipCommits.Contains(commit.CommitId))
                    ret.Commits[i] = commit.ToPlaceholder();
            }

            return ret;
        }

        public void StoreCommits(CommitList myCommits, ISyncProgress progress)
        {
            MergeCommits(myCommits, progress);
        }
    }
}
