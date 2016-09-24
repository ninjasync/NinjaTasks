using System;
using System.IO;
using System.Threading.Tasks;
using NinjaSync;
using NinjaSync.MasterSlave;
using NinjaSync.P2P;
using NinjaSync.P2P.Serializing;
using NinjaSync.Storage;
using NinjaTasks.Core;
using NinjaTasks.Db.MvxSqlite;
using NinjaTasks.Model.Storage;
using NinjaTasks.Sync.TaskWarrior;
using NinjaTools;
using NinjaTools.Connectivity.Connections;
using TaskWarriorLib.Network;

#if !DOT42
using NinjaTasks.App.Wpf.Services.TcpIp;
#else
using TslConnectionFactory = NinjaTasks.App.Droid.Services.Tls.AndroidTslConnectionFactory;
#endif

namespace NinjaTasks.Tests
{
    class MockSyncStoragesFactory : ISyncStoragesFactory
    {
        private readonly SQLiteFactory _sqlite;
        private readonly MvxSqliteSyncServiceFactory _fac1;

        public MockSyncStoragesFactory(SQLiteFactory sqlite, MvxSqliteSyncServiceFactory fac1)
        {
            _sqlite = sqlite;
            _fac1 = fac1;
        }

        public SyncStorages CreateSyncStorages()
        {
            return _fac1.CreateMirrorStorages(_sqlite.Get("todo"));
        }
    }

    public class MockTlsFailsAtSpecifiedConnectionFactory : ITslConnectionFactory
    {
        private int _connections;
        private readonly int _failAtConnection;
        readonly ITslConnectionFactory _fac = new TslConnectionFactory();

        public MockTlsFailsAtSpecifiedConnectionFactory(int failAtConnection = 2)
        {
            _failAtConnection = failAtConnection;
        }


        public Stream ConnectAndSecureFromFiles(string server, int port, 
            string clientCertificateAndKeyPfxFile,string serverCertificatePfxFile,
            int genericTimeoutMs
            )
        {
            if (++_connections == _failAtConnection)
                throw new Exception("simulating network unreachable");
            return _fac.ConnectAndSecureFromFiles(server, port, clientCertificateAndKeyPfxFile, serverCertificatePfxFile, genericTimeoutMs);
        }

        public Stream ConnectAndSecureFromPem(string server, int port, string clientCertificateAndKeyPem, string serverCertificatePem, int genericTimeoutMs)
        {
            if (++_connections == _failAtConnection)
                throw new Exception("simulating network unreachable");
            return _fac.ConnectAndSecureFromPem(server, port, clientCertificateAndKeyPem, serverCertificatePem, genericTimeoutMs);
        }
    }

    internal class MirrorSetupFactory
    {
        public static MirrorSetup LocalToP2P_Pipes(string sync1Name, string sync2Name)
        {
            var conFac = Helpers.CreateConnectionFactory();
            var syncFac = new MvxSqliteSyncServiceFactory(new TslConnectionFactory());

            var trackableSerializer = new JsonNetModificationSerializer(new TodoTrackableFactory());

            TokenBag keep = new TokenBag();
            PipeStreamFactory remote1Listens = new PipeStreamFactory(1024 * 1024, "pipes1");
            PipeStreamFactory remote2Listens = new PipeStreamFactory(1024 * 1024, "pipes2");

            // remote1.
            var facRemote1 = new SQLiteFactory(conFac, sync1Name + ".sqlite");
            Helpers.ClearDatabase(facRemote1);
            keep += facRemote1;

            TodoSyncStorages storages1 = syncFac.CreateMirrorStorages(facRemote1.Get("todo"));


            var client1 = new P2PSyncRemoteEndpoint(remote2Listens, trackableSerializer);
            var sync1 = syncFac.CreateP2PSync(facRemote1.Get("todo"), client1, "unified-sync1");
            var server1 = new P2PServer(remote1Listens.GetListener(null), remote1Listens,
                                 new MockSyncStoragesFactory(facRemote1, syncFac), new JsonNetModificationSerializer(new TodoTrackableFactory()), "sync1-server");
            Task.Run(() => server1.StartServing());
            keep += server1;

            // remote2.
            var facRemote2 = new SQLiteFactory(conFac, sync2Name + ".sqlite");
            Helpers.ClearDatabase(facRemote2);
            keep += facRemote2;

            TodoSyncStorages storages2 = syncFac.CreateMirrorStorages(facRemote2.Get("todo"));

            var client2 = new P2PSyncRemoteEndpoint(remote1Listens, trackableSerializer);
            var sync2 = syncFac.CreateP2PSync(facRemote2.Get("todo"), client2, "unified-sync2");
            var server2 = new P2PServer(remote2Listens.GetListener(null), remote2Listens,
                                 new MockSyncStoragesFactory(facRemote2, syncFac), new JsonNetModificationSerializer(new TodoTrackableFactory()), "sync2-server");
            Task.Run(() => server2.StartServing());
            keep += server2;

            return new MirrorSetup
            {
                Keep = keep,
                Storage1 = storages1.Todo,
                Sync1 = sync1,
                Storage2 = storages2.Todo,
                Sync2 = sync2,
            };
        }

