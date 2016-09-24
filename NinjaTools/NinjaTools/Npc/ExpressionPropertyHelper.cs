using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NinjaTools.Npc.Reflection;

namespace NinjaTools.Npc
{
    public static class ExpressionPropertyHelper
    {
        public static Func<T, object> CreateGet<T>(this PropertyInfo propertyInfo)
        {
            return CreateGet<T, object>(propertyInfo);
        }

        public static Func<T, TValue> CreateGet<T, TValue>(this PropertyInfo propertyInfo)
        {
            ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");

            Type instanceType = typeof(T);
            Type resultType = typeof(object);

            ParameterExpression parameterExpression = Expression.Parameter(instanceType, "instance");
            Expression resultExpression;

            MethodInfo getMethod = propertyInfo.GetGetMethod(true);

            if (getMethod.IsStatic)
            {
                resultExpression = Expression.MakeMemberAccess(null, propertyInfo);
            }
            else
            {
                Expression readParameter = EnsureCastExpression(parameterExpression, propertyInfo.DeclaringType);

                resultExpression = Expression.MakeMemberAccess(readParameter, propertyInfo);
            }

            resultExpression = EnsureCastExpression(resultExpression, resultType);

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Func<T, TValue>), resultExpression, parameterExpression);

            Func<T, TValue> compiled = (Func<T, TValue>)lambdaExpression.Compile();
            return compiled;
        }

        public static Action<T, object> CreateSet<T>(this PropertyInfo propertyInfo)
        {
            return CreateSet<T, object>(propertyInfo);
        }

        public static Action<T, TValue> CreateSet<T, TValue>(this PropertyInfo propertyInfo)
        {
            ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");

            // use reflection for structs
            // expression doesn't correctly set value
            if (propertyInfo.DeclaringType.IsValueType())
                return (o, v) => propertyInfo.SetValue(o, v, null);

            Type instanceType = typeof(T);
            Type valueType = typeof(object);

            ParameterExpression instanceParameter = Expression.Parameter(instanceType, "instance");

            ParameterExpression valueParameter = Expression.Parameter(valueType, "value");
            Expression readValueParameter = EnsureCastExpression(valueParameter, propertyInfo.PropertyType);

            MethodInfo setMethod = propertyInfo.GetSetMethod(true);

            Expression setExpression;
            if (setMethod.IsStatic)
            {
                setExpression = Expression.Call(setMethod, readValueParameter);
            }
            else
            {
                Expression readInstanceParameter = EnsureCastExpression(instanceParameter, propertyInfo.DeclaringType);

                setExpression = Expression.Call(readInstanceParameter, setMethod, readValueParameter);
            }

            LambdaExpression lambdaExpression = Expression.Lambda(typeof(Action<T, TValue>), setExpression, instanceParameter, valueParameter);

            Action<T, TValue> compiled = (Action<T, TValue>)lambdaExpression.Compile();
            return compiled;
        }

        private static Expression EnsureCastExpression(Expression expression, Type targetType)
        {
            Type expressionType = expression.Type;

            // check if a cast or conversion is required
            if (expressionType == targetType || (!expressionType.IsValueType() && targetType.IsAssignableFrom(expressionType)))
                return expression;

            return Expression.Convert(expression, targetType);
        }

        //interface ICastingSetter
        //{
        //    void Set(object obj, object value);
        //}

        //class CastingSetter<TTarget, TValue> : ICastingSetter
        //{
        //    private readonly Action<TTarget, TValue> _del;
        //    public CastingSetter(Action<TTarget, TValue> del)
        //    {
        //        _del = del;
        //    }
        //    public void Set(object obj, object value)
        //    {
        //        _del((TTarget)obj, (TValue)value);
        //    }
        //}

        //interface ICastingGetter
        //{
        //    object Get(object obj);
        //}

        //class CastingGetter<TTarget, TValue> : ICastingGetter
        //{
        //    private readonly Func<TTarget, TValue> _del;
        //    public CastingGetter(Func<TTarget, TValue> del)
        //    {
        //        _del = del;
        //    }
        //    public object Get(object obj)
        //    {
        //        return _del((TTarget)obj);
        //    }
        //}

        /// <summary>
        /// this returns a setter delegate for which object type and 
        /// property type are not fixed at compile time.
        /// </summary>
        public static Action<object, object> CreateObjectSetter(this PropertyInfo property)
        {
            return CreateSet<object>(property);
            //var createSet = typeof(ExpressionPropertyHelper).GetRuntimeMethods().Single(m=>m.Name == "CreateSet" && m.GetGenericArguments().Length == 1);
            //var createSetBound = createSet.MakeGenericMethod(property.DeclaringType);
            //var nativeSetter = createSetBound.Invoke(null, new object[] { property });

            //var castingSetterType = typeof (CastingSetter<,>).MakeGenericType(property.DeclaringType, property.PropertyType);
            //var castingSetter = (ICastingSetter)Activator.CreateInstance(castingSetterType, nativeSetter);
            //return castingSetter.Set;
        }

        /// <summary>
        /// this returns a getter delegate for which object type and 
        /// property type are not fixed at compile time.
        /// </summary>
        public static Func<object, object> CreateObjectGetter(this PropertyInfo property)
        {
            return CreateGet<object>(property);
            //var createSet = typeof(ExpressionPropertyHelper).GetRuntimeMethods().Single(m => m.Name == "CreateGet" && m.GetGenericArguments().Length == 1);
            //var createSetBound = createSet.MakeGenericMethod(property.DeclaringType);
            //var nativeSetter = createSetBound.Invoke(null, new object[] { property });

            //var castingSetterType = typeof(CastingGetter<,>).MakeGenericType(property.DeclaringType, property.PropertyType);
            //var castingSetter = (ICastingGetter)Activator.CreateInstance(castingSetterType, nativeSetter);
            //return castingSetter.Get;
        }
    }
}
