using Cirrious.MvvmCross.Droid.Views;
using Dot42.Manifest;

namespace NinjaTasks.App.Droid
{

    [Activity(
        Label = "@string/app_name", 
        MainLauncher = true, 
        Icon = "@drawable/ic_launcher",
        Theme = "@style/ThemeSplash",
        NoHistory = true
        )]
    public class SplashScreenActivity : MvxSplashScreenActivity
    {
        public SplashScreenActivity()
            : base(R.Layout.SplashScreen)
        {
        }
    }
}

