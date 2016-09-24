using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using NinjaTools.Npc.Reflection;

namespace NinjaTools.Npc.Helpers
{
    internal class OneWayBindingWithoutTValue<TSource, TTarget> : IDisposable where TSource : INotifyPropertyChanged
    {
        private Func<object> _getSourceValue;
        private Action<object> _setTargetValue;
        private readonly IDisposable _subscription;

        public OneWayBindingWithoutTValue(TSource source, string sourceName, TTarget target, string targetName,
                                          bool makeWeakBinding, bool immediatelySetValue)
        {
            // retrieve source property getter
            var prop = PropertyInfoCache.GetInstanceProperty(typeof(TSource), sourceName);
            var unboundGetter = PropertyInfoCache.GetGetter<TSource>(prop);
            _getSourceValue = () => unboundGetter(source);

            // retrieve destination property setter.     
            prop = PropertyInfoCache.GetInstanceProperty(typeof(TTarget), targetName);
            var unboundSetter = PropertyInfoCache.GetObjectSetter(prop);
            _setTargetValue = val => unboundSetter(target, val);

            if (makeWeakBinding)
                _subscription = new WeakSubscription<TSource>(source, sourceName, OnSourceChanged);
            else
                _subscription = new Subscription(source, sourceName, OnSourceChanged);

            if (immediatelySetValue)
                OnSourceChanged();
        }

        private static Type GetType(object obj)
        {
#if DOT42
            return obj.GetTypeReflectionSafe();
#else
            return obj.GetType();
#endif
        }

        private void OnSourceChanged()
        {
            object val = _getSourceValue();
            _setTargetValue(val);
        }

        public void Dispose()
        {
            _subscription.Dispose();
            //_source = default(TSource);
            //_target = null;
            _getSourceValue = null;
            _setTargetValue = null;
        }

    }
}