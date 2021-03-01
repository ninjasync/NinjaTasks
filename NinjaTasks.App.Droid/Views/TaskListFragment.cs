using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using R = NinjaTasks.App.Droid.Resource;

namespace NinjaTasks.App.Droid.Views
{
    [Android.Runtime.Register("ninjatasks.app.droid.views.TaskListFragment")]
    public class TaskListFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(R.Layout.TaskList, container, false);
        }
    }

}
