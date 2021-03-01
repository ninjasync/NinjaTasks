using System;
using System.ComponentModel;
using JetBrains.Annotations;
using NinjaTools.Npc.Helpers;

namespace NinjaTools.Npc
{
    /// <summary>
    /// this part contains the string-based methods.
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
        public static IDisposable TwoWayBind<TSource1, TSource2>(
                                        this TSource1 source1, string property1,
                                             TSource2 source2, string property2=null,
                                             bool immediatelySetValue = true)
            where TSource1 : INotifyPropertyChanged
            where TSource2 : INotifyPropertyChanged
        {
            // Note: it would be possible to detect and stop infinitive ping-pong-setting of the same
            //       value. never needed it though, since PropertyChanged.Fody handles this pretty good.
            var binding1 = new OneWayBindingWithoutTValue<TSource1,TSource2>(source1, property1, source2, property2 ?? property1, false, immediatelySetValue);
            var binding2 = new OneWayBindingWithoutTValue<TSource2,TSource1>(source2, property2 ?? property1, source1, property1, false, false);
            return new TwoWayBinding(binding1,binding2);
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
        public static IDisposable TwoWayBindWeak<TSource1, TSource2>(
                                        this TSource1 source1, string property1,
                                        TSource2 source2, string property2=null,
                                        bool immediatelySetValue = true)
            where TSource1 : INotifyPropertyChanged
            where TSource2 : INotifyPropertyChanged
        {
            // Note: it would be possible to detect and stop infinitive ping-pong-setting of the same
            //       value. never needed it though, since PropertyChanged.Fody handles this pretty good.
            return new TwoWayBinding(new OneWayBindingWithoutTValue<TSource1,TSource2>(source1, property1, source2, property2 ?? property1, true, immediatelySetValue),
                                     new OneWayBindingWithoutTValue<TSource2,TSource1>(source2, property2 ?? property1, source1, property1, true, false));
        }
        /// <summary>
        /// can bind to private setters.
        /// <para/>
        /// if immediatelySetValue is true, the the second value will be set to the first.
        /// </summary>>
        public static IDisposable BindTo<TSource, TTarget>(this TSource source, string sourceProperty,
                                                                TTarget target, string targetProperty = null,
                                                                bool immediatelySetValue = true)
            where TSource : INotifyPropertyChanged
            where TTarget : class
        {
            return new OneWayBindingWithoutTValue<TSource,TTarget>(source, sourceProperty, target, targetProperty ?? sourceProperty, false, immediatelySetValue);
        }

        /// <summary>
        /// can bind to private setters.
        /// <para/>
        /// if immediatelySetValue is true, the the second value will be set to the first.
        /// </summary>>
        [Pure]
        public static IDisposable BindToWeak<TSource, TTarget>(this TSource source, string sourceProperty,
                                                                    TTarget target, string targetProperty=null,
                                                                    bool immediatelySetValue = true)
            where TSource : INotifyPropertyChanged
            where TTarget : class
        {
            return new OneWayBindingWithoutTValue<TSource, TTarget>(source, sourceProperty, target, targetProperty ?? sourceProperty, true, immediatelySetValue);
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
        public static IDisposable SubscribeWeak<TSource>(this TSource source, string propertyName, Action action)
            where TSource : INotifyPropertyChanged
        {
            return new WeakSubscription(source, propertyName, action);
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
        public static IDisposable SubscribeWeak<TSource>(this TSource source, string propertyName, Action<TSource> action)
                                                                 where TSource : INotifyPropertyChanged
        {
            return new WeakSubscription<TSource>(source, propertyName, action);
        }

        public static IDisposable Subscribe<TSource>(this TSource source, string propertyName, Action<TSource> action)
                                                             where TSource : INotifyPropertyChanged
        {
            return new Subscription(source, propertyName, () => action(source));
        }

        public static IDisposable Subscribe<TSource>(this TSource source, string propertyName, Action action)
                                                             where TSource : INotifyPropertyChanged
        {
            return new Subscription(source, propertyName, action);
        }
    }
}
