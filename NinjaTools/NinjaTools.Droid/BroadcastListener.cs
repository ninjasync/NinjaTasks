using Android.Content;

namespace NinjaTools.Droid
{
    public interface IBroadcastReceiver
    {
        void OnBroadcastReceived(Context context, Intent intent);
    }
#if !DOT42
    public class BroadcastListener : BroadcastReceiver
    {
        private readonly HiddenReference<IBroadcastReceiver>  _parent = new HiddenReference<IBroadcastReceiver>();

        public BroadcastListener(IBroadcastReceiver parent)
        {
            _parent.Value = parent;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            _parent.Value.OnBroadcastReceived(context, intent);
                
        }
    }
#else
    public class BroadcastListener : BroadcastReceiver
    {
        private readonly IBroadcastReceiver _parent;

        public BroadcastListener(IBroadcastReceiver parent)
        {
            _parent = parent;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            _parent.OnBroadcastReceived(context, intent);

        }
    }

#endif
}