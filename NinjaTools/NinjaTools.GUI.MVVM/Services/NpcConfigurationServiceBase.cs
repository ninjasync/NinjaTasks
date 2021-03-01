using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace NinjaTools.GUI.MVVM.Services
{
    public abstract class NpcConfigurationServiceBase<T> : IConfigurationService<T> where T : INotifyPropertyChanged
    {
        private readonly Dictionary<string, PropertyInfo> _properties = new Dictionary<string, PropertyInfo>();
        private readonly HashSet<string> _settingprop = new HashSet<string>();

        public T Cfg { get; private set;}

        

        public NpcConfigurationServiceBase()
        {
            Cfg = Activator.CreateInstance<T>();
            
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                _properties.Add(prop.Name, prop);
            }

            Cfg.PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        /// loads all properties.
        /// </summary>
        /// <returns>true, if some properties where stored.</returns>
        protected bool LoadProperties(bool storeIfCouldNotLoad=false)
        {
            bool reqiresSave = false;
            foreach (var prop in _properties.Values)
                if (!PreferenceToProperty(prop.Name) && storeIfCouldNotLoad)
                {
                    reqiresSave = true;
                    PropertyToPreference(prop.Name);
                }
            return reqiresSave;
        }

        T IConfigurationService<T>.Cfg
        {
            get { return Cfg; }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool isSetting = false;
            
            lock (_settingprop)
                isSetting = _settingprop.Contains(e.PropertyName);
            
            if(!isSetting)
                PropertyToPreference(e.PropertyName);
        }

        protected void PropertyToPreference(string name)
        {
            PropertyInfo prop;
            if (!_properties.TryGetValue(name, out prop))
                return;

            var val = prop.GetValue(Cfg, null);
            Type type = prop.PropertyType;

            SetConfigValue(name, type, val);
        }

        protected bool PreferenceToProperty(string name)
        {
            PropertyInfo prop;

            if (!_properties.TryGetValue(name, out prop))
                return false;

            Type type = prop.PropertyType;
            var defaultVal = prop.GetValue(Cfg);

            object value;
            if (!GetConfigValue(name, type, defaultVal, out value))
                return false;

            lock (_settingprop)
                _settingprop.Add(name);

            try
            {
                prop.SetValue(Cfg, value, null);
            }
            finally
            {
                lock (_settingprop)
                    _settingprop.Remove(name);
            }
            
            return true;
        }


        public abstract void SetConfigValue(string name, Type type, object val);
        public abstract bool GetConfigValue(string name, Type type, object defaultVal, out object value);

        public abstract void Save();
    }
}