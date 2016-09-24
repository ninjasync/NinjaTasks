﻿using Android.App;
using Cirrious.MvvmCross.Droid.Views;

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
        public SplashScreenActivity() : base(Resource.Layout.SplashScreen)
        {
            
        }
    }
}

