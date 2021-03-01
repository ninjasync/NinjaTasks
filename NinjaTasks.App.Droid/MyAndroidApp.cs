using System;
using Android.App;
using Android.Runtime;
using NinjaTools.Droid;

namespace NinjaTasks.App.Droid
{
    [Application(/*Debuggable = true,  // TODO: remove for shipment*/
                 Icon="@drawable/ic_launcher",
                 Theme="@style/ThemeNinjaTasks",
                 Label="@string/app_name"
                 /*,HardwareAccelerated = true*/)]
    public class MyApplication : ApplicationEx
    {
        public MyApplication(IntPtr handle, JniHandleOwnership owner)
            : base(handle, owner)
        {
        }
    }
}