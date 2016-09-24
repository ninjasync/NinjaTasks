using System.Collections.Generic;
using Newtonsoft.Json;

namespace NinjaSync.P2P.Serializing
{
    public class JsonCommit
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BasedOnCommitId2 { get; set; }

        public string CommitId { get; set; }

        public List<JsonModification> Changes { get; set; }
    }
}