using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Database;
using Java.Util.Concurrent.Atomic;
using NinjaTasks.Core.Services;
using NinjaTasks.Model.Storage;
using NinjaTasks.Model.Sync;
using Uri = Android.Net.Uri;

namespace NinjaTasks.App.Droid.Services
{
    public class AndroidSyncOnContentProviderChanged : ContentObserver, IDisposable
    {
        private readonly Context _ctx;
        private readonly IAccountsStorage _account;
        private readonly ISyncManager _syncMan;
        private readonly SyncAccountType _type;
        private readonly string _authority;
        private readonly AtomicInteger _syncRequired = new AtomicInteger();

        public AndroidSyncOnContentProviderChanged(Context ctx, 
                                                   IAccountsStorage account, 
                                                   ISyncManager syncMan,
                                                   SyncAccountType type, 
                                                   Uri listen)
            :base(null)
        {
            _ctx = ctx;
            _account = account;
            _syncMan = syncMan;
            _type = type;
            ctx.ContentResolver.RegisterContentObserver(listen, true, this);
            
        }

        /*
         * Define a method that's called when data in the
         * observed content provider changes.
         * This method signature is provided for compatibility with
         * older platforms.
         */
        public override void OnChange(bool selfChange) 
        {
            /*
             * Invoke the method signature available as of
             * Android platform version 4.1, with a null URI.
             */
            OnChange(selfChange, null);
        }
        /*
         * Define a method that's called when data in the
         * observed content provider changes.
         */
        public async /*override*/ void OnChange(bool selfChange, Uri changeUri)
        {
            //if (selfChange) return;
            var account = _account.GetAccounts().FirstOrDefault(a => a.Type == _type);
            if (account == null || !account.IsSyncOnDataChanged) return;
            // self change!
            if (_syncMan.IsSyncActive(account)) return;

            _syncRequired.IncrementAndGet();

            // allow changes to accumulate
            await Task.Delay(250);

            int syncRequired = _syncRequired.GetAndSet(0);
            if (syncRequired == 0) return;

            // don't wait.
            _syncMan.SyncNowAsync(account);
        }

        public void Dispose()
        {
            _ctx.ContentResolver.UnregisterContentObserver(this);
        }
    }
}
    
