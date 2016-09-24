using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using NinjaSync.Model;
using NinjaSync.Storage;
using NinjaSync.Storage.MvxSqlite;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;

namespace NinjaTasks.Db.MvxSqlite
{
    public class MvxSqliteSyncAccountStorageService : IAccountsStorage, ISyncStatusStorage, ITaskWarriorAccountsStorage
    {
        private readonly ISQLiteConnection _connection;

        private static readonly Dictionary<string, SemaphoreSlim> Syncs = new Dictionary<string, SemaphoreSlim>();
        private readonly SqliteExpressionBuilder _taskWarrior;
        private readonly SqliteExpressionBuilder _remoteRepr;
        private readonly SqliteExpressionBuilder _syncAccount;
        private readonly SqliteExpressionBuilder _syncStatus;

        public MvxSqliteSyncAccountStorageService(SQLiteFactory factory)
            : this(factory.Get("accounts"))
        {
        }

        public MvxSqliteSyncAccountStorageService(ISQLiteConnection sqlite)
        {
            _connection = sqlite;
            _connection.EnsureTableCreated<SyncStatus>();
            _connection.EnsureTableCreated<TaskWarriorAccount>();
            _connection.EnsureTableCreated<SyncRemoteRepresentation>();
            _connection.EnsureTableCreated<SyncAccount>();

            _taskWarrior = new SqliteExpressionBuilder(_connection.GetMapping<TaskWarriorAccount>().TableName);
            _remoteRepr = new SqliteExpressionBuilder(_connection.GetMapping<SyncRemoteRepresentation>().TableName);
            _syncAccount = new SqliteExpressionBuilder(_connection.GetMapping<SyncAccount>().TableName);
            _syncStatus = new SqliteExpressionBuilder(_connection.GetMapping<SyncStatus>().TableName);
        }


        public IEnumerable<TaskWarriorAccount> GetTaskWarriorAccounts()
        {
            return _connection.Query<TaskWarriorAccount>(_taskWarrior.Select());
        }

        public SyncAccount SaveAccount(TaskWarriorAccount account)
        {
            // make sure the accounts share the same id.
            var twAccount = GetAccounts().FirstOrDefault(p => p.Type == SyncAccountType.TaskWarrior && p.Id == account.Id);
            if (twAccount == null)
                twAccount = new SyncAccount();

            twAccount.Type = SyncAccountType.TaskWarrior;
            twAccount.Name = "Taskwarrior";
            twAccount.Address = account.ServerHostname + ":" + account.ServerPort;

            SaveAccount(twAccount);

            account.Id = twAccount.Id;
            _connection.InsertOrReplace(account);

            return twAccount;
        }

        public void SaveRepresentation(SyncRemoteRepresentation map)
        {
            Debug.Assert(!string.IsNullOrEmpty(map.AccountId));
            Debug.Assert(!string.IsNullOrEmpty(map.Uuid));
            Debug.Assert(!string.IsNullOrEmpty(map.Representation));

            _connection.InsertOrReplace(map);
        }

        public void Delete(SyncAccount account)
        {
            _connection.Delete(account);
            _connection.Execute(string.Format("DELETE FROM '{0}' WHERE AccountId=?",
                                              _connection.GetMapping<SyncStatus>().TableName),
                                account.AccountId);
            _connection.Execute(string.Format("DELETE FROM '{0}' WHERE AccountId=?",
                                              _connection.GetMapping<SyncRemoteRepresentation>().TableName),
                                account.AccountId);
            _connection.Execute(string.Format("DELETE FROM '{0}' WHERE Id=?",
                                              _connection.GetMapping<TaskWarriorAccount>().TableName),
                                account.Id);
        }

        public SyncRemoteRepresentation GetRepresentation(string accountId, string uuid)
        {
            // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
            return _connection.NxTable<SyncRemoteRepresentation>()
                              .Where("AccountId=? AND Uuid=?", accountId, uuid)
                //.Where(p=>p.AccountId ==accountId && p.Uuid == uuid)
                              .FirstOrDefault();
        }

        public IEnumerable<SyncAccount> GetAccounts()
        {
            return _connection.NxTable<SyncAccount>();
        }

        public void SaveAccount(SyncAccount account, IList<string> selectedProperties = null)
        {
            if (account.Id == 0)
                _connection.Insert(account);
            else
                _connection.Update(account, selectedProperties);
        }

        public SemaphoreSlim GetAccountSemaphore(string accountId)
        {
            lock (Syncs)
            {
                string key = _connection.DatabasePath + "|" + accountId;
                SemaphoreSlim ret;
                if (Syncs.TryGetValue(key, out ret))
                    return ret;
                return Syncs[key] = new SemaphoreSlim(1);
            }
        }

        public SyncStatus GetStatus(string accountTypeAndId)
        {
            // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault
            var ret = _connection.NxTable<SyncStatus>()
                                 .Where("AccountId=?", accountTypeAndId)
                //.Where(w => w.AccountId == accountTypeAndId)
                                 .FirstOrDefault();
            if (ret == null)
            {
                ret = new SyncStatus { AccountId = accountTypeAndId };
                _connection.Insert(ret);
            }

            return ret;
        }

        public SyncStatus GetStatusByRemoteStorageId(string remoteStorageId)
        {
            return _connection.NxTable<SyncStatus>()
                              .Where("RemoteStorageId=?", remoteStorageId)
                //.FirstOrDefault(p => p.RemoteStorageId == remoteStorageId);
                              .FirstOrDefault();
        }

        public void SaveStatus(SyncStatus status)
        {
            Debug.Assert(!string.IsNullOrEmpty(status.AccountId));
            // if the status does not exist any more, it was deleted!
            _connection.Update(status);
        }
    }
}