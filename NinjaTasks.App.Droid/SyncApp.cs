using Android.Views;
using Cirrious.MvvmCross.ViewModels;
using NinjaTasks.Core.ViewModels.Sync;

namespace NinjaTasks.App.Droid
{
    public class SyncApp : MvxApplication
    {
        public override void Initialize()
        {
            NinjaTasks.Core.App.RegisterTypesWithIoC(CreatableTypes());
            this.RegisterAppStart<ConfigureAccountsViewModel>();
        }
    }
}
