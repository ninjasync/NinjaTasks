using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NinjaTools.Npc
{
    public static class LateBoundPropertyHelper
    {
        /// <summary>
        /// this helps with getting a delegate for a property that type is not known.
        /// </summary>
        public static Func<T, object> CreateGet<T>(this PropertyInfo property)
        {
            return obj => property.GetValue(obj, null);
        }

        /// <summary>
        /// this returns a setter for a property that type that type is not fixed at compile time.
        /// </summary>
        public static Action<T, object> CreateSet<T>(this PropertyInfo property)
        {
            return (obj, value) => property.SetValue(obj, value, null);
        }

        public static Action<object, object> CreateObjectSetter(this PropertyInfo property)
        {
            return (p, o) => property.SetValue(p, o, null);
        }

        public static Func<object, object> CreateObjectGetter(this PropertyInfo property)
        {
            return (p) => property.GetValue(p, null);
        }
    }
}
