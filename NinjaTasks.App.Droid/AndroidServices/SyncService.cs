using Android.App;
using Android.Content;
using Android.OS;

namespace NinjaTasks.App.Droid.AndroidServices
{
    [Service(Exported = true/*, Process = ":sync"*/)]
    [IntentFilter(new[] { "android.content.SyncAdapter" })]
    [MetaData("android.content.SyncAdapter", Resource = "@xml/sync_nonsenseapps_notepad")]
    [MetaData("android.content.SyncAdapter", Resource = "@xml/sync_ninjatasks")]
    public class SyncService : Service
    {
        static SyncAdapter _syncAdapter = null;
        static readonly object _syncAdapterLock = new object();

        public override void OnCreate()
        {
            base.OnCreate();

            lock(_syncAdapterLock)
            {
                if(_syncAdapter == null)
                    _syncAdapter = new SyncAdapter(ApplicationContext, true);
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return _syncAdapter.SyncAdapterBinder;
        }
    }
}

