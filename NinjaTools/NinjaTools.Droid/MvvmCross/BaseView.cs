using Android.OS;
using MvvmCross.Platforms.Android.Views;
using NinjaTools.GUI.MVVM;

namespace NinjaTools.Droid.MvvmCross
{
    public enum LifecycleState
    {
        None,
        Created,
        Started,
        Resumed,
        Paused,
        Stopped,
        Destroyed,
    }

    public abstract class BaseView : MvxActivity
    {
        public LifecycleState LifecycleState { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            LifecycleState = LifecycleState.Created;
        }

        protected override void OnStart()
        {
            LifecycleState = LifecycleState.Started;
            base.OnStart();
        }

        protected override void OnResume()
        {
            base.OnResume();

            LifecycleState = LifecycleState.Resumed;
            if(ViewModel is IActivate activate)
                activate.OnActivate();
        }

        protected override void OnPause()
        {
            if (ViewModel is IDeactivate deactivate)
                deactivate.OnDeactivate();
            base.OnPause();
            LifecycleState = LifecycleState.Paused;
        }

        protected override void OnStop()
        {
            if (ViewModel is IDeactivate deactivate)
                deactivate.OnDeactivated(false);
            base.OnStop();
            LifecycleState = LifecycleState.Stopped;
        }

        protected override void OnDestroy()
        {
            if (ViewModel is IDeactivate deactivate)
                deactivate.OnDeactivated(true);
            
            base.OnDestroy();
            LifecycleState = LifecycleState.Destroyed;
        }
    }
}