using Dot42.Manifest;
using NinjaTasks.App.Droid;
using NinjaTasks.App.Droid.Resources3rdParty;
using NinjaTools.Droid;
using NinjaTools.Droid.Services;

namespace NinjaTasks.App.Dot42
{
    [Application("@string/app_name"
                ,Icon = "@drawable/ic_launcher"
                ,Theme = "@style/ThemeNinjaTasks"
                ,HardwareAccelerated = true
                //,Debuggable = true
                )]

    public class MyApplication : ApplicationEx
    {
        public MyApplication()
        {
            PullToRefreshResInit.InitResources();
            DisplayMessageService.ErrorIconResourceId = R.Drawable.ic_error_outline_black_36dp;
        }

        //public override void OnCreate()
        //{
        //    base.OnCreate();

        //    try
        //    {
        //        new Tests.TestNpcBinding().RunTests();

        //        var storage = new AndroidSqliteTrackableStorage(ApplicationContext, "");
        //        if (storage != null)
        //        {

        //        }
        //    }
        //    catch (IncompatibleClassChangeError ex)
        //    {
        //        Log.E("ninja", ex.Message, ex);
        //        return;
                
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.E("ninja", ex.Message, ex);
        //        return;
        //    }
            
        //}
    }
}
