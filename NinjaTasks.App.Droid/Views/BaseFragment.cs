using NinjaTasks.App.Droid.Views.Utils;
using NinjaTools.Droid.MvvmCross;
using MvvmCross.ViewModels;
using MvvmCross.Platforms.Android.Views.Fragments;

namespace NinjaTasks.App.Droid.Views
{
    public class BaseFragment : MvxFragment
    {
        private readonly LifecycleToViewModelActivation _lifecycle = new LifecycleToViewModelActivation();

        public override void OnViewModelSet()
        {
            base.OnViewModelSet();
            _lifecycle.SetDataContext(ViewModel);
        }

        public override void OnResume()
        {
            base.OnResume();
            _lifecycle.SetLifecycle(LifecycleState.Resumed);
        }

        public override void OnPause()
        {
            _lifecycle.SetLifecycle(LifecycleState.Paused);
            base.OnPause();
        }

        public override void OnStop()
        {
            _lifecycle.SetLifecycle(LifecycleState.Stopped);
            base.OnStop();
        }

        public override void OnDestroy()
        {
            _lifecycle.SetLifecycle(LifecycleState.Destroyed);
            base.OnDestroy();
        }
    }

    public class BaseFragment<TViewModel> : BaseFragment where TViewModel : class, IMvxViewModel
    {
        public new TViewModel ViewModel
        {
            get { return (TViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }
    }


}
