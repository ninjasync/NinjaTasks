using System;
using System.ComponentModel;
using Android.Content;
using Android.Preferences;
using Cirrious.CrossCore.Platform;
using NinjaTools.MVVM.Services;

namespace NinjaTools.Droid.Services
{
    /// <summary>
    /// Connects a shared preference with an INotifyPropertyChanged
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AndroidConfigurationService<T> : NpcConfigurationServiceBase<T>, IDisposable,
                                                  ISharedPreferences_IOnSharedPreferenceChangeListener
                                                  where T: INotifyPropertyChanged
    {
        private readonly IMvxTrace _trace;
        
        private readonly ISharedPreferences _pref;

        private ISharedPreferences_IEditor _edit;
        private readonly IMvxJsonConverter _json;

        private readonly Guard _guard = new Guard();

        public AndroidConfigurationService(Context ctx, IMvxJsonConverter json, IMvxTrace trace)
        {
            _json = json;
            _trace = trace;
            _pref = PreferenceManager.GetDefaultSharedPreferences(ctx);

            bool reqiresSave = base.LoadProperties(true);
            
            if(reqiresSave)
                Save();

            _pref.RegisterOnSharedPreferenceChangeListener(this);
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


        public override bool GetConfigValue(string name, Type type, object defaultVal, out object value)
        {
            value = null;
            try
            {
                if (!_pref.Contains(name))
                    return false;

                if (type == typeof (string))
                    value = _pref.GetString(name, null) ?? defaultVal;
                else if (type == typeof (bool))
                    value = _pref.GetBoolean(name, (bool)defaultVal);
                else if (type == typeof (int))
                    value = _pref.GetInt(name, (int)defaultVal);
                else if (type == typeof (long))
                    value = _pref.GetLong(name, (long)defaultVal);
                else if (type == typeof (float))
                    value = _pref.GetFloat(name, (float)defaultVal);
                //else if (prop.PropertyType.IsAssignableFrom(typeof(ICollection<string>)))
                    //    prop.SetValue(_cfg, _pref.GetStringSet(name, null));
                else
                {
                    string val = _pref.GetString(name, null);
                    value = val == null ? defaultVal : _json.DeserializeObject(type, val);
                }
            }
            catch (Exception ex)
            {
                _trace.Trace(MvxTraceLevel.Warning, "pref", ex.Message);
                return false;
            }
            return true;
        }

        public sealed override void Save()
        {
            if (_edit != null)
                using (_guard.Use())
                    _edit.Commit();
            _edit = null;
        }

        void ISharedPreferences_IOnSharedPreferenceChangeListener.OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (_guard.InUse) return;
            PreferenceToProperty(key);
        }

        public void Dispose()
        {
            _pref.UnregisterOnSharedPreferenceChangeListener(this);
        }
    }
}