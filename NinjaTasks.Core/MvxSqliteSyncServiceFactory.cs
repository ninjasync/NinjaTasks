using NinjaTools.Sqlite;
using NinjaSync;
using NinjaSync.MasterSlave;
using NinjaSync.Model;
using NinjaSync.P2P;
using NinjaSync.Storage;
using NinjaSync.Storage.MvxSqlite;
using NinjaTasks.Db.MvxSqlite;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTasks.Sync;
using NinjaTasks.Sync.TaskWarrior;
using TaskWarriorLib.Network;

namespace NinjaTasks.Core
{
    public class MvxSqliteSyncServiceFactory
    {
        private readonly ITslConnectionFactory _tslConnectionFactory;

        public MvxSqliteSyncServiceFactory(ITslConnectionFactory tslConnectionFactory)
        {
            _tslConnectionFactory = tslConnectionFactory;
        }

        public ISyncService CreateTaskWarriorSync(TaskWarriorAccount account, TodoSyncStorages localStorages)
        {
            var twRemoteFixed = new TaskWarriorFixRemoteStorage(_tslConnectionFactory, localStorages.Status, account);
            var twRemoteMapped = new ListMappingTodoRemoteMasterStorageAdapter(twRemoteFixed, localStorages.Todo);

            var endpoint = new LocalSlaveSyncEndpoint(twRemoteFixed, localStorages.Storage);
            return new SyncWithMasterService(twRemoteMapped, endpoint, localStorages.Status, "TaskWarrior:" + account.Id);
        }

        public ISyncService CreateTaskWarriorSync(TaskWarriorAccount account, ISQLiteConnection sqlite)
        {
            TodoSyncStorages storages = new TodoSyncStorages();

            storages.Todo = new MvxSqliteTodoStorage(sqlite);
            storages.Storage = new TrackableJournalStorageAdapter(storages.Todo, new MvxSqliteJournalStorage(sqlite));
            storages.Status = new MvxSqliteSyncAccountStorageService(sqlite);

            return CreateTaskWarriorSync(account, storages);
        }

        public ISyncWithMasterService CreateRemoteSync(ISQLiteConnection sqlite, ITrackableRemoteMasterStorage remote, string accountName,
                                                       string tablePrefix, bool createIdMap = false)
        {
            var storages = CreateMirrorStorages(sqlite, tablePrefix);

            remote = WrapRemoteIfNecessary(sqlite, remote, storages, createIdMap, tablePrefix);

            var endpoint = new LocalSlaveSyncEndpoint(remote, storages.Storage);
            return new SyncWithMasterService(remote, endpoint, storages.Status, accountName);
        }

        public ISyncWithMasterService CreateRemoteSync(ISQLiteConnection sqlite, ITrackableRemoteMasterStorage remote, string accountName)
        {
            var storages = CreateMirrorStorages(sqlite);
            var endpoint = new LocalSlaveSyncEndpoint(remote, storages.Storage);
            return new SyncWithMasterService(remote, endpoint, storages.Status, accountName);
        }
        /// <summary>
        /// This will create a two-way sync beween master and slave, using the specified tables as backing storage.
        /// [and especially as change-tracker for slave]
        /// </summary>
        /// <returns></returns>
        public ISyncService CreateSyncBetweenSlaveAndMaster(ISQLiteConnection sqlite,
                                        ITrackableRemoteSlaveStorage slave, ITrackableRemoteMasterStorage master,
                                        string tablePrefix, string accountNameBase, bool invertLocalRemoteMeaningInProgress,
                                        bool createSlaveIdMap = false, bool createMasterIdMap = false)
        {
            var storage = CreateMirrorStorages(sqlite, tablePrefix);

            slave = WrapRemoteIfNecessary(sqlite, slave, storage, createSlaveIdMap, tablePrefix);
            master = WrapRemoteIfNecessary(sqlite, master, storage, createMasterIdMap, tablePrefix);

            var syncManager = new SyncBetweenSlaveAndMasterService(master, slave, storage,
                                                            accountNameBase + ":Slave",
                                                            accountNameBase + ":Master",
                                                            invertLocalRemoteMeaningInProgress);
            return syncManager;

        }

