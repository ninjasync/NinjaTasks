using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NinjaSync.Model.Journal
{
    public class ModifiedProperty
    {
        public readonly string Property;
        public readonly DateTime ModifiedAt;

        public ModifiedProperty(string property, DateTime modifiedAt)
        {
            Property = property;
            ModifiedAt = modifiedAt;
        }
    }
    
    [DebuggerDisplay("{ObjectType} {Key.ObjectId} ModCount={ModifiedProperties.Count}")]
    public class Modification
    {
        // always set for local changes; can be null for new remote changes.
        public readonly TrackableId Key;

        /// <summary>
        /// set on modified or created.
        /// </summary>
        public readonly ITrackable Object;

        /// <summary>
        /// modification date. can be updated.
        /// </summary>
        public DateTime ModifiedAt;

        /// <summary>
        /// create a "deleted" object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modifiedAt"></param>
        public Modification(TrackableId key, DateTime modifiedAt)
        {
            Key = key;
            ModifiedAt = modifiedAt;
        }

        /// <summary>
        /// create a newly created object
        /// </summary>
        /// <param name="obj"></param>
        public Modification(ITrackable obj)
        {
            if (obj.Id != null) Key = new TrackableId(obj);
            Object = obj;
            ModifiedAt = obj.ModifiedAt;
        }

        /// <summary>
        /// a modification.
        /// </summary>
        public Modification(ITrackable obj, List<ModifiedProperty> modifiedProperties)
        {
            if(obj.Id != null) Key = new TrackableId(obj);

            ModifiedPropertiesEx = modifiedProperties;

            Object = obj;
            ModifiedAt = obj.ModifiedAt;
        }



        // null on deletion or creation.
        public IList<string> ModifiedProperties { get {  return ModifiedPropertiesEx==null?null:ModifiedPropertiesEx.Select(p=>p.Property).ToArray();} }
        public readonly List<ModifiedProperty> ModifiedPropertiesEx;

        //public bool IsCreation { get { return Object != null && ModifiedProperties == null; } }
        //public bool IsModification { get { return Object != null && ModifiedProperties != null; } }
        public bool IsDeletion { get { return Object == null; } }


        public TrackableType ObjectType { get { return Key != null ? Key.Type : Object.TrackableType; } }
    }
}