        public static MirrorSetup LocalToP2P_Direct(string sync1Name, string sync2Name)
        {
            var conFac = Helpers.CreateConnectionFactory();
            var syncFac = new MvxSqliteSyncServiceFactory(new TslConnectionFactory());

            TokenBag keep = new TokenBag();

            // remote1.
            var facRemote1 = new SQLiteFactory(conFac, sync1Name + ".sqlite");
            Helpers.ClearDatabase(facRemote1);
            keep += facRemote1;

            TodoSyncStorages storages1 = syncFac.CreateMirrorStorages(facRemote1.Get("todo"));
            var master1 = new TrackableStorageP2PSyncEndpoint(storages1.Storage);

            // remote2.
            var facRemote2 = new SQLiteFactory(conFac, sync2Name + ".sqlite");
            Helpers.ClearDatabase(facRemote2);
            keep += facRemote2;

            TodoSyncStorages storages2 = syncFac.CreateMirrorStorages(facRemote2.Get("todo"));
            var master2 = new TrackableStorageP2PSyncEndpoint(storages2.Storage);

            var sync1 = syncFac.CreateP2PSync(facRemote1.Get("todo"), master2, "unified-sync1");

            var sync2 = syncFac.CreateP2PSync(facRemote2.Get("todo"), master1, "unified-sync2");

            return new MirrorSetup
            {
                Keep = keep,
                Storage1 = storages1.Todo,
                Sync1 = sync1,
                Storage2 = storages2.Todo,
                Sync2 = sync2,
            };
        }

        public static MirrorSetup LocalToLocal(string sync1Name, string sync2Name, bool idMap1, bool idMap2, bool implicitListsOn1, bool implicitListsOn2)
        {
            var conFac = Helpers.CreateConnectionFactory();
            var syncFac = new MvxSqliteSyncServiceFactory(null);

            // central table
            var facLocal = new SQLiteFactory(conFac, "local-to-local.sqlite");
            Helpers.ClearDatabase(facLocal);

            // 1.extern.
            var facRemote1 = new SQLiteFactory(conFac, sync1Name + ".sqlite");
            Helpers.ClearDatabase(facRemote1);
            var remote1Storage = new MvxSqliteTodoStorage(facRemote1.Get("tests"));
            var remote1Extern = implicitListsOn1 ? (ITrackableRemoteSlaveStorage)
                                                   new TodoRemoteSlaveWithFakeListsStorageWrapper(remote1Storage)
                                                 : new TrackableRemoteDumbSlaveStorage(remote1Storage);

            // <List, ExternTest1Task, ExternTest1TaskProperty, ExternTest1Journal, ExternTest1CommitEntry, 
            //  TodoList, TodoTask, TodoTaskProperty, JournalEntry, CommitEntry, ExternTest1Map>
            var syncManager1 = syncFac.CreateLocalSyncManager(facLocal.Get("todo"), remote1Extern, "ExternTest1",
                                                              null, sync1Name, false, idMap1);

            // 2.extern using the same table, but a different map.
            var facRemote2 = new SQLiteFactory(conFac, sync2Name + ".sqlite");
            Helpers.ClearDatabase(facRemote2);
            var remote2Storage = new MvxSqliteTodoStorage(facRemote2.Get("test"));
            var remote2Extern = implicitListsOn1 ? (ITrackableRemoteSlaveStorage)
                                                   new TodoRemoteSlaveWithFakeListsStorageWrapper(remote2Storage)
                                                 : new TrackableRemoteDumbSlaveStorage(remote2Storage);

            var syncManager2 = syncFac.CreateLocalSyncManager(facLocal.Get("todo"), remote2Extern, "ExternTest2",
                                                              null, sync2Name, false, idMap2);


            return new MirrorSetup
            {
                Factories = new[] { facLocal, facRemote1, facRemote2 },
                Storage1 = remote1Storage,
                Sync1 = syncManager1,
                Storage2 = remote2Storage,
                Sync2 = syncManager2,
            };
        }

