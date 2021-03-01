using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NinjaTools.GUI.MVVM.Services
{
    public class MemoryConfigurationSettingService<T> : NpcConfigurationServiceBase<T>, IDisposable
                                                     where T: INotifyPropertyChanged
    {
        private readonly  MemoryConfigurationService _mem = new MemoryConfigurationService();

        public MemoryConfigurationSettingService()
        {
        }
        public override void SetConfigValue(string name, Type type, object val)
        {
            _mem.SetConfigValue(name, type, val);
        }
        
        public override bool GetConfigValue(string name, Type type, object defaultVal, out object value)
        {
            return _mem.GetConfigValue(name, type, defaultVal, out value);
        }

        public override void Save()
        {
            _mem.Save();
        }

        public void Dispose()
        {
        }
    }
}
