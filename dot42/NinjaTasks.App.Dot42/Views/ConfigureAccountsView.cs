using Android.OS;
using NinjaTools.Droid.MvvmCross;

using Dot42.Manifest;

namespace NinjaTasks.App.Droid.Views
{
    [Activity(Label = "@string/app_name", Icon="@drawable/ic_launcher", 
              Exported = true, VisibleInLauncher = false)]
    public class ConfigureAccountsView : BaseView
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(R.Layout.ConfigureAccounts);

            //SetContentView(R.Layout.);
            //FindViewById<Button>(Resource.Id.MyButton).Click += OnClickButton;
            //var presenter = (DroidPresenter)Mvx.Resolve<IMvxAndroidViewPresenter>();

            ////var vm = Mvx.IocConstruct<TaskWarriorAccountViewModel>();
            //var initialFragment = new TaskWarriorAccountFragment { ViewModel = (TaskWarriorAccountViewModel)ViewModel };
            //presenter.RegisterFragmentManager(FragmentManager, initialFragment);
           // AutoCompleteTextView v;
           // v.ItemClick += OnItemclick;
           //Android.App.Application.SynchronizationContext
        }

     
    }

   
}