using System.Threading.Tasks;

namespace NinjaSync
{
    public interface ISyncService
    {
        void Sync(ISyncProgress progress);
        Task SyncAsync(ISyncProgress progress);
    }

  
}