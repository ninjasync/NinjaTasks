using System;
using System.Collections.Generic;
using System.Reflection;
using NinjaTools.Npc.Reflection;

namespace NinjaTools.Npc.Helpers
{
    public static class PropertyInfoCache
    {
        private static readonly Dictionary<Tuple<Type, string>, PropertyInfo> PropertyCache = new Dictionary<Tuple<Type, string>, PropertyInfo>();
        private static readonly Dictionary<Tuple<Type,PropertyInfo>, object> GetterCache = new Dictionary<Tuple<Type, PropertyInfo>, object>();
        private static readonly Dictionary<Tuple<Type, PropertyInfo>, object> SetterCache = new Dictionary<Tuple<Type, PropertyInfo>, object>();
        private static readonly Dictionary<PropertyInfo, object> ObjectSetterCache = new Dictionary<PropertyInfo, object>();

#if DOT42 // for performance.
        private static readonly BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
#else
        private const           BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
#endif

        public static PropertyInfo GetInstanceProperty(Type type, string propertyName)
        {
            var key = Tuple.Create(type, propertyName);

            PropertyInfo propertyInfo;

            lock (PropertyCache)
            {
                if (PropertyCache.TryGetValue(key, out propertyInfo))
                    return propertyInfo;
            }

            propertyInfo = FindProperty(type, propertyName);

            lock (PropertyCache)
                PropertyCache[key] = propertyInfo;

            return propertyInfo;
        }

        private static PropertyInfo FindProperty(Type type, string propertyName)
        {
            // we also want to return private properties/setters.
            return type.GetProperty(propertyName, InstanceBindingFlags);
        }

        public static Func<T,object> GetGetter<T>(PropertyInfo prop)
        {
            var key = Tuple.Create(typeof(T), prop);
            lock (GetterCache)
            {
                object val;
                if (!GetterCache.TryGetValue(key, out val))
                {
                    val = prop.CreateGet<T>();
                    GetterCache[key] = val;
                }
                return (Func<T, object>) val;
            }
        }

        public static Action<T, object> GetSetter<T>(PropertyInfo prop)
        {
            var key = Tuple.Create(typeof(T), prop);
            lock (SetterCache)
            {
                object val;

                if (!SetterCache.TryGetValue(key, out val))
                {
                    val = prop.CreateSet<T>();
                    SetterCache[key] = val;
                }
                return (Action<T, object>)val;
            }
        }

        public static Action<object, object> GetObjectSetter(PropertyInfo prop)
        {
            lock (ObjectSetterCache)
            {
                object val;

                if (!ObjectSetterCache.TryGetValue(prop, out val))
                {
                    val = prop.CreateObjectSetter();
                    ObjectSetterCache[prop] = val;
                }
                return (Action<object, object>)val;
            }
        }
    }

}
