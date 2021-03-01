using System;
using System.Linq;
using System.Text;
using NinjaTools.Sqlite;

namespace NinjaTasks.Db.MvxSqlite
{
    public class SQLiteFactory : IDisposable
    {
        private readonly ISQLiteConnectionFactoryEx _factory;
        private ISQLiteConnection _connection;
        private readonly string   _filename;
        private readonly bool     _useEncryption;


        public SQLiteFactory(ISQLiteConnectionFactoryEx factory, string filename, bool useEncryption = false)
        {
            _factory       = factory;
            _filename      = filename;
            _useEncryption = useEncryption;
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

            if (_useEncryption)
            {
                // primitive obfuscation.
                string command = Encoding.UTF8.GetString(Convert.FromBase64String("UFJBR01BIEtFWT0nezB9Jzs" + "=")); // PRAGMA KEY='{0}';
                command = string.Format(command, GetType().FullName.Substring(1, 36));
                int success = _connection.Query<int>(command).Single();
                if (success != 0)
                    throw new Exception(); // unable to enable encryption (?)
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
            return new SQLiteFactory(_factory, _filename, _useEncryption);
        }
    }
}
