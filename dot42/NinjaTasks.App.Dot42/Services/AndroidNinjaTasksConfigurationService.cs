using Android.Content;
using Cirrious.CrossCore.Platform;
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
        public AndroidNinjaTasksConfigurationService(Context ctx, IMvxJsonConverter json, IMvxTrace trace)
            : base(ctx, json, trace)
        {
        }

        public new NinjaTasksConfiguration Cfg { get { return base.Cfg; } }
    }
}
