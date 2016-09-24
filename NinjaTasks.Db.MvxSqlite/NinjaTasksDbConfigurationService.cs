using System;
using System.ComponentModel;
using System.Linq;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using NinjaSync.Storage.MvxSqlite;
using NinjaTasks.Model;
using NinjaTools.Logging;

namespace NinjaTasks.Db.MvxSqlite
{
    public class NinjaTasksDbConfiguration : NinjaTasksConfiguration
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }

    public class NinjaTasksDbConfigurationService : INinjaTasksConfigurationService, IDisposable
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private NinjaTasksDbConfiguration _config;
        private readonly ISQLiteConnection _connection;

        public NinjaTasksConfiguration Cfg { get { return GetConfig(); } }

        public bool Autosave { get; set; }

        public NinjaTasksDbConfigurationService(SQLiteFactory sqlite)
        {
            _connection = sqlite.Get("cfg");
            _connection.EnsureTableCreated<NinjaTasksDbConfiguration>();
            Autosave = true;
        }

        public NinjaTasksDbConfiguration GetConfig()
        {
            if (_config != null) return _config;
            _config = _connection.NxTable<NinjaTasksDbConfiguration>()
                                 .OrderBy("Id")
                                 .FirstOrDefault();

            if (_config == null)
                _config = new NinjaTasksDbConfiguration();

            _config.PropertyChanged += OnPropertyChanged;

            return _config;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Autosave) return;
            try
            {
                Save();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public void Save()
        {
            if (_config.Id == 0) _connection.Insert(_config);
            else _connection.Update(_config);
        }


        public void Dispose()
        {
            _connection.Dispose();
        }

        public bool GetConfigValue(string name, Type type, object defaultVal, out object value)
        {
            // TODO: implement
            value = defaultVal;
            return false;
        }

        public void SetConfigValue(string name, Type type, object value)
        {
            // TODO: implement
        }
    }
}