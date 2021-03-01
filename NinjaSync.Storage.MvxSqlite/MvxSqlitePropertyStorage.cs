using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTools.Sqlite;

namespace NinjaSync.Storage.MvxSqlite
{
    public interface IPropertyStorage
    {
        void SetProperties(string id, ICollection<Tuple<string, string>> propValues, bool onlyUpdateSpecified);
        IEnumerable<Tuple<string, string>> GetProperties(string id);

        void Clear();
    }

    public class MvxSqlitePropertyStorage : IPropertyStorage
    {
        private readonly ISQLiteConnection _con;
        private readonly string _table;

        public MvxSqlitePropertyStorage(ISQLiteConnection con, string tableName)
        {
            _con = con;
            _table = tableName;
            _con.EnsureTableCreated<AdditionalProperty>(_table);
        }

        public void SetProperties(string id, ICollection<Tuple<string, string>> propValues, bool onlyUpdateSpecified)
        {
            string cmd;
            if (!onlyUpdateSpecified)
            {

                string selectNotSpecifiedInClause = SQLiteHelpers.MakeIdWhereClause(
                                                        propValues.Select(p => p.Item1),
                                                        "Member",
                                                        includeWhere: false,
                                                        mode: SelectionMode.SelectNotSpecified);
                // set value to null for 'deleted' additional properties, to generate an update-trigger
                cmd = string.Format("UPDATE {0} SET Value=NULL WHERE Id=? AND {1}", _table, selectNotSpecifiedInClause);
                _con.Execute(cmd, id);

                // now delete from database.
                cmd = string.Format("DELETE FROM {0} WHERE Id=? AND {1}", _table, selectNotSpecifiedInClause);
                _con.Execute(cmd, id);

            }

            foreach (var prop in propValues)
            {
                // -- Try to update any existing row
                cmd = string.Format("UPDATE {0} SET Value=?1 WHERE Id=?2 AND Member=?3;", _table);
                _con.Execute(cmd, prop.Item2, id, prop.Item1);

                // -- Make sure it exists
                cmd = string.Format("INSERT OR IGNORE INTO {0} (Id,Member,Value) VALUES (?1, ?2, ?3)", _table);
                _con.Execute(cmd, id, prop.Item1, prop.Item2);
            }
        }

        public IEnumerable<Tuple<string, string>> GetProperties(string id)
        {
            return _con.Query<AdditionalProperty>(string.Format("SELECT * FROM '{0}' WHERE Id=?", _table), id)
                       .AsEnumerable()
                       .Select(p => Tuple.Create(p.Member, p.Value));
        }

        public void Clear()
        {
            _con.Execute("DELETE FROM '" + _table + "'");
        }
    }
}
