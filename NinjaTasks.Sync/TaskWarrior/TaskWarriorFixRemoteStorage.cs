using System;
using System.Linq;
using NinjaSync.Journaling;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTasks.Model.Sync;
using NinjaTools.Logging;
using NinjaTools.Progress;
using TaskWarriorLib.Network;

namespace NinjaTasks.Sync.TaskWarrior
{
    /// <summary>
    /// this class is meant to provide a seamless workaround for taskwarrior, who 
    /// - silently discards changes/new entries send to him on a fresh sync.
    /// </summary>
    public class TaskWarriorFixRemoteStorage : TaskWarriorRemoteStorage
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();


        public TaskWarriorFixRemoteStorage(ITslConnectionFactory tsl, ISyncStatusStorage storage, TaskWarriorAccount account) :
            base(tsl, storage, account)
        {
        }

        public override CommitList MergeModifications(CommitList commits, IProgress progress)
        {
            var pp = new PartitialProgress(progress);
            
            bool isFreshSync = string.IsNullOrEmpty(commits.RemoteCommitId);

            CommitList retCommitList = null, retCommitList2 = null;

            while (true)
            {
                // use progress only on first try.
                pp.NextStep(0.2f);

                IProgress useProgress = retCommitList == null ? (IProgress)pp : new NullProgress();

                CommitList uploadCommits = commits;

                if (isFreshSync && retCommitList==null)
                {
                    Log.Info("empty sync key / TaskWarrior does not accept uploads on fresh sync: will sychronize twice: uploading local changes in the second step.");
                    uploadCommits = new CommitList();
                }

                pp.NextStep(0.6f);
                // progress update...

                Log.Debug("uploading changes: from local/remote commit id: {0}/{1} to {2}",
                                    commits.BasedOnCommitId, commits.RemoteCommitId, commits.FinalCommitId);

                //the lists will already be mapped.
                var ret = base.MergeModifications(uploadCommits, useProgress);
                    
                if(ret == null)
                    throw new NullReferenceException();

                if (retCommitList == null)
                    retCommitList = ret;
                else
                    retCommitList2 = ret;

                if (isFreshSync && retCommitList2 == null)
                {
                    // since TaskWarrior does not do the merging 
                    // of our data with his, we have to do it on our own...

                    var merger = new CommitListMerger();
                    CommitList accepted; Commit keptAndRejected;
                    merger.CreateDiff(retCommitList, commits, out accepted, out keptAndRejected);

                    // re-insert all lists.
                    keptAndRejected.Modified.InsertRange(0, accepted.Commits.SelectMany(m=>m.Modified).OfTypeTodoList());

                    // we want to send accepted to task warrior, and keptAndRejected back to our caller.
                    accepted.RemoteCommitId = retCommitList.RemoteCommitId;
                    retCommitList.Commits.Clear();
                    retCommitList.Commits.Add(keptAndRejected);
                    retCommitList.RemoteCommitId = "invalid:temp";

                    commits = accepted;
                    
                    continue;
                }
                    

                break;
            }

            pp.NextStep(0.2f);

            if (retCommitList2 != null)
            {
                retCommitList.Commits.AddRange(retCommitList2.Commits);
                retCommitList.RemoteCommitId = retCommitList2.RemoteCommitId;
            }

            return retCommitList;
        }

    }
}
