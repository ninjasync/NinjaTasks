using System;
using System.IO;
using NinjaTools.Sqlite;
using NinjaTasks.Db.MvxSqlite;
using NinjaTasks.Model.Sync;
using NinjaTools.Logging;

namespace NinjaTasks.Tests
{
    static class Helpers
    {
        public static readonly TaskWarriorAccount Account = new TaskWarriorAccount
        {
            Id = 1,
            ClientCertificateAndKeyPfxFile = @"n:\xdata\certificates\PublicOlaf-taskd.pfx",
            Org = "Public",
            User = "Olaf",
            Key = "245d91a2-37f9-41b4-b7c0-cfcb6080101e",
            ServerHostname = "knutwg",
            ServerPort = 8020
        };

        public static ISQLiteConnectionFactoryEx CreateConnectionFactory()
        {
#if !DOT42
            NinjaTools2NLog.Register();

            //string sqlitebin = Path.GetFullPath(Environment.Is64BitProcess ? "x64" : "x86");
            //Environment.SetEnvironmentVariable("PATH", sqlitebin + ";" + Environment.GetEnvironmentVariable("PATH"));
            return new NinjaTools.Sqlite.SqliteNetPCL.SqLiteNetPCLConnectionFactory();
#else
            NinjaTools.Droid.NinjaToolsLoggerToLogCat.Register();
            return new Cirrious.NinjaTools.Sqlite.Dot42.MvxDot42SQLiteConnectionFactory();
#endif
        }

        public static void ClearDatabase(SQLiteFactory fac)
        {
            var con = fac.Get("test");
            string dropCmd = con.ExecuteScalar<string>("select group_concat( 'drop table ' || name, ';') " +
                                                       "from sqlite_master where type = 'table'" +
                                                       "and name <> 'sqlite_sequence' " +
                                                       "group by type");
            if (!string.IsNullOrEmpty(dropCmd))
                foreach (var s in dropCmd.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    con.Execute(s);
        }

    }
}
