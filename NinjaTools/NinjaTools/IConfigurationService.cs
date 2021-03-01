using System;

namespace NinjaTools
{
    public interface IConfigurationService
    {
        void SetConfigValue(string name, Type type, object value);
        bool GetConfigValue(string name, Type type, object defaultVal, out object value);

        void Save();
    }

    public class PrefixedConfigurationService : IConfigurationService
    {
        private readonly string _name;
        private readonly IConfigurationService _cfg;

        public PrefixedConfigurationService(IConfigurationService cfg, string name)
        {
            _cfg = cfg;
            _name = name + ".";
        }

        public PrefixedConfigurationService(IConfigurationService cfg, Type type) : this(cfg, type.Name)
        {
        }

        public void SetConfigValue(string name, Type type, object value)
        {
            _cfg.SetConfigValue(_name + name, type, value);
        }

        public bool GetConfigValue(string name, Type type, object defaultVal, out object value)
        {
            return _cfg.GetConfigValue(_name + name, type, defaultVal, out value);
        }

        public void Save()
        {
            _cfg.Save();
        }
    }

    public interface IConfigurationService<out T> : IConfigurationService
    {
        T Cfg { get; }
    }

    public static class ConfigurationServiceExtension
    {
        public static void SetValue(this IConfigurationService cfg, string name, string value)
        {
            cfg.SetConfigValue(name, typeof(string), value);
        }

        public static string GetValue(this IConfigurationService cfg, string name, string @default = null)
        {
            object ret;
            cfg.GetConfigValue(name, typeof(string), @default, out ret);
            return (string)ret;
        }

        public static bool? GetBool(this IConfigurationService cfg, string name)
        {
            object ret;
            cfg.GetConfigValue(name, typeof(bool), null, out ret);
            return (bool?)ret;
        }

        public static void SetValue(this IConfigurationService cfg, string name, bool value)
        {
            cfg.SetConfigValue(name, typeof(bool), value);
        }

        public static T GetValue<T>(this IConfigurationService cfg, string name, T defaultVal)
        {
            object ret;
            cfg.GetConfigValue(name, typeof(T), defaultVal, out ret);
            return ret == null ? default(T): (T) ret;
        }

        public static void SetValue<T>(this IConfigurationService cfg, string name, T value)
        {
            cfg.SetConfigValue(name, typeof(T), value);
        }

    }
}