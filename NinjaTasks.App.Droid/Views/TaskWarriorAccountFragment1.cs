using Android.OS;
using Android.Views;
using Cirrious.MvvmCross.Droid.FullFragging.Fragments;
using NinjaTasks.Core.ViewModels;

namespace NinjaTasks.App.Droid.Views
{
    public class TaskWarriorAccountFragment1 : MvxFragment<TaskWarriorAccountViewModel>
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.TaskWarriorAccountView, container, false);
        }
    }
}