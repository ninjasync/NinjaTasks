using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using NinjaSync;
using NinjaTasks.Model.Sync;

namespace NinjaTasks.Core.Services
{
    public interface ISyncManager : IDisposable, INotifyPropertyChanged
    {
        bool IsEnabled { get; set; }
        
        bool IsSyncActive(SyncAccount account);
        int ActiveSyncs { get; }

        /// <summary>
        /// this overload will catch, record and suppress any exceptions
        /// </summary>
        Task SyncNowAsync(SyncAccount account, CancellationToken cancel = default(CancellationToken), bool isManualSync=true);
        /// <summary>
        /// this overload will catch, record and suppress any exceptions
        /// </summary>
        Task SyncNowAsync(CancellationToken cancel = default(CancellationToken));

        /// <summary>
        /// this overload will throw back any expression at the caller, after recording them.
        /// </summary>
        Task SyncNowAsync(SyncAccount account, ISyncProgress progress, bool isManualSync=false);
        
        void RefreshAccounts();
    }
}
