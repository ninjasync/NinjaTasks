using System;
using System.Collections.Generic;
using System.Linq;

#if !DOT42
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif 

namespace TaskWarriorLib
{
// ReSharper disable InconsistentNaming
    public enum Priority
    {
        H,
        M,
        L,
    }

    public enum Status
    {
        INVALID,
        pending,
        deleted,
        completed,
        waiting,
        recurring
    }
    // ReSharper restore InconsistentNaming

    public class Annotation
    {
        public string Description;
        public DateTime Entry;
    }

    public class TaskWarriorTask
    {
        // required.
        public string Uuid;
        public string Description;
        public Status Status;
        public DateTime Entry;

        // optional.

        public DateTime? Modified;

        public string Project;

        public Priority? Priority;

        public DateTime? Start;
        public DateTime? End;
        public DateTime? Due;
        public DateTime? Until;
        public DateTime? Scheduled;
        public DateTime? Wait;

        public string Recur;
        public int? Imask;
        public string Mask;

        public string Parent;

        //public IList<string> Depends { get; set; }
        public string Depends { get; set; }
        public IList<string> Tags { get; set; }
        public IList<Annotation> Annotations { get; set; }

        // My User Defined Attributes (UDA)
        public double? SortPosition { get; set; }

#if !DOT42
        [JsonIgnore]
#endif
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(Uuid)
                    && !string.IsNullOrEmpty(Description)
                    && Status != Status.INVALID
                    && Entry != DateTime.MinValue;
            }
        }

#if !DOT42
        [JsonIgnore]

        // extra fields
        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        [JsonIgnore]
        public IDictionary<string, string> AdditionalData { get { return _additionalData == null?null:_additionalData.ToDictionary(kv=>kv.Key, kv=>kv.Value.ToString(Formatting.None)); }}
        
        public void AddAdditionalData(string key, string jsonRepr)
        {
            if(_additionalData == null)
                _additionalData = new Dictionary<string, JToken>();
            _additionalData.Add(key, JToken.Parse(jsonRepr));
        }

        [JsonIgnore]
#endif
        public string JsonOriginalLine { get; set; }


        public void AddAnnotation(string description, DateTime entry)
        {
            if(Annotations == null)
                Annotations = new List<Annotation>();
            Annotations.Add(new Annotation {Description = description, Entry = entry});
        }
    }
}