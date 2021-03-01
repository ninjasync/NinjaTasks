using NinjaSync;
using NinjaTasks.Model.Sync;

namespace NinjaTasks.Core.Services
{
    public interface ISyncServiceFactory
    {
        ISyncService Create(SyncAccount account);
        void Destroy(ISyncService sync);

        //SyncStorages CreateStorages();
    }
}
