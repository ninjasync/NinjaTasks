using Android.App;
using Android.Content;
using Android.OS;

namespace NinjaTasks.App.Droid.AndroidServices
{
    [Service(Exported = true/*, Process = ":sync"*/)]
    [IntentFilter(new[] { "android.content.SyncAdapter" })]
    [MetaData("android.content.SyncAdapter", Resource = "@xml/sync_ninjatasks")]
    public class SyncService1 : Service
    {
        private static SyncAdapter _syncAdapter = null;
        private static readonly object _syncAdapterLock = new object();


        public override void OnCreate()
        {
            base.OnCreate();
            EnsureSyncAdapterCreated();
        }

        public static SyncAdapter EnsureSyncAdapterCreated()
        {
            lock (_syncAdapterLock)
            {
                if (_syncAdapter == null)
                    _syncAdapter = new SyncAdapter(Application.Context, true);
                return _syncAdapter;
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return _syncAdapter.SyncAdapterBinder;
        }
    }

    [Service(Exported = true/*, Process = ":sync"*/)]
    [IntentFilter(new[] { "android.content.SyncAdapter" })]
    [MetaData("android.content.SyncAdapter", Resource = "@xml/sync_nonsenseapps_notepad")]
    public class SyncService2 : Service
    {

        public override void OnCreate()
        {
            base.OnCreate();
            SyncService1.EnsureSyncAdapterCreated();
        }

        public override IBinder OnBind(Intent intent)
        {
            return SyncService1.EnsureSyncAdapterCreated().SyncAdapterBinder;
        }
    }
}

