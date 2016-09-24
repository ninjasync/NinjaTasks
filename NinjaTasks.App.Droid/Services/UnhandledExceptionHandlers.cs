using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Java.Lang;
using Exception = System.Exception;

namespace NinjaTasks.App.Droid.Services
{
    /// <summary>
    /// to use, include in your project and call initialize.
    /// </summary>
    internal class UnhandledExceptionHandlers
    {
        private readonly Context _ctx;
        private static UnhandledExceptionHandlers _instance;


        public UnhandledExceptionHandlers(Context ctx)
        {
            _ctx = ctx;
            //if (Application.Current == null)
            //    app.Startup += InstallUnhandledExceptionHandlers;
            //else
            //    InstallUnhandledExceptionHandlers(Application.Current, null);
            InstallUnhandledExceptionHandlers();
        }

        public static void Initialize(Context app)
        {
            if(_instance == null)
                _instance = new UnhandledExceptionHandlers(app);
        }

        private void InstallUnhandledExceptionHandlers()
        {
            // Global exception handling  
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedException;
            //Test2
            AndroidEnvironment.UnhandledExceptionRaiser += HandleAndroidException;

            //Java.Lang.Thread.DefaultUncaughtExceptionHandler  = new ExceptHandler();

        }

        private void HandleAndroidException(object sender, RaiseThrowableEventArgs e)
        {
            e.Handled = true;

            var ex = e.Exception;
            LogToError("Android unhandled exception:", ex);

            if (ex != null)
                ShowUnhandledException(ex);

            
        }

        private void TaskSchedulerUnobservedException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // don't show.
            LogToError("TaskScheduler unhandled exception:", e.Exception);
            
        }

        private void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            LogToError("AppDomain unhandled exception:", ex);
            
            if (ex != null)
                ShowUnhandledException(ex);
        }

        //private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        //{
        //    if (e.Exception is OperationCanceledException)
        //    {
        //        e.Handled = true;
        //        return;
        //    }

        //    LogToError("AppDispatcher unhandled exception:", e.Exception);

        //    if (!System.Diagnostics.Debugger.IsAttached)
        //    {
        //        e.Handled = true;
        //        ShowUnhandledException(e.Exception);
        //    }
        //}

        private void LogToError(string msg, Exception exception)
        {
            Log.Error("ninja", msg, exception);
        }

        private void ShowUnhandledException(Exception ex)
        {
            string msg = "";

            if (!string.IsNullOrEmpty(msg))
                msg = "";
            else
                msg += " ";

            var throwable =ex as Java.Lang.Throwable;
            if (throwable != null)
                msg += throwable.LocalizedMessage;
            else
                msg +=  string.Format("{0}",ex.Message +
                                                (ex.InnerException != null
                                                     ? "\n" + ex.InnerException.Message
                                                     : ""));

            if (string.IsNullOrWhiteSpace(msg))
                msg = "An unhandled exception has occured.";

            Toast.MakeText(_ctx, msg, ToastLength.Long).Show();
        }
    }

    //internal class ExceptHandler : Java.Lang.Object, Thread.IUncaughtExceptionHandler
    //{
    //    public void UncaughtException(Thread thread, Throwable ex)
    //    {
    //        ex.
    //    }
    //}
}
