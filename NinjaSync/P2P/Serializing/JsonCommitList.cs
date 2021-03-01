using System.Collections.Generic;

namespace NinjaSync.P2P.Serializing
{
    public class JsonCommitList
    {
        public int DeletionCount { get; set; }
        public int ModificationCount { get; set; }

        public string BasedOnCommitId { get; set; }
        
        public string StorageId { get; set; }

        public List<JsonCommit> Commits { get; set; }

        public JsonCommitList()
        {
            Commits = new List<JsonCommit>();
        }
    }
}
