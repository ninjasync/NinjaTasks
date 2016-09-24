// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the App type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Wpf.Views;
using NinjaTasks.App.Wpf.MvvmCross;
using NinjaTools.GUI.Wpf;

namespace NinjaTasks.App.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// Setup complete indicator.
        /// </summary>
        private bool setupComplete;

        public App()
        {
        }
        /// <summary>
        /// Does the setup.
        /// </summary>
        private void DoSetup()
        {
            UnhandledExceptionHandlers.Initialize(this);

            IMvxWpfViewPresenter presenter = new MyWpfPresenter(MainWindow);

            Setup setup = new Setup(Dispatcher, presenter);
            setup.Initialize();

            IMvxAppStart start = Mvx.Resolve<IMvxAppStart>();
            start.Start();

            this.setupComplete = true;

            

        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Activated" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnActivated(EventArgs e)
        {
            if (!this.setupComplete)
            {
                try
                {
                    DoSetup();
                }
                catch (Exception ex)
                {
                    if (!Debugger.IsAttached)
                    {
                        MessageBox.Show(ex.Message, "Startup Error");
                        Shutdown(3);
                    }
                    else
                        throw;
                }
            }

            base.OnActivated(e);
        }
    }
}
