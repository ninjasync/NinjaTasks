using System;
using System.ComponentModel;
using Android.Content;
using AndroidX.Preference;
using MvvmCross.Base;
using NinjaTools.GUI.MVVM.Services;
using NinjaTools.Logging;

namespace NinjaTools.Droid.Services
{
    /// <summary>
    /// connect a shared preference with an INotifyPropertyChanged
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AndroidConfigurationService<T> : NpcConfigurationServiceBase<T>, IDisposable
                                                  where T: INotifyPropertyChanged
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly Listener _listener;
        private readonly ISharedPreferences _pref;

        private ISharedPreferencesEditor _edit;
        private readonly IMvxJsonConverter _json;

        private Guard _guard = new Guard();


        public AndroidConfigurationService(Context ctx, IMvxJsonConverter json)
        {
            _json = json;
            _pref = PreferenceManager.GetDefaultSharedPreferences(ctx);

            bool reqiresSave = base.LoadProperties(true);
            
            if(reqiresSave)
                Save();

            _listener = new Listener(this);
            
        }

        public override void SetConfigValue(string name, Type type, object val)
        {
            if (_edit == null)
                _edit = _pref.Edit();

            if (type == typeof (string))
                _edit.PutString(name, (string) val);
            else if (type == typeof (bool))
                _edit.PutBoolean(name, (bool) val);
            else if (type == typeof (int))
                _edit.PutInt(name, (int) val);
            else if (type == typeof (long))
                _edit.PutLong(name, (long) val);
            else if (type == typeof (float))
                _edit.PutFloat(name, (float) val);
                //else if (prop.PropertyType.IsAssignableFrom(typeof(ICollection<string>)))
                //     _edit.PutStringSet(name, (float)val);
            else
            {
                string sval = _json.SerializeObject(val);
                _edit.PutString(name, sval);
            }
        }


        public override bool GetConfigValue(string name, Type type, object defaultValue, out object value)
        {
            value = null;
            try
            {
                if (!_pref.Contains(name))
                    return false;

                if (type == typeof (string))
                    value = _pref.GetString(name, null) ?? defaultValue;
                else if (type == typeof (bool))
                    value = _pref.GetBoolean(name, (bool) defaultValue);
                else if (type == typeof (int))
                    value = _pref.GetInt(name, (int)defaultValue);
                else if (type == typeof (long))
                    value = _pref.GetLong(name, (long)defaultValue);
                else if (type == typeof (float))
                    value = _pref.GetFloat(name, (float)defaultValue);
                    //else if (prop.PropertyType.IsAssignableFrom(typeof(ICollection<string>)))
                    //    prop.SetValue(_cfg, _pref.GetStringSet(name, null));
                else
                {
                    string val = _pref.GetString(name, null);
                    value = val == null ? defaultValue : _json.DeserializeObject(type, val);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("unable to get config value", ex.Message);
                return false;
            }
            return true;
        }

        private class Listener : Java.Lang.Object, ISharedPreferencesOnSharedPreferenceChangeListener
        {
            private readonly HiddenReference<AndroidConfigurationService<T>> _parent = new HiddenReference<AndroidConfigurationService<T>>();

            public Listener(AndroidConfigurationService<T> parent)
            {
                _parent.Value  = parent;
                _parent.Value._pref.RegisterOnSharedPreferenceChangeListener(this);
            }

            public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
            {
                if (_parent.Value._guard.InUse) return;
                _parent.Value.PreferenceToProperty(key);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    _parent.Value._pref.UnregisterOnSharedPreferenceChangeListener(this);

                base.Dispose(disposing);
            }
        }

        public sealed override void Save()
        {
            if (_edit != null)
                using (_guard.Use())
                    _edit.Commit();
            _edit = null;
        }

        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}