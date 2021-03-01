using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTools.Npc
{
    interface IGetterSetterFactory
    {
        Func<T, TValue> CreateGet<T, TValue>(PropertyInfo propertyInfo);
        Action<T, TValue> CreateSet<T, TValue>(PropertyInfo propertyInfo);
    }

    public static class PropertyInfoExtensions
    {
#if !DOT42
        private static IGetterSetterFactory _factory = new CompileExpressionGetterSetterFactory();
#else
        private class SimpleGetterSetterFactory : IGetterSetterFactory
        {
            public Func<T, TValue> CreateGet<T, TValue>(PropertyInfo propertyInfo)
            {
                return obj => (TValue)propertyInfo.GetValue(obj);
            }

            public Action<T, TValue> CreateSet<T, TValue>(PropertyInfo propertyInfo)
            {
                return (obj, val) => propertyInfo.SetValue(obj, val);
            }
        }
        private static IGetterSetterFactory _factory = new SimpleGetterSetterFactory();
#endif

        public static Func<T, object> CreateGet<T>(this PropertyInfo propertyInfo)
        {
            return _factory.CreateGet<T, object>(propertyInfo);
        }
        public static Func<T, TValue> CreateGet<T, TValue>(this PropertyInfo propertyInfo)
        {
            return _factory.CreateGet<T, TValue>(propertyInfo);
        }

        public static Action<T, object> CreateSet<T>(this PropertyInfo propertyInfo)
        {
            return _factory.CreateSet<T, object>(propertyInfo);
        }

        public static Action<T, TValue> CreateSet<T, TValue>(this PropertyInfo propertyInfo)
        {
            return _factory.CreateSet<T, TValue>(propertyInfo);
        }

        /// <summary>
        /// this returns a setter delegate for which object type and 
        /// property type are not fixed at compile time.
        /// </summary>
        public static Action<object, object> CreateObjectSetter(this PropertyInfo property)
        {
            return _factory.CreateSet<object, object>(property);
        }

        ///// <summary>
        ///// this returns a getter delegate for which object type and 
        ///// property type are not fixed at compile time.
        ///// </summary>
        public static Func<object, object> CreateObjectGetter(this PropertyInfo property)
        {
            return _factory.CreateGet<object,object>(property);
        }
    }
}
