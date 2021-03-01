using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NinjaTools.GUI.MVVM.Services;

namespace NinjaTools.GUI.Wpf.Services
{
    public class JsonConfigurationService<T> : IConfigurationService<T> , IDisposable where T: new()
    {
        public T Cfg { get; } 

        private readonly Dictionary<string, JValue> _additionalValues = new Dictionary<string, JValue>();

        public JsonConfigurationService()
        {
            try
            {
                Cfg = JsonConvert.DeserializeObject<T>(File.ReadAllText(SettingsPath));
            }
            catch 
            {
                Cfg = new T();
            }

        }

        public string SettingsPath
        {
            get
            {
                const string publisher = "Ninja";
                string appname = Assembly.GetEntryAssembly()?.GetName().Name ?? "MyFirstProgram";
                string       dir;

                if (Directory.Exists("portable-data"))
                    dir = "portable-data";
                else
                {
                    dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), publisher,
                                       appname);
                    Directory.CreateDirectory(dir);
                }

                return Path.GetFullPath(Path.Combine(dir, appname + ".config"));

            }
        }

        public void SetConfigValue(string name, Type type, object value)
        {
        }

        public bool GetConfigValue(string name, Type type, object defaultVal, out object value)
        {
            value = defaultVal;
            return false;
        }

        public void Save()
        {
            File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(Cfg, Formatting.Indented));
        }

        public void Dispose()
        {
            Save();
        }
    }
}
