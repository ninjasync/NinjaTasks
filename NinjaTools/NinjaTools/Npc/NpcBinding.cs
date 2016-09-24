using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using NinjaTools.Npc.Helpers;


namespace NinjaTools.Npc
{
    /// <summary>
    /// Enables Binding of Properties of INotifyPropertyChanged implementing classes.
    /// 
    /// <para>
    /// Note that even for weak bindings, the returned token holds a strong reference to source 
    /// and target (this could be changed though). For weak bindings, source will not hold a 
    /// strong reference to us. The subscription will be automatically unsubscribed during garbage
    /// collection when the returned token is collected. 
    /// </para>
    /// </summary>
    public static partial class NpcBinding
    {
        /// <summary>
        /// Creates a Two-Way binding between source1.propertyExpression1 and source2.propertyExpression2
        /// <para/>
        /// if immediatelySetValue is true, the the second value will be set to the first.
        /// <para/>
        /// can bind to private setters
        /// <para/>
        /// does not prevent infinite ping-pong setting of the same value, so your INPC-Setter should have
        /// an Equals-Noop check.
        /// </summary>
        public static IDisposable TwoWayBind<TSource1, TSource2, TValue>(
                                        this TSource1 source1, Expression<Func<TSource1, TValue>> propertyExpression1,
                                        TSource2 source2, Expression<Func<TSource2, TValue>> propertyExpression2,
                                        bool immediatelySetValue = true)
            where TSource1 : INotifyPropertyChanged
            where TSource2 : INotifyPropertyChanged
        {
            string name1 = GetMemberInfo(propertyExpression1).Name;
            string name2 = GetMemberInfo(propertyExpression2).Name;

            // TODO: it would be possible to detect and stop infinitive ping-pong-setting of the same
            //       value. never needed it though, since PropertyChanged.Fody handles this pretty good.
            return new TwoWayBinding(new OneWayBinding<TSource1, TSource2, TValue>(source1, name1, source2, name2, false, immediatelySetValue),
                                     new OneWayBinding<TSource2, TSource1, TValue>(source2, name2, source1, name1, false, false));
        }

        /// <summary>
        /// Creates a Two-Way binding between source1.propertyExpression1 and source2.propertyExpression2
        /// <para/>
        /// if immediatelySetValue is true, the the second value will be set to the first.
        /// <para/>
        /// can bind to private setters
        /// <para/>
        /// does not prevent infinite ping-pong setting of the same value, so your INPC-Setter should have
        /// an Equals-Noop check.
        /// </summary>
        [Pure]
        public static IDisposable TwoWayBindWeak<TSource1, TSource2, TValue>(
                                        this TSource1 source1, Expression<Func<TSource1, TValue>> propertyExpression1,
                                        TSource2 source2, Expression<Func<TSource2, TValue>> propertyExpression2,
                                        bool immediatelySetValue = true)
            where TSource1 : INotifyPropertyChanged
            where TSource2 : INotifyPropertyChanged
        {
            string name1 = GetMemberInfo(propertyExpression1).Name;
            string name2 = GetMemberInfo(propertyExpression2).Name;

            // TODO: it would be possible to detect and stop infinitive ping-pong-setting of the same
            //       value. never needed it though, since PropertyChanged.Fody handles this pretty good.
            return new TwoWayBinding(new OneWayBinding<TSource1, TSource2, TValue>(source1, name1, source2, name2, true, immediatelySetValue),
                                     new OneWayBinding<TSource2, TSource1, TValue>(source2, name2, source1, name1, true, false));
        }
        /// <summary>
        /// can bind to private setters.
        /// <para/>
        /// if immediatelySetValue is true, the the second value will be set to the first.
        /// </summary>>
        public static IDisposable BindTo<TSource, TTarget, TValue>(this TSource source, Expression<Func<TSource, TValue>> sourceExpression,
                                                                   TTarget target, Expression<Func<TTarget, TValue>> targetExpression,
                                                                   bool immediatelySetValue = true)
            where TSource : INotifyPropertyChanged
            where TTarget : class
        {
            string sourceName = GetMemberInfo(sourceExpression).Name;
            string destName = GetMemberInfo(targetExpression).Name;

            return new OneWayBinding<TSource, TTarget, TValue>(source, sourceName, target, destName, false, immediatelySetValue);
        }

        /// <summary>
        /// can bind to private setters.
        /// <para/>
        /// if immediatelySetValue is true, the the second value will be set to the first.
        /// </summary>>
        [Pure]
        public static IDisposable BindToWeak<TSource, TTarget, TValue>(this TSource source, Expression<Func<TSource, TValue>> sourceExpression,
                                                                       TTarget target, Expression<Func<TTarget, TValue>> targetExpression,
                                                                       bool immediatelySetValue = true)
            where TSource : INotifyPropertyChanged
            where TTarget : class
        {
            string sourceName = GetMemberInfo(sourceExpression).Name;
            string destName = GetMemberInfo(targetExpression).Name;

            return new OneWayBinding<TSource, TTarget, TValue>(source, sourceName, target, destName, true, immediatelySetValue);
        }

        /// <summary>
        /// 
        /// Sets up a weak subscription, i.e. source does not get to hold a strong reference
        ///  on the action.<para/>
        /// <remarks>
        /// You got to hold a reference in your calling class to the returned IDisposable,
        /// to bind its lifetime to the calling classes lifetime.<para/>
        /// </remarks>
        /// </summary>
        [Pure]
        public static IDisposable SubscribeWeak<TSource, TValue>(this TSource source, Expression<Func<TSource, TValue>> propertyExpression, Action action)
            where TSource : INotifyPropertyChanged
        {
            string propertyName = GetMemberInfo(propertyExpression).Name;
            return new WeakSubscription<TSource>(source, propertyName, action);
        }

        /// <summary>
        /// 
        /// Sets up a weak subscription, i.e. source does not get to hold a strong reference
        ///  on the action.<para/>
        /// <remarks>
        /// You got to hold a reference in your calling class to the returned IDisposable,
        /// to bind its lifetime to the calling classes lifetime.<para/>
        /// </remarks>
        /// </summary>
        [Pure]
        public static IDisposable SubscribeWeak<TSource, TValue>(this TSource source, Expression<Func<TSource, TValue>> propertyExpression, Action<TSource> action)
                                                                 where TSource : INotifyPropertyChanged
        {
            string propertyName = GetMemberInfo(propertyExpression).Name;
            return new WeakSubscription<TSource>(source, propertyName, action);
        }

        public static IDisposable Subscribe<TSource, TValue>(this TSource source, Expression<Func<TSource, TValue>> propertyExpression, Action<TSource> action)
                                                             where TSource : INotifyPropertyChanged
        {
            string propertyName = GetMemberInfo(propertyExpression).Name;
            return new Subscription(source, propertyName, () => action(source));
        }

        public static IDisposable Subscribe<TSource, TValue>(this TSource source, Expression<Func<TSource, TValue>> propertyExpression, Action action)
                                                             where TSource : INotifyPropertyChanged
        {
            string propertyName = GetMemberInfo(propertyExpression).Name;
            return new Subscription(source, propertyName, action);
        }


        /// <summary>
        /// Converts an expression into a <see cref="T:System.Reflection.MemberInfo"/>.
        /// 
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <returns>
        /// The member info.
        /// </returns>
        private static MemberInfo GetMemberInfo(Expression expression)
        {
            LambdaExpression lambdaExpression = (LambdaExpression)expression;
            return (!(lambdaExpression.Body is UnaryExpression) ? (MemberExpression)lambdaExpression.Body : (MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand).Member;
        }

    }
}
