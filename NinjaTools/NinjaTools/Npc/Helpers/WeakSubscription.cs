using System;
using System.ComponentModel;
using System.Diagnostics;
using NinjaTools.WeakEvents;

namespace NinjaTools.Npc.Helpers
{
    internal class WeakSubscription : IDisposable 
    {
        private readonly string _propertyName;
        private Action _action;
        private readonly WeakEventHandler _unsubscribe;

        public WeakSubscription(INotifyPropertyChanged source, string propertyName, Action action)
        {
            _propertyName = propertyName;
            _action = action;

            _unsubscribe = PropertyChangedWeakEventHandler.Register(source, this, Handler);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_propertyName)
             && !string.IsNullOrEmpty(e.PropertyName) && e.PropertyName != _propertyName)
                return;

            var a = _action;
            if (a != null)
                a();
        }

        private static void Handler(WeakSubscription me, object sender, PropertyChangedEventArgs args)
        {
            me.OnPropertyChanged(sender, args);
        }

        public void Dispose()
        {
            _unsubscribe.Dispose();
            _action = null;
        }
    }

    internal class WeakSubscription<TSource> : IDisposable where TSource:INotifyPropertyChanged
    {
        //private readonly INotifyPropertyChanged _source;
        private readonly string _propertyName;
        private Action _action;
        private Action<TSource> _actionSource;
        private readonly WeakEventHandler _unsubscribe;

        public WeakSubscription(TSource source, string propertyName, Action action)
        {
            //_source = source;
            _propertyName = propertyName;
            _action = action;

            _unsubscribe = PropertyChangedWeakEventHandler.Register(source, this, Handler);
        }


        public WeakSubscription(TSource source, string propertyName, Action<TSource> action)
        {
            //_source = source;
            _propertyName = propertyName;
            _actionSource = action;

            _unsubscribe = PropertyChangedWeakEventHandler.Register(source, this, Handler);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_propertyName) 
             && !string.IsNullOrEmpty(e.PropertyName) && e.PropertyName != _propertyName)
                return;

            var a = _action;
            var aS = _actionSource;
            if (a != null)
                a();
            else if(aS != null)
                aS((TSource) sender);
        }

        private static void Handler(WeakSubscription<TSource> me, object sender, PropertyChangedEventArgs args)
        {
            me.OnPropertyChanged(sender, args);
        }

        public void Dispose()
        {
            _unsubscribe.Dispose();
            _action = null;
            _actionSource = null;
        }
    }
}