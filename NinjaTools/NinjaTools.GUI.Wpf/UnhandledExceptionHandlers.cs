using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NinjaTools.Logging;

namespace NinjaTools.GUI.Wpf
{
    /// <summary>
    /// to use, include in your project and call initialize.
    /// </summary>
    internal class UnhandledExceptionHandlers
    {
        private static UnhandledExceptionHandlers _instance;
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly Application _app;

        public UnhandledExceptionHandlers(Application app)
        {
            _app = app;
            //if (Application.Current == null)
            //    app.Startup += InstallUnhandledExceptionHandlers;
            //else
            //    InstallUnhandledExceptionHandlers(Application.Current, null);
            InstallUnhandledExceptionHandlers(app, null);
        }

        public static void Initialize(Application app)
        {
            if(_instance == null)
                _instance = new UnhandledExceptionHandlers(app);
        }

        private void InstallUnhandledExceptionHandlers(object sender, StartupEventArgs e)
        {
            // Global exception handling  
            Application.Current.DispatcherUnhandledException += AppDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedException;
            
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

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is OperationCanceledException)
            {
                e.Handled = true;
                return;
            }

            LogToError("AppDispatcher unhandled exception:", e.Exception);

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                e.Handled = true;
                ShowUnhandledException(e.Exception);
            }
        }

        private void LogToError(string msg, Exception exception)
        {
            Log.Error(msg, exception);
        }

        private void ShowUnhandledException(Exception ex)
        {
            if (_app == null) return;

            string errorMessage = string.Format("{0}",
                                                ex.Message +
                                                (ex.InnerException != null
                                                     ? "\n" + ex.InnerException.Message
                                                     : ""));
            
            if (string.IsNullOrWhiteSpace(errorMessage))
                errorMessage = "An unhandled exception has occured.";


            try
            {
                if (_app.MainWindow == null)
                {
                    // FIX for SplashScreen: http://stackoverflow.com/questions/576503/how-to-set-wpf-messagebox-owner-to-desktop-window-because-splashscreen-closes-me/5328590#5328590
                    SetActiveWindow(IntPtr.Zero);

                }
            }
            catch (Exception) { }


            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            
            if (_app.MainWindow == null)
                _app.Shutdown();  // nothing can be done at this stage...
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);
    }
}
