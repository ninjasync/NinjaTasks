using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using NinjaSync.Model;
using NinjaSync.Model.Journal;

namespace NinjaSync.Storage.MvxSqlite
{
    public class MvxSqliteStringMappingStorage : MvxSqliteMappingStorage<StringIdMap, string>,
                                                 IStringMappingStorage
    {
        public MvxSqliteStringMappingStorage(ISQLiteConnection con, string tablePrefix)
            : base(con, tablePrefix)
        {
        }
    }

    public abstract class MvxSqliteMappingStorage<TMapType, TIdType> : IMappingStorage<TIdType>
                                        where TMapType : IdMap<TIdType>, new()
    {
        private ISQLiteConnection Connection { get; set; }
        private readonly SqliteExpressionBuilder _query;

        public MvxSqliteMappingStorage(ISQLiteConnection con, string tablePrefix)
        {
            Connection = con;
            var tableName = tablePrefix + Connection.GetMapping<TMapType>().TableName;

            Connection.EnsureTableCreated<TMapType>(tableName);
            _query = new SqliteExpressionBuilder(tableName);
        }


        public IdMap<TIdType> GetMappingFromLocal(TrackableType objectType, string localId)
        {
            return (IdMap<TIdType>)(object)// fix for Dot42 bug
                    Connection.Query<TMapType>(_query.Select().Where("ObjectType=? AND LocalId=?"), objectType, localId)
                              .FirstOrDefault();
        }

        public IdMap<TIdType> GetMappingFromRemote(TrackableType objectType, TIdType remoteid)
        {
            return (IdMap<TIdType>)(object) // fix for Dot42 bug
                   Connection.Query<TMapType>(_query.Select().Where("ObjectType=? AND RemoteId=?"), objectType, remoteid)
                             .FirstOrDefault();
            //.FirstOrDefault(p => p.ObjectType == objectType && p.RemoteCommitId == remoteid);
        }

        public IEnumerable<IdMap<TIdType>> GetMappings()
        {
            return Connection.Query<TMapType>(_query.Select());
        }

        public virtual void SaveMapping(IdMap<TIdType> mapping)
        {
            Debug.Assert(mapping.LocalId != null);
            Debug.Assert(!Equals(mapping.RemoteId, default(TIdType)));

            Connection.Insert(_query.TableName, mapping, "OR REPLACE");
        }

        public void SaveMapping(TrackableType objectType, string localId, TIdType remoteId)
        {
            var map = new TMapType { ObjectType = objectType, LocalId = localId, RemoteId = remoteId };
            SaveMapping(map);
        }
    }
}
