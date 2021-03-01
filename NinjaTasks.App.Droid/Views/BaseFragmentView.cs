using Android.OS;
using NinjaTools.Droid.MvvmCross;
using NinjaTools.GUI.MVVM;
using AndroidX.Fragment.App;
using MvvmCross.Platforms.Android.Views;

namespace NinjaTasks.App.Droid.Views
{
    public class BaseFragmentView : MvxActivity
    {
        public FragmentManager ActiveFragmentManager => SupportFragmentManager;

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
            var activate = ViewModel as IActivate;
            if (activate != null)
                activate.OnActivate();
        }

        protected override void OnPause()
        {
            var deactivate = ViewModel as IDeactivate;
            if (deactivate != null)
                deactivate.OnDeactivate();
            base.OnPause();
            LifecycleState = LifecycleState.Paused;
        }

        protected override void OnStop()
        {
            var deactivate = ViewModel as IDeactivate;
            if (deactivate != null)
                deactivate.OnDeactivated(false);
            base.OnStop();
            LifecycleState = LifecycleState.Stopped;
        }

        protected override void OnDestroy()
        {
            var deactivate = ViewModel as IDeactivate;
            if (deactivate != null)
                deactivate.OnDeactivated(true);

            base.OnDestroy();
            LifecycleState = LifecycleState.Destroyed;
        }
    }
}
