using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NinjaSync;
using NinjaSync.P2P;
using NinjaSync.P2P.Serializing;
using NinjaSync.Storage;
using NinjaTasks.Core.ViewModels.Sync;
using NinjaTasks.Db.MvxSqlite;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTools;
using NinjaTools.Connectivity;
using NinjaTools.Connectivity.Discover;
using TaskWarriorLib.Network;

namespace NinjaTasks.Core.Services
{
    public class SqliteSyncServiceFactory : ISyncServiceFactory, ISyncStoragesFactory
    {
        public static readonly Guid BluetoothGuid = new Guid("10219B33-782B-4E22-9B79-E360CED08465");

        private readonly ITaskWarriorAccountsStorage _twStorage;
        private readonly ITslConnectionFactory _tsl;
        private readonly SQLiteFactory _sqlite;
        private readonly IBluetoothStreamSubsystem _bluetooth;
        private readonly ITcpStreamSubsystem _tcpip;
        private readonly MvxSqliteSyncServiceFactory _factory;

        public SqliteSyncServiceFactory(ITaskWarriorAccountsStorage twStorage, 
                                        ITslConnectionFactory tsl,
                                        SQLiteFactory sqlite,
                                        IBluetoothStreamSubsystem bluetooth,
                                        ITcpStreamSubsystem tcpip)
        {
            _twStorage = twStorage;
            _tsl = tsl;
            _sqlite = sqlite;
            _bluetooth = bluetooth;
            _tcpip = tcpip;
            _factory = new MvxSqliteSyncServiceFactory(tsl);
        }

        public virtual ISyncService Create(SyncAccount account)
        {
            if (account.Type == SyncAccountType.TaskWarrior)
            {
                var twAccount = _twStorage.GetTaskWarriorAccounts()
                                          .First(t => t.Id == account.Id);
                var sqlite = _sqlite.Clone();
                return new SyncServiceWrapper(_factory.CreateTaskWarriorSync(twAccount, sqlite.Get("sync")), sqlite);
            }
            if (account.Type == SyncAccountType.BluetoothP2P)
            {

                var deviceInfo = new Endpoint(EndpointType.Bluetooth, account.Name, account.Address, BluetoothGuid.ToString());
                var connector = _bluetooth.GetConnector(deviceInfo);
                var remote = new P2PSyncRemoteEndpoint(connector, new JsonNetModificationSerializer(new TodoTrackableFactory()));
                var sqlite = _sqlite.Clone();
                return new SyncServiceWrapper(_factory.CreateP2PSync(sqlite.Get("sync"), remote, account.AccountId), sqlite);
            }
            if (account.Type == SyncAccountType.TcpIpP2P)
            {
                if(_tcpip == null)
                    throw new NotImplementedException("tcp/ip not yet supported.");

                var address = SelectTcpIpHostViewModel.SplitHostAndPort(account.Address);
                var deviceInfo = Endpoint.IpTarget(address.Item1, address.Item2).WithName(account.Name);
                var connector = _tcpip.GetConnector(deviceInfo);
                var remote = new P2PSyncRemoteEndpoint(connector, new JsonNetModificationSerializer(new TodoTrackableFactory()));
                var sqlite = _sqlite.Clone();
                return new SyncServiceWrapper(_factory.CreateP2PSync(sqlite.Get("sync"), remote, account.AccountId), sqlite);
            }

            throw new Exception("unknown account type: " + account.Type);
        }

        public virtual void Destroy(ISyncService service)
        {
            if(service is SyncServiceWrapper)
                ((SyncServiceWrapper)service).Parent.Dispose();
            else
            {
                Debug.Assert(false);
            }
        }

        public SyncStorages CreateSyncStorages()
        {
            SQLiteFactory fac = _sqlite.Clone();
            return new SyncStoragesWrapper(_factory.CreateMirrorStorages(fac.Get("sync")), fac);
        }

        protected class SyncStoragesWrapper : TodoSyncStorages
        {
            private readonly SQLiteFactory _fac;

            public SyncStoragesWrapper(TodoSyncStorages storages, SQLiteFactory fac)
            {
                _fac = fac;
                Status = storages.Status;
                Storage = storages.Storage;
                Todo = storages.Todo;

            }
            public override void Dispose()
            {
                base.Dispose();
                _fac.Dispose();
            }
        }

        protected class SyncServiceWrapper : ISyncService
        {
            private readonly ISyncService _s;
            public SQLiteFactory Parent { get; private set; }

            public SyncServiceWrapper(ISyncService s, SQLiteFactory parent)
            {
                _s = s;
                Parent = parent;
            }

            public void Sync(ISyncProgress progress)
            {
                _s.Sync(progress);
            }

            public Task SyncAsync(ISyncProgress progress)
            {
                return _s.SyncAsync(progress);
            }
        }
    }
}
