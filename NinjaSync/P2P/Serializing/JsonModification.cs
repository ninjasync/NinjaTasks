using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NinjaSync.Model.Journal;

namespace NinjaSync.P2P.Serializing
{
    public enum ChangeType
    {
        Full,
        Deletion,
        Update
    }

    public class Mod
    {
        public string Property { get; set; }
        public DateTime ModifiedAt { get; set; }
        public object Value { get; set; }
    }

    public class JsonModification
    {
        public ChangeType Change { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TrackableType? Type { get; set; }

        /// <summary>
        /// this will be set for Deletion and Update, not for Full
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        /// <summary>
        /// this is only set for deletion.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DeletedAt { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Mod> Modifications { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData = new Dictionary<string, object>();
    }
}
