using NinjaTools.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

#if DOT42
using System.Collections.Concurrent;
#endif

namespace NinjaSync.Storage.MvxSqlite
{
    public static class SQLiteHelpers
    {
        public static string MakeIdWhereClause(IEnumerable<string> ids, string columnName = "Id", SelectionMode mode = SelectionMode.SelectSpecified, bool includeWhere = true)
        {
            string idList = string.Join("','", ids.Select(i => i.Replace("'", "''")));
            if (idList.Length == 0)
            {
                return (includeWhere ? " WHERE " : "")
                       + (mode == SelectionMode.SelectSpecified ? " 0 " : " 1 ");
            }

            string optNot = mode == SelectionMode.SelectSpecified ? " " : " NOT ";
            return String.Format(" {0} {1} {2} IN ('{3}') ", includeWhere ? "WHERE" : "", columnName, optNot, idList);
        }


        //private static string MakeIdWhereClause(IEnumerable<int> ids, string columnName="Id", SelectionMode mode = SelectionMode.SelectSpecified)
        //{
        //    string idList = string.Join(",", ids.Select(i => i.ToString(CultureInfo.InvariantCulture)));
        //    string optNot = mode == SelectionMode.SelectSpecified ? " " : " NOT ";
        //    return string.Format(" {0} {1} IN ({2}) ", columnName, optNot, idList);
        //}
#if DOT42
        private static readonly ConcurrentDictionary<Tuple<Type, string, string>, int> CreatedTables = new ConcurrentDictionary<Tuple<Type, string, string>, int>();
#else
        private static readonly Dictionary<Tuple<Type, string, string>, int> CreatedTables = new Dictionary<Tuple<Type, string, string>, int>();
#endif
        /// <summary>
        /// Ensures that each table is only created once in each database 
        /// file during program execution. This ensures that all tables are 
        /// up to date, but limits locking problems.
        /// </summary>
        public static bool EnsureTableCreated<T>(this ISQLiteConnection con, string overwriteTableName = null)
        {
            return EnsureTableCreated(con, typeof(T), overwriteTableName);
        }

        public static bool EnsureTableCreated(this ISQLiteConnection con, Type type, string overwriteTableName = null)
        {
            var mapping = con.GetMapping(type);
            var tableName = overwriteTableName ?? mapping.TableName;
            
            bool exists = con.ExecuteScalar<int>("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=?;",
                                                 tableName) > 0;

            var key = Tuple.Create(type, con.DatabasePath, tableName);
#if !DOT42
            lock (CreatedTables)
#endif
            {
                if (exists && CreatedTables.ContainsKey(key))
                    return false;

                lock (CreatedTables)
                {
                    con.CreateTable(type, overwriteTableName);
#if DOT42
                    CreatedTables.TryAdd(key, 0);
#else
                    CreatedTables[key] = 0;
#endif
                }
            }
            return true;
        }
    }
}