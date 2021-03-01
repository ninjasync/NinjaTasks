using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using NinjaTools.Sqlite;
using NinjaSync.Model;
using NinjaSync.Model.Journal;

namespace NinjaSync.Storage.MvxSqlite
{
    /// <summary>
    /// This class store each Trackable Type in its own table. Call AddType() to cunfigure a 
    /// type and its tables.
    /// </summary>
    public class MvxSqliteTrackableStorage : ITrackableStorage
    {
        private readonly ISQLiteConnection _con;
        protected string TablePrefix { get; private set; }
        protected ISQLiteConnection Connection { get { return _con; } }

        protected class DbType
        {
            public TrackableType Type;
            public ITableMapping Mapping;
            public Type StorageType;

            public string TableName;
            public string PropertyTableName;

            public SqliteExpressionBuilder Query;

            public IPropertyStorage PropertyStorage;
        }

        protected readonly Dictionary<TrackableType, DbType> Types = new Dictionary<TrackableType, DbType>();

        public MvxSqliteTrackableStorage(ISQLiteConnection con, string tablePrefix = null)
        {
            _con = con;
            TablePrefix = tablePrefix;
        }

        /// <summary>
        /// return true if the tables where newly created, false otherwise.
        /// </summary>
        /// <returns></returns>
        public bool AddType(TrackableType type, Type dbType, bool enableProperties = false)
        {
            if (Types.Count == 0)
                LoadOrCreateId(dbType.Name);

            Debug.Assert(typeof(ITrackable).GetTypeInfo().IsAssignableFrom(dbType.GetTypeInfo()));

            var db = new DbType { Type = type, StorageType = dbType };

            db.Mapping = _con.GetMapping(dbType);
            db.TableName = TablePrefix + db.Mapping.TableName;


            bool wasTableCreated = _con.EnsureTableCreated(dbType, db.TableName);

            if (enableProperties)
            {
                Debug.Assert(typeof(ITrackableWithAdditionalProperties).GetTypeInfo().IsAssignableFrom(dbType.GetTypeInfo()));

                db.PropertyTableName = db.TableName + "Properties";
                _con.EnsureTableCreated<AdditionalProperty>(db.PropertyTableName);

                db.PropertyStorage = new MvxSqlitePropertyStorage(_con, db.PropertyTableName);

                if (wasTableCreated)
                {
                    // create delete trigger for the properties.
                    string deleteProperties = db.TableName + "_delete_properties";
                    Connection.Execute("DROP TRIGGER IF EXISTS " + deleteProperties);
                    var cmd = string.Format(
                       @"CREATE TRIGGER {0}
                          AFTER DELETE ON  {1}
                          BEGIN
                            DELETE FROM {2} WHERE {3}=old.{3};
                          END;", deleteProperties, db.TableName, db.PropertyTableName, TrackableProperties.ColId);
                    Connection.Execute(cmd);

                }
            }

            db.Query = new SqliteExpressionBuilder(db.TableName);

            Types.Add(type, db);

            return wasTableCreated;
        }

        private void LoadOrCreateId(string name)
        {
            _con.EnsureTableCreated<StorageProperty>();
            var id = _con.Query<StorageProperty>("SELECT * FROM StorageProperty")
                         .FirstOrDefault(s => s.Id == name && s.Member == "StorageId");
            if (id == null)
            {
                id = new StorageProperty
                {
                    Id = name,
                    Member = "StorageId",
                    Value = Guid.NewGuid().ToString()
                };
                _con.Insert(id);
            }

            StorageId = id.Value;
        }


        public virtual void Save(ITrackable obj, ICollection<string> properties)
        {
            var db = Types[obj.TrackableType];

            if (obj.CreatedAt == default(DateTime))
                obj.CreatedAt = DateTime.UtcNow;

            bool createNewId = obj.IsNew;

            if (createNewId)
                obj.SetNewId();

            RunInTransaction(() =>
            {
                bool exists = false;

                if (!createNewId)
                {
                    var cmd = db.Query.Select("COUNT (*)").WhereEquals(TrackableProperties.ColId, "?");
                    exists = _con.ExecuteScalar<int>(cmd, obj.Id) > 0;
                }

                IList<string> baseProperties = properties == null ? obj.Properties : properties.Intersect(obj.Properties).ToList();
                if (obj is ITrackableWithAdditionalProperties addProp)
                    baseProperties = baseProperties.Except(addProp.AdditionalProperties).ToList();

                // store in db.
                if (exists)
                {
                    if(baseProperties.Count > 0)
                        _con.Update(db.TableName, obj, db.StorageType, baseProperties);
                }
                else
                    _con.Insert(db.TableName, obj, db.StorageType);

                // handle additional properties.
                if (db.PropertyTableName != null)
                {
                    var objProp = (ITrackableWithAdditionalProperties)obj;
                    var updateProperties = properties?.Except(baseProperties) ?? objProp.AdditionalProperties;

                    var props = updateProperties.Select(a => Tuple.Create(a, (string)obj.GetProperty(a)))
                                                .ToList();

                    db.PropertyStorage.SetProperties(obj.Id, props, properties != null);

                }
            });
        }

        public string StorageId { get; private set; }

        public ITrackable GetById(TrackableId id)
        {
            var db = Types[id.Type];
            var ret = (ITrackable)Connection.Query(db.Mapping, db.Query.Select().Where("Id=?"), id.ObjectId)
                                            .FirstOrDefault();

            if (ret != null && db.PropertyStorage != null)
                LoadProperties(db.PropertyStorage, ret);

            return ret;

        }

        public IEnumerable<ITrackable> GetById(params TrackableId[] ids)
        {
            IEnumerable<ITrackable> ret = new ITrackable[0];

            foreach (var type in ids.GroupBy(i => i.Type))
            {
                var db = Types[type.Key];

                string cmd = db.Query.Select().WhereIn(TrackableProperties.ColId, type.Select(p => p.ObjectId));

                var result = _con.Query(db.Mapping, cmd).Cast<ITrackable>();

                if (db.PropertyStorage != null)
                    result = result.Select(p => LoadProperties(db.PropertyStorage, p));

                ret = ret.Concat(result);
            }
            return ret;
        }

        public IEnumerable<TrackableId> GetIds(SelectionMode mode, TrackableType type, params string[] ids)
        {
            var db = Types[type];

            var build = db.Query.Select(TrackableProperties.ColId);
            if (mode == SelectionMode.SelectSpecified)
                build.WhereIn(TrackableProperties.ColId, ids);
            else
                build.Where().NotIn(TrackableProperties.ColId, ids);

            return _con.Query<string>(build)
                       .Select(id => new TrackableId(type, id));
        }

        public IEnumerable<ITrackable> GetTrackable(params TrackableType[] limitToSpecifiedTypes)
        {
            IEnumerable<ITrackable> ret = new ITrackable[0];

            var dbTypes = limitToSpecifiedTypes.Length == 0 ? Types : Types.Where(p => limitToSpecifiedTypes.Contains(p.Key));

            foreach (var db in dbTypes)
            {
                var result = _con.Query(db.Value.Mapping, db.Value.Query.Select())
                                 .Cast<ITrackable>();

                var propertyStorage = db.Value.PropertyStorage;
                if (propertyStorage != null)
                    result = result.Select(p => LoadProperties(propertyStorage, p));

                ret = ret.Concat(result);
            }

            return ret;

        }

        public void Delete(SelectionMode mode, params TrackableId[] id)
        {
            _con.RunInTransaction(() =>
            {
                foreach (var type in id.GroupBy(i => i.Type))
                {
                    var db = Types[type.Key];
                    string whereClause = SQLiteHelpers.MakeIdWhereClause(type.Select(p => p.ObjectId), "Id", mode);
                    _con.Execute("DELETE FROM " + db.TableName + whereClause);

                    //// additional properties.
                    //if (db.PropertyDbType != null)
                    //{
                    //    var cmdDelAdd = string.Format("DELETE FROM {0} WHERE {1}", db.PropertyTableName, whereClause);
                    //    _con.Execute(cmdDelAdd);
                    //}
                }

            });
        }

        private static ITrackable LoadProperties(IPropertyStorage db, ITrackable obj)
        {
            foreach (var prop in db.GetProperties(obj.Id))
                obj.SetProperty(prop.Item1, prop.Item2);
            return obj;
        }


        public void Clear()
        {
            foreach (var type in Types)
            {
                _con.Execute(type.Value.Query.Delete());
                if (type.Value.PropertyStorage != null)
                    type.Value.PropertyStorage.Clear();
            }
        }

        [DebuggerHidden]
        public void RunInTransaction(Action a)
        {
            _con.RunInTransaction(a);
        }


        [DebuggerHidden]
        public void RunInImmediateTransaction(Action a)
        {
            if (!_con.IsInTransaction)
            {
                _con.Execute("BEGIN IMMEDIATE TRANSACTION");
                try
                {
                    a();
                    _con.Execute("COMMIT");
                }
                catch (Exception)
                {
                    try
                    {
                        _con.Execute("ROLLBACK");
                    }
                    catch (Exception)
                    {
                    }

                    throw;
                }
            }
            else
            {
                // we re already in a transaction. workaround to force a reserved lock.
                string g = Guid.NewGuid().ToString();
                _con.Execute($"CREATE TEMP TABLE '{g}' (id);");
                _con.Execute($"INSERT INTO '{g}' VALUES (1);");
                _con.Execute($"DROP TABLE '{g}'");

                _con.RunInTransaction(a);
            }
        }

        private class Transaction : ITransaction
        {
            private readonly ISQLiteConnection _con;
            private string _id;

            public Transaction(ISQLiteConnection con)
            {
                _con = con;
                _id = _con.SaveTransactionPoint();
            }

            public void Dispose()
            {
                if (_id != null)
                    _con.RollbackTo(_id);
                _id = null;
            }

            public void Commit()
            {
                if (_id == null)
                    throw new Exception("can only commit once.");
                _con.Release(_id);
                _id = null;
            }
        }

        public ITransaction BeginTransaction()
        {
            return new Transaction(_con);
        }

        public override string ToString()
        {
            var file = Path.GetFileName(_con.DatabasePath);
            if (string.IsNullOrEmpty(TablePrefix))
                return file;
            return string.Format("{0} ({1})", file, TablePrefix);
        }
    }
}
