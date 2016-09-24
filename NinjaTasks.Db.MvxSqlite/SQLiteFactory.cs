using System;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;

namespace NinjaTasks.Db.MvxSqlite
{
    public class SQLiteFactory : IDisposable
    {
        private readonly ISQLiteConnectionFactoryEx _factory;
        private readonly string _filename;
        private ISQLiteConnection _connection;


        public SQLiteFactory(ISQLiteConnectionFactoryEx factory, string filename = "ninjatasks.sqlite")
        {
            _factory = factory;
            _filename = filename;
        }

        public ISQLiteConnection Get(string purpose)
        {
            if (_connection == null)
            {
                // store as string, with seconds precision.
                var opts = new SQLiteConnectionOptions { DateTimeFormat = DateTimeFormat.IsoString };

                _connection = _factory.CreateEx(_filename, opts);
                _connection.BusyTimeout = TimeSpan.FromSeconds(5);

            }
            return _connection;
        }

        public void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();
        }

        public SQLiteFactory Clone()
        {
            return new SQLiteFactory(_factory, _filename);
        }
    }
}
