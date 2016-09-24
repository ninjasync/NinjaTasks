using System;
using System.Collections.Generic;

namespace NinjaTools.MVVM.Services
{
    public class MemoryConfigurationService : IConfigurationService
    {
        private Dictionary<string, object> _cfg = new Dictionary<string, object>();

        public void SetConfigValue(string name, Type type, object value)
        {
            _cfg[name] = value;
        }

        public bool GetConfigValue(string name, Type type, object defaultVal, out object value)
        {
            if (!_cfg.TryGetValue(name, out value))
            {
                value = defaultVal;
                return false;
            }
            return true;
        }

        public void Save()
        {
        }
    }
}
