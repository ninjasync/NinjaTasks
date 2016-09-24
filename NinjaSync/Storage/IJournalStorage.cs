using System.Collections.Generic;
using NinjaSync.Model;

namespace NinjaSync.Storage
{
    /// <summary>
    /// Manages a journal of changes to Individual Fields of objects. For midification of fields,
    /// tracks only the last modification. For deletion/creation tracks only last last event of
    /// each type (e.g. when an object is deleted than recreated, there will be only one creation
    /// record for the object, no deletion record.)
    /// </summary>
    public interface IJournalStorage
    {
        /// <summary>
        /// commits all uncommited changes (if any)
        /// and returns the new (or newest) commit id.
        /// </summary>
        /// <returns></returns>
        /// <param name="setCommitId">if not null, this will force aspecific commit id
        ///     (which also will be returned). will create an empty commit if no changes
        ///     are present.</param>
        /// <param name="mergeSecondBaseCommitId">set to a previous commit 
        /// id to record a merging Commit.</param>
        string CommitChanges(string setCommitId = null, string mergeSecondBaseCommitId=null);

        /// <summary>
        /// retreives the last CommitId
        /// </summary>
        /// <returns>
        /// the empty string "" if the database is empty, 
        /// null if there are uncommited changes in the database,
        /// the last commit id otherwise.
        /// </returns>
        string GetLastCommitId();

        /// <summary>
        /// this returns all change-tracking records since the specified commit id.
        /// the entries are ordered in their natural order.
        /// </summary>
        /// <param name="sinceButExcludingCommitId">if empty returns all modifications</param>
        /// <returns>specify if you are interested in uncommited changes.</returns>
        IEnumerable<CommitEntry> GetCommits(string sinceButExcludingCommitId);

        void AddOrReplaceJournalEntry(JournalEntry journalEntry);
        
        IList<string> GetCommitIdsSince(string lastCommonCommitId);
        bool HasCommit(string commitId);

        /// <summary>
        /// this deletes everyting.
        /// </summary>
        void Clear();
    }

}
