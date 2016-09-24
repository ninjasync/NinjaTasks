using Cirrious.CrossCore.Plugins;

namespace NinjaTasks.App.Droid.MvvmCross
{
    public class SqlitePluginBootstrap
        : MvxPluginBootstrapAction<Cirrious.MvvmCross.Community.Plugins.Sqlite.PluginLoader>
    {
    }

    public class MethodBindingPluginBootstrap
       : MvxPluginBootstrapAction<Cirrious.MvvmCross.Plugins.MethodBinding.PluginLoader>
    {
    }

    public class MessengerPluginBootstrap
     : MvxPluginBootstrapAction<Cirrious.MvvmCross.Plugins.Messenger.PluginLoader>
    {
    }
}