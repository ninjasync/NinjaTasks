using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using NinjaTools.Npc.Reflection;

namespace NinjaTools.Npc.Helpers
{
    internal class OneWayBinding<TSource, TTarget, TValue> : IDisposable where TSource:INotifyPropertyChanged
    {
        //private readonly TSource _source;
        //private readonly object _target;
        //private readonly string _sourceName;
        private Func<TValue> _getSourceValue;
        private Action<TValue> _setTargetValue;
        private readonly IDisposable _subscription;

        public OneWayBinding(TSource source, string sourceName, object target, string targetName, bool makeWeakBinding, bool immediatelySetValue)
        {
            // retrieve source property getter
            var sourceType = typeof(TSource); 
            var prop = sourceType.GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance)
                                 .First(p => p.Name == sourceName && p.PropertyType == typeof(TValue));
            var unboundGetter = PropertyInfoCache.GetGetter<TSource>(prop);
            _getSourceValue = () => (TValue)unboundGetter(source);

            // retrieve destination property setter.               
            var targetType = typeof(TTarget);
            prop = targetType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                             .First(p => p.Name == targetName && p.PropertyType == typeof(TValue));
            var unboundSetter = PropertyInfoCache.GetObjectSetter(prop);
            _setTargetValue = val => unboundSetter(target, val);


            if (makeWeakBinding)
                _subscription = new WeakSubscription<TSource>(source, sourceName, OnSourceChanged);
            else
                _subscription = new Subscription(source, sourceName, OnSourceChanged);

            if (immediatelySetValue)
                OnSourceChanged();
        }

        private void OnSourceChanged()
        {
            TValue val = _getSourceValue();
            _setTargetValue(val);
        }

        public void Dispose()
        {
            _subscription.Dispose();
            _getSourceValue = null;
            _setTargetValue = null;
        }

    }
}