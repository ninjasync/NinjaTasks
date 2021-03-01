using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Platforms.Android.Binding;

namespace NinjaTasks.App.Droid.MvvmCross
{
    class MyBindingBuilder : MvxAndroidBindingBuilder
    {
        protected override IMvxTargetBindingFactoryRegistry CreateTargetBindingRegistry()
        {
            return new MyTargetBindingFactoryRegistry();
        }
    }
}