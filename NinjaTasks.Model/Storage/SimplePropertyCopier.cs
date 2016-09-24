using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NinjaTools;

#if !DOT42
using System.Linq.Expressions;
using NinjaTools.Npc;
#endif

namespace NinjaTasks.Model.Storage
{
    public interface IPropertyCopier
    {
        void Copy(object target, object source, string property);
    }

   
    public static class PropertyCopierExtensions
    {
        public static void Copy(this IPropertyCopier copy, object target, object source, params string[] properties)
        {
            foreach(var prop in properties)
                copy.Copy(target, source, prop);
        }
        public static void Copy(this IPropertyCopier copy, object target, object source, IEnumerable<string> properties)
        {
            foreach (var prop in properties)
                copy.Copy(target, source, prop);
        }

#if !DOT42
        public static void Copy<T>(this IPropertyCopier copy, T target, T source, params Expression<Func<T, object>>[] properties)
        {
            foreach(var prop in properties)
                copy.Copy(target, source, ExpressionHelper.GetMemberName(prop));
        }
#endif

    }

    /// <summary>
    /// shallow copies with reflection
    /// </summary>
    public class SimplePropertyCopier : IPropertyCopier
    {
        public void Copy(object target, object source, string prop)
        {
            var sourceProp = source.GetType().GetRuntimeProperty(prop);
            var targetProp = target.GetType().GetRuntimeProperty(prop);

            object val = sourceProp.GetValue(source);
            targetProp.SetValue(target, val);
        }
    }

#if !DOT42
    /// <summary>
    /// shallow copies, caches access to getters and setter.
    /// </summary>
    public class ReflectionCachingPropertyCopier: IPropertyCopier
    {
        private static readonly Dictionary<Tuple<Type,string>, Func<object, object>> Getters = new Dictionary<Tuple<Type, string>, Func<object, object>>();
        private static readonly Dictionary<Tuple<Type, string>, Action<object, object>> Setters = new Dictionary<Tuple<Type, string>, Action<object, object>>();


        public void Copy(object target, object source, string prop)
        {
            var setter = GetSetter(target, prop);
            var getter = GetGetter(source, prop);

            object val = getter(source);
            setter(target, val);
        }

        private static Action<object, object> GetSetter(object target, string prop)
        {
            var keytarget = Tuple.Create(target.GetType(), prop);
            Action<object, object> setter;

            if (!Setters.TryGetValue(keytarget, out setter))
            {
                setter = target.GetType().GetRuntimeProperty(prop).CreateSet<object>();
                Setters.Add(keytarget, setter);
            }
            return setter;
        }

        private static Func<object, object> GetGetter(object source, string prop)
        {
            var keysource = Tuple.Create(source.GetType(), prop);
            Func<object, object> getter;

            if (!Getters.TryGetValue(keysource, out getter))
            {
                getter = source.GetType().GetRuntimeProperty(prop).CreateGet<object>();
                Getters.Add(keysource, getter);
            }
            return getter;
        }
    }
#endif
}
