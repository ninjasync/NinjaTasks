using System.Diagnostics;

namespace NinjaSync.Model.Journal
{
    /// <summary>
    /// note that the journaling itself is agnostic of the 
    /// value of the ObjectType, so you can easily invent 
    /// new values and/or remove the old.
    /// <para>
    /// the order of the items should be as such that objects
    /// that depend on other objects should have higher values
    /// than those they depend on. eg. task, with a reference to 
    /// list, is listed below list.
    /// </para>
    /// <para/>
    /// the values are here for debugging purposes.
    /// </summary>
    public enum TrackableType
    {
        List,
        Task,
    };

    
    public class TrackableId
    {
        public TrackableId(TrackableType type, string objectId)
        {
            this.Type = type;
            this.ObjectId = objectId;

            Debug.Assert(ObjectId != null);
        }

        public TrackableId(ITrackable t)
        {
            this.Type = t.TrackableType;
            this.ObjectId = t.Id;

            Debug.Assert(ObjectId != null);
        }

        public TrackableType Type { get; private set; } 
        public string ObjectId { get; private set; }
        
        #region Equality, etc

        protected bool Equals(TrackableId other)
        {
            return Type == other.Type && string.Equals(ObjectId, other.ObjectId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TrackableId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Type*397) ^ (ObjectId != null ? ObjectId.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("Type: {0}, ObjectId: {1}", Type, ObjectId);
        }

        #endregion
    }
}