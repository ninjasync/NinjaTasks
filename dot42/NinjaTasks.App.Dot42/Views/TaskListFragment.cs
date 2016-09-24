using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

namespace NinjaTasks.App.Droid.Views
{
    public class TaskListFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(R.Layout.TaskList, container, false);
        }
    }

}
