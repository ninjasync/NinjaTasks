using Android.Content;
using NinjaSync;
using NinjaSync.Model;
using NinjaTasks.App.Droid.RemoteStorages.NonsenseApps;
using NinjaTasks.App.Droid.RemoteStorages.org.Tasks;
using NinjaTasks.Core;
using NinjaTasks.Core.Services;
using NinjaTasks.Db.MvxSqlite;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using NinjaTools.Connectivity;
using TaskWarriorLib.Network;

namespace NinjaTasks.App.Droid.Services
{
    public class AndroidSyncServiceFactory : SqliteSyncServiceFactory
    {
        private readonly Context _ctx;
        private readonly ITslConnectionFactory _tsl;
        private readonly SQLiteFactory _sqlite;

        public AndroidSyncServiceFactory(Context ctx,
                                         ITaskWarriorAccountsStorage tw,
                                         ITslConnectionFactory tsl,
                                         SQLiteFactory sqlite,
                                         IBluetoothStreamSubsystem bluetooth,
                                         ITcpStreamSubsystem tcpip) :
            base(tw, tsl, sqlite, bluetooth, tcpip)
        {
            _ctx = ctx;
            _tsl = tsl;
            _sqlite = sqlite;
        }

        public override ISyncService Create(SyncAccount account)
        {
            return Create(account, null);

        }

        public ISyncService Create(SyncAccount account, ContentProviderClient cpClient)
        {
            if (account.Type == SyncAccountType.NonsenseAppsNotePad)
            {
                var sqlite = _sqlite.Clone();
                return new SyncServiceWrapper(CreateNotePadSync(account, cpClient), sqlite);
            }
            if (account.Type == SyncAccountType.OrgTasks)
            {
                var sqlite = _sqlite.Clone();
                return new SyncServiceWrapper(CreatedOrgTasksSync(account, cpClient), sqlite);
            }

            return base.Create(account);
        }

        public ISyncService CreateNotePadSync(SyncAccount account, ContentProviderClient provider)
        {
            if (provider == null)
                provider = _ctx.ContentResolver.AcquireContentProviderClient(NpContract.Authority);

            var npRemote = new NotePadRemoteStorage(provider);

            var syncFac = new MvxSqliteSyncServiceFactory(_tsl);

            return syncFac.CreateLocalSyncManager(_sqlite.Get("sync"), npRemote, "ExternNonseAppsNotePad", null, account.AccountId, true);
        }

        public ISyncService CreatedOrgTasksSync(SyncAccount account, ContentProviderClient provider)
        {
            if (provider == null)
                provider = _ctx.ContentResolver.AcquireContentProviderClient(TasksContract.Authority);

            var npRemote = new OrgTasksRemoteStorage(provider);

            var syncFac = new MvxSqliteSyncServiceFactory(_tsl);

            return syncFac.CreateLocalSyncManager(_sqlite.Get("sync"), npRemote, "ExternOrgTasks", null, account.AccountId, true, true);
        }

        //public SyncStorages CreateNotePadMirrorStorages(SQLiteFactory sqlite)
        //{
        //    var syncFac = new MvxSqliteSyncServiceFactory(_tsl);
        //    return syncFac.CreateMirrorStorages<ExternNonseAppsNotePadList,
        //                                        ExternNonseAppsNotePadTask,
        //                                        ExternNonseAppsNotePadTaskProperty,
        //                                        ExternNonseAppsNotePadJournal,
        //                                        ExternNonseAppsNoteCommitEntry,
        //                                        NullIdMap>(sqlite);
        //}
    }
}