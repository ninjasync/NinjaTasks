using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Util;
using Android.Widget;
using Java.Lang;
using Exception = System.Exception;

namespace NinjaTasks.App.Droid.Services
{
    /// <summary>
    /// to use, include in your project and call initialize.
    /// </summary>
    internal class UnhandledExceptionHandlers : Thread.IUncaughtExceptionHandler
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
            //AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedException;
            //Test2
            Thread.DefaultUncaughtExceptionHandler = this;
        }

        private void TaskSchedulerUnobservedException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // don't show.
            LogToError("TaskScheduler unhandled exception:", e.Exception);
        }

        public void UncaughtException(Thread thread, Exception exception)
        {
            LogToError("Thread Uncaugh Exception:", exception);
        }

        private void LogToError(string msg, Exception exception)
        {
            Log.E("ninja", msg, exception);
        }

        private void ShowUnhandledException(Exception ex)
        {
            string msg = "";

            if (!string.IsNullOrEmpty(msg))
                msg = "";
            else
                msg += " ";

             msg +=  string.Format("{0}",ex.Message +
                                        (ex.InnerException != null
                                        ? "\n" + ex.InnerException.Message
                                        : ""));

            if (string.IsNullOrWhiteSpace(msg))
                msg = "An unhandled exception has occured.";

            Toast.MakeText(_ctx, msg, Toast.LENGTH_LONG).Show();
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
