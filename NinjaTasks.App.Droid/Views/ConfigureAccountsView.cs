using Android.App;
using Android.OS;
using NinjaTools.Droid.MvvmCross;

namespace NinjaTasks.App.Droid.Views
{
    [Activity(Label = "@string/app_name", Icon="@drawable/ic_launcher", Exported = true)]
    public class ConfigureAccountsView : BaseView
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.ConfigureAccounts);

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