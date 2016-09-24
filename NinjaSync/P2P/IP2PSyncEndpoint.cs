using System.Collections.Generic;
using NinjaSync.Model.Journal;

namespace NinjaSync.P2P
{
    public interface IP2PSyncLocalEndpoint
    {
        string StorageId { get; }

        CommitList MergeCommits(CommitList myNewCommits, ISyncProgress progress);
        IList<string> GetCommitIdsSince(string lastCommonCommitId);
    }
    public interface IP2PSyncRemoteEndpoint
    {
        CommitList GetMissingCommits(string lastCommonCommitId, string myStorageId, IList<string> myCommits);
        void StoreCommits(CommitList myCommits, ISyncProgress progress);
    }

    public interface IP2PSyncEndpoint : IP2PSyncLocalEndpoint, IP2PSyncRemoteEndpoint
    {
    }
}
