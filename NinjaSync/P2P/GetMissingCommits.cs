using System.Collections.Generic;

namespace NinjaSync.P2P
{
    public class GetMissingCommits
    {
        public string LastCommonCommitId { get; set; }
        public string StorageId { get; set; }

        public IList<string> CommitIds { get; set; }
    }
}
