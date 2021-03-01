using Android.Content;
using MvvmCross.Base;
using NinjaTasks.Model;
using NinjaTools.Droid.Services;

namespace NinjaTasks.App.Droid.Services
{
    public class AndroidNinjaTasksConfiguration : NinjaTasksConfiguration
    {
    }

    public class AndroidNinjaTasksConfigurationService : AndroidConfigurationService<AndroidNinjaTasksConfiguration>,
                                                         INinjaTasksConfigurationService
    {
        public AndroidNinjaTasksConfigurationService(Context ctx, IMvxJsonConverter json)
            : base(ctx, json)
        {
        }

        public new NinjaTasksConfiguration Cfg { get { return base.Cfg; } }
    }
}
