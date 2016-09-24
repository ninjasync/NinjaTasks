using NinjaSync.Model.Journal;

namespace NinjaSync.Journaling
{
    public interface IModificationAssembler
    {
        ///// <summary>
        ///// this will commit all uncommited changes, so that all returned 
        ///// entries have a valid commit id.
        ///// </summary>
        ///// <param name="sinceButExcludingCommitId"></param>
        ///// <returns></returns>
        //Commit GetModified(string sinceButExcludingCommitId);

        /// <summary>
        /// this is the same as GetModified, exept that it will partition the
        /// changes into their respective commits.
        /// </summary>
        /// <param name="sinceButExcludingCommitId"></param>
        /// <returns></returns>
        CommitList GetCommits(string sinceButExcludingCommitId);
    }
}