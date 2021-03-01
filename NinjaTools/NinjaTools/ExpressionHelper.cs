using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NinjaTools
{
    [Obsolete("use nameof()")]
    public static class ExpressionHelper
    {
        public static MemberExpression FindMemberExpression<T>(Expression<Func<T>> expression)
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

        public static string GetMemberName<TX>(Expression<Func<TX, object>> expression)
        {
            return FindMemberExpression(expression).Member.Name;
        }

        public static string GetMemberName(Expression<Func<object>> expression)
        {
            return FindMemberExpression(expression).Member.Name;
        }

        public static IList<string> ToMemberNames<TX>(IEnumerable<Expression<Func<TX, object>>> expressions)
        {
            return expressions.Select(GetMemberName).ToList();
        }
    }
}
