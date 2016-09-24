using Android.App;
using Android.Content;
using Android.OS;
using Dot42.Manifest;

namespace NinjaTasks.App.Droid.AndroidServices
{
    [Service(Exported = true/*, Process = ":sync"*/)]
    [IntentFilter(Actions = new[] { "android.content.SyncAdapter" })]
    [MetaData(Name = "android.content.SyncAdapter", Resource = "@xml/sync_nonsenseapps_notepad")]
    [MetaData(Name = "android.content.SyncAdapter", Resource = "@xml/sync_ninjatasks")]
    public class SyncService : Service
    {
        private static SyncAdapter _syncAdapter = null;
        private static readonly object SyncAdapterLock = new object();

        public override void OnCreate()
        {
            base.OnCreate();

            lock(SyncAdapterLock)
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

