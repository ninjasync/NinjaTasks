using System;
using System.ComponentModel;

namespace NinjaTools.Npc.Helpers
{
    internal class Subscription : IDisposable
    {
        private readonly INotifyPropertyChanged _source;
        private readonly string _propertyName;
        private Action _action;

        public Subscription(INotifyPropertyChanged source, string propertyName, Action action)
        {
            _source = source;
            _propertyName = propertyName;
            _action = action;

            source.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_propertyName)
             && !string.IsNullOrEmpty(e.PropertyName) && e.PropertyName != _propertyName)
                return;

            var a = _action;
            if (a != null) a();
        }

        public void Dispose()
        {
            _source.PropertyChanged -= OnPropertyChanged;
            _action = null;
        }
    }
}