        /// <summary>
        /// two independent databases, syncing to the same Task-Warrior account.
        /// </summary>
        public static MirrorSetup LocalToTw(string sync1Name, string sync2Name, bool withIdMap = false)
        {
            var conFac = Helpers.CreateConnectionFactory();

            var syncFac = new MvxSqliteSyncServiceFactory(new TslConnectionFactory());

            // NOTE: to understands what's happening here, it might be 
            //       best to draw the different storages into an image.

            // 1.extern.
            var facremote1 = new SQLiteFactory(conFac, sync1Name + ".sqlite");
            var conremote1 = facremote1.Get("todo");
            Helpers.ClearDatabase(facremote1);

            var storage1 = new MvxSqliteTodoStorage(conremote1, "ExternTest1");

            var twSyncStorageRemote1 = new MvxSqliteSyncAccountStorageService(facremote1);
            var twRemote1 = new TaskWarriorFixRemoteStorage(new TslConnectionFactory(), twSyncStorageRemote1,
                                                              Helpers.Account);

            // <ExternTest1List, ExternTest1Task, ExternTest1TaskProperty, ExternTest1Journal, ExternTest1CommitEntry, ExternTest1Map>
            ISyncService sync1 = syncFac.CreateRemoteSync(facremote1.Get("todo"), twRemote1, sync1Name + "-account", "ExternTest1", withIdMap);

            // 2. extern
            var facremote2 = new SQLiteFactory(conFac, sync2Name + ".sqlite");
            var conremote2 = facremote1.Get("todo");
            Helpers.ClearDatabase(facremote2);

            var storage2 = new MvxSqliteTodoStorage(conremote2, "ExternTest2");

            var twSyncStorageRemote2 = new MvxSqliteSyncAccountStorageService(facremote2);
            var twRemote2 = new TaskWarriorFixRemoteStorage(new TslConnectionFactory(), twSyncStorageRemote2,
                                                              Helpers.Account);

            ISyncService sync2 = syncFac.CreateRemoteSync(facremote2.Get("todo"), twRemote2, sync2Name + "-account", "ExternTest2", withIdMap);

            return new MirrorSetup
            {
                Factories = new[] { facremote1, facremote2 },
                Storage1 = storage1,
                Sync1 = sync1,
                Storage2 = storage2,
                Sync2 = sync2,
            };
        }

        /// <summary>
        /// two independent databases, syncing to the same Task-Warrior account.
        /// </summary>
        public static MirrorSetup RemoteToLocalToTw(string sync1Name, string sync2Name, int failSync2AtConnection = -1)
        {
            var conFac = Helpers.CreateConnectionFactory();

            var syncFac = new MvxSqliteSyncServiceFactory(new TslConnectionFactory());
            // allow simulation of connection failure.
            var tlsFac2 = failSync2AtConnection > 0 ? (ITslConnectionFactory) new MockTlsFailsAtSpecifiedConnectionFactory(failSync2AtConnection) : new TslConnectionFactory();

            // extern 1
            var facRemote1 = new SQLiteFactory(conFac, sync1Name + ".sqlite");
            Helpers.ClearDatabase(facRemote1);
            var remote1Storage = new MvxSqliteTodoStorage(facRemote1.Get("todo"));
            var remote1Extern = new TodoRemoteSlaveWithFakeListsStorageWrapper(remote1Storage);

            // local 1
            var facLocal1 = new SQLiteFactory(conFac, sync1Name + "-local-to-tw.sqlite");
            Helpers.ClearDatabase(facLocal1);

            var twSyncStorage1 = new MvxSqliteSyncAccountStorageService(facLocal1);
            var twRemote1Fixed = new TaskWarriorFixRemoteStorage(new TslConnectionFactory(), twSyncStorage1, Helpers.Account);

            // <TodoList,TodoTask,TodoTaskProperty,JournalEntry,CommitEntry,ExternTest2Map,NullIdMap>
            var sync1 = syncFac.CreateSyncBetweenSlaveAndMaster(facLocal1.Get("todo"), remote1Extern, twRemote1Fixed, null, sync1Name, false, true, false);

            // extern 2.
            var facRemote2 = new SQLiteFactory(conFac, sync2Name + ".sqlite");
            Helpers.ClearDatabase(facRemote2);
            var remote2Storage = new MvxSqliteTodoStorage(facRemote2.Get("todo"));
            var remote2Extern = new TodoRemoteSlaveWithFakeListsStorageWrapper(remote2Storage);

            // local 2
            var facLocal2 = new SQLiteFactory(conFac, sync2Name + "-local-to-tw.sqlite");
            Helpers.ClearDatabase(facLocal2);

            var twSyncStorage2 = new MvxSqliteSyncAccountStorageService(facLocal2);
            var twRemote2Fixed = new TaskWarriorFixRemoteStorage(tlsFac2, twSyncStorage2, Helpers.Account);

            var sync2 = syncFac.CreateSyncBetweenSlaveAndMaster(facLocal2.Get("todo"), remote2Extern, twRemote2Fixed, 
                                                                null, sync1Name, false, true, false);
            return new MirrorSetup
            {
                Factories = new[] { facRemote1, facRemote2, facLocal1, facLocal2 },
                Storage1 = remote1Storage,
                Sync1 = sync1,
                Storage2 = remote2Storage,
                Sync2 = sync2,
            };
        }
    }
}
