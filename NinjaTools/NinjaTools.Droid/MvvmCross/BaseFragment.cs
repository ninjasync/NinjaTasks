using AndroidX.Fragment.App;
using NinjaTools.GUI.MVVM;

namespace NinjaTools.Droid.MvvmCross
{
    public abstract class BaseFragment : Fragment
    {
        public object ViewModel { get; protected set; }

        public override void OnResume()
        {
            base.OnResume();

            var activate = ViewModel as IActivate;
            if(activate != null)
                activate.OnActivate();
        }

        public override void OnPause()
        {
            var deactivate = ViewModel as IDeactivate;
            if (deactivate != null)
                deactivate.OnDeactivate();
            base.OnPause();
        }

        public override void OnStop()
        {
            var deactivate = ViewModel as IDeactivate;
            if (deactivate != null)
                deactivate.OnDeactivated(false);
            base.OnStop();
        }

        public override void OnDestroy()
        {
            var deactivate = ViewModel as IDeactivate;
            if (deactivate != null)
                deactivate.OnDeactivated(true);
            
            base.OnDestroy();
        }
    }
}