using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Database;
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
        //private readonly string _authority;
        private int _syncRequired = 0;

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
        public async override void OnChange(bool selfChange, Uri changeUri)
        {
            //if (selfChange) return;
            var account = _account.GetAccounts().FirstOrDefault(a => a.Type == _type);
            if (account == null || !account.IsSyncOnDataChanged) return;
            // self change!
            if (_syncMan.IsSyncActive(account)) return; 

            Interlocked.Increment(ref _syncRequired);

            // allow changes to accumulate
            await Task.Delay(250);

            int syncRequired = Interlocked.Exchange(ref _syncRequired, 0);
            if (syncRequired == 0) return;

            // don't wait.
            #pragma warning disable CS4014
            _syncMan.SyncNowAsync(account);
            #pragma warning restore CS4014
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
                _ctx.ContentResolver.UnregisterContentObserver(this);
            
            base.Dispose(disposing);
        }
    }
}
    
