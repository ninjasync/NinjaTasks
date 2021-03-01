using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#if !DOT42
namespace NinjaTools.GUI.MVVM.Zools
{
    public static class Reflection
    {
        public static PropertyInfo GetPropertyInfoFromExpression<T>(object target, Expression<Func<T>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            MemberExpression memberExpression = FindMemberExpression(expression);
            if (memberExpression == null)
                throw new ArgumentException("Wrong expression\nshould be called with expression like\n() => PropertyName", "expression");
            PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Wrong expression\nshould be called with expression like\n() => PropertyName", "expression");
            if (propertyInfo.DeclaringType == null)
                throw new ArgumentException("Wrong expression\nshould be called with expression like\n() => PropertyName", "expression");
            if (target != null && !propertyInfo.DeclaringType.IsInstanceOfType(target))
                throw new ArgumentException("Wrong expression\nshould be called with expression like\n() => PropertyName", "expression");
            if (propertyInfo.GetGetMethod(true).IsStatic)
                throw new ArgumentException("Wrong expression\nshould be called with expression like\n() => PropertyName", "expression");
            else
                return propertyInfo;
        }

        private static MemberExpression FindMemberExpression<T>(Expression<Func<T>> expression)
        {
            if (!(expression.Body is UnaryExpression))
                return expression.Body as MemberExpression;
            MemberExpression memberExpression = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Wrong unary expression\nshould be called with expression like\n() => PropertyName", "expression");
            else
                return memberExpression;
        }

        public static MemberExpression FindMemberExpression<TX, T>(Expression<Func<TX, T>> expression)
        {
            if (!(expression.Body is UnaryExpression))
                return expression.Body as MemberExpression;
            MemberExpression memberExpression = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Wrong unary expression\nshould be called with expression like\nx => x.PropertyName", "expression");
            else
                return memberExpression;
        }

    }
}
#endif