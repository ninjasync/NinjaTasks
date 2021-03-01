using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NinjaTools.Sqlite;
using NinjaSync.Model.Journal;
using NinjaTools;

using PropertyChanged;
using NinjaTools.Npc;

namespace NinjaSync.Model
{
    public interface ITrackableWithAdditionalProperties : ITrackable
    {
        IList<string> AdditionalProperties { get; }
    }

    [DataContract]
    public abstract class TrackableBase : ITrackableWithAdditionalProperties, INotifyPropertyChanged
    {
        [PrimaryKey][DataMember]
        public string Id { get; set; }

        [NotNull][DataMember]
        public DateTime CreatedAt { get; set; }

        [Track, Indexed, NotNull][DataMember]
        public DateTime ModifiedAt { get; set; }

        [Ignore]
        public bool IsNew { get { return Id.IsNullOrEmpty(); } }

        [Ignore, DoNotNotify]
        public abstract TrackableType TrackableType { get; }

        public void SetNewId()
        {
            Id = SequentialGuid.NewGuidString();
        }

        public IList<string> Properties { get { return BaseProperties[GetType()].Concat(_additionalProperties.Keys).ToList(); } }
        public IList<string> AdditionalProperties { get { return _additionalProperties.Keys.ToList(); } }

        private readonly Dictionary<string, string> _additionalProperties = new Dictionary<string, string>();

        public object GetProperty(string name)
        {
            Func<object, object> getter;
            if (BaseGetters.TryGetValue(new PropertyKey(GetType(), name), out getter))
                return getter(this);

            string ret;
            _additionalProperties.TryGetValue(name, out ret);
            return ret;
        }

        public Type GetPropertyType(string name)
        {
            if (BasePropertyType.TryGetValue(new PropertyKey(GetType(), name), out var type))
                return type;

            // additional properties are only saved as string for now.
            return typeof(string);
        }

        public void SetProperty(string name, object value)
        {
            Debug.Assert(!name.IsNullOrEmpty());
            if(name.IsNullOrEmpty())
                throw new ArgumentException("name must not be null or empty", "name");

            Action<object, object> setter;
            if (BaseSetters.TryGetValue(new PropertyKey(GetType(), name), out setter))
                setter(this, value);
            else
            {
                string strValue = value?.ToString();
                if (strValue == null)
                    _additionalProperties.Remove(name);
                else 
                    _additionalProperties[name] = strValue;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }

        }

        #region Property Reflection & Copy & Clone

        protected TrackableBase()
        {
            Debug.Assert(BaseProperties.ContainsKey(GetType()), "be sure to call SetupProperties in static constructor of " + GetType().FullName);
        }

        private class PropertyKey
        {
            public readonly Type Type;
            public readonly string Name;

            public PropertyKey(Type actualType, string prop)
            {
                Type = actualType;
                Name = prop;
            }

            public bool Equals(PropertyKey other)
            {
                return Type == other.Type && string.Equals(Name, other.Name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (PropertyKey)) return false;
                return Equals((PropertyKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Type.GetHashCode()*397) ^ Name.GetHashCode();
                }
            }
        }

        private static readonly Dictionary<PropertyKey, Func<object, object>> BaseGetters = new Dictionary<PropertyKey, Func<object, object>>();
        private static readonly Dictionary<PropertyKey, Action<object, object>> BaseSetters = new Dictionary<PropertyKey, Action<object, object>>();
        private static readonly Dictionary<Type, string[]> BaseProperties = new Dictionary<Type, string[]>();
        private static readonly Dictionary<PropertyKey, Type> BasePropertyType = new Dictionary<PropertyKey, Type>();

        /// <summary>
        /// NOTE: It is assumed that all initialization of all Inheriting classes is done 
        ///       from a single thread, before any mulithreaded access to any Trackable
        /// </summary>
        protected static void SetupProperties(Type actualType, params string[] baseProperties)
        {
            BaseProperties.Add(actualType, baseProperties);

            foreach (var prop in baseProperties)
            {
                var key = new PropertyKey(actualType, prop);

                var runtimeProperty = actualType.GetRuntimeProperty(prop);
                var setter = runtimeProperty.CreateObjectSetter();
                var getter = runtimeProperty.CreateObjectGetter();
                
                BaseGetters.Add(key, getter);
                BaseSetters.Add(key, setter);
                BasePropertyType.Add(key, runtimeProperty.PropertyType);
            }
        }
      
        public virtual void CopyFrom(ITrackable source, ICollection<string> onlyTheseProperties = null)
        {
            if (onlyTheseProperties == null)
            {
                // set all values. also means: reset additional properties.
                _additionalProperties.Clear();

                foreach (var prop in source.Properties)
                    SetProperty(prop, source.GetProperty(prop));
            }
            else
            {
                // only copy specified properties
                foreach(var prop in onlyTheseProperties)
                    SetProperty(prop, source.GetProperty(prop));
            }
        }

        public virtual ITrackable Clone()
        {
            ITrackable ret = (ITrackable)Activator.CreateInstance(this.GetType());
            ret.CopyFrom(this);
            return ret;
        }


       
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        
    }
}