using System;
using System.Collections.Generic;

namespace NinjaSync.Model.Journal
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class Track : Attribute
    {
    }

    public static class TrackableProperties
    {
        public const string ColId = "Id";
        public const string ColCreatedAt = "CreatedAt";
        public const string ColModifiedAt = "ModifiedAt";
    }

    public interface ITrackable
    {
        string Id { get; set; }
        TrackableType TrackableType { get; }

        /// <summary>
        /// created at will be set - if not filled in by the application - 
        /// by the database upon first save.
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Handling ModifiedAt is at the applications descretion. If two 
        /// modifications occured at the same "logical time" - are conflicting
        /// modifications - ModifiedAt is used to help resolve the conflicts.
        /// </summary>
        DateTime ModifiedAt { get; set; }

        IList<string> Properties { get; }

        object GetProperty(string name);
        void SetProperty(string name, object value);

        ITrackable Clone();
        void CopyFrom(ITrackable source, ICollection<string> onlyTheseProperties=null);

        bool IsNew { get; }
        void SetNewId();
    }
}