        /// <summary>
        /// todo: find a better name
        /// </summary>
        public ISyncService CreateLocalSyncManager(ISQLiteConnection sqlite, ITrackableRemoteSlaveStorage slave,
                                                    string tablePrefixSlave, string tablePrefixMaster, string mirrorAccountNameBase,
                                                   bool invertRemoteAndLocalMeaningInProgress, bool createMapSlavesMaster = false)
        {
            var masterStorage = new MvxSqliteTodoStorage(sqlite, tablePrefixMaster);
            var masterJournal = new MvxSqliteJournalStorage(sqlite, tablePrefixMaster);
            var masterRemote = new TrackableRemoteMasterStorage(new TrackableJournalStorageAdapter(masterStorage, masterJournal));

            return CreateSyncBetweenSlaveAndMaster(sqlite, slave, masterRemote, tablePrefixSlave, mirrorAccountNameBase,
                                                   invertRemoteAndLocalMeaningInProgress, createMapSlavesMaster);
        }

        private static ITrackableRemoteMasterStorage WrapRemoteIfNecessary(ISQLiteConnection sqlite,
                ITrackableRemoteMasterStorage remote, TodoSyncStorages storages, bool needsIdMapping, string tablePrefix)
        {
            var todoRemote = remote as ITodoRemoteMasterStorage;
            if (todoRemote == null) return remote;

            // we want to map lists first, than ids,
            // so we have to create the adapters in reverse order
            if (needsIdMapping)
            {
                // only create a mapping if required.
                var mapStorage = new MvxSqliteStringMappingStorage(sqlite, tablePrefix);
                todoRemote = new IdMappingTodoRemoteMasterStorageAdapter(todoRemote, mapStorage, todoRemote.HasOnlyImplicitLists());
            }

            if (todoRemote.HasOnlyImplicitLists())
            {
                // create a list wrapper
                todoRemote = new ListMappingTodoRemoteMasterStorageAdapter(todoRemote, storages.Todo);
            }
            return todoRemote;
        }

        private static ITrackableRemoteSlaveStorage WrapRemoteIfNecessary(ISQLiteConnection sqlite,
                                                   ITrackableRemoteSlaveStorage slave, TodoSyncStorages storage,
                                                   bool needsIdMapping, string tablePrefix)
        {
            // only create a mapping if required.
            var todoRemote = slave as ITodoRemoteSlaveStorage;
            if (todoRemote == null) return slave;

            // we want to map lists first, than ids,
            // so we have to create the adapters in reverse order
            if (needsIdMapping)
            {
                var mapStorage = new MvxSqliteStringMappingStorage(sqlite, tablePrefix);
                todoRemote = new IdMappingTodoRemoteSlaveStorageAdapter(todoRemote, mapStorage,
                               todoRemote.HasOnlyImplicitLists());
            }

            if (todoRemote.HasOnlyImplicitLists())
                todoRemote = new ListMappingTodoRemoteSlaveStorageAdapter(todoRemote, storage.Todo);

            return todoRemote;
        }


        public TodoSyncStorages CreateMirrorStorages(ISQLiteConnection sqlite, string tablePrefix = null)
        {
            TodoSyncStorages storage = new TodoSyncStorages();
            var todo = new MvxSqliteTodoStorage(sqlite, tablePrefix);
            storage.Todo = todo;
            storage.Storage = new TrackableJournalStorageAdapter(todo, new MvxSqliteJournalStorage(sqlite, tablePrefix));
            storage.Status = new MvxSqliteSyncAccountStorageService(sqlite);

            return storage;
        }

        public ISyncService CreateP2PSync(ISQLiteConnection sqlite, IP2PSyncRemoteEndpoint remote, string accountId)
        {
            SyncStorages mirror = CreateMirrorStorages(sqlite);
            var local = new TrackableStorageP2PSyncEndpoint(mirror.Storage);
            return new P2PSyncService(local, remote, mirror.Status, accountId);

        }
    }
}

