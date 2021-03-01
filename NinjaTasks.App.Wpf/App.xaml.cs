using MvvmCross.Core;
using MvvmCross.Platforms.Wpf.Views;

namespace NinjaTasks.App.Wpf
{
    public partial class App : MvxApplication
    {
        public App()
        {
            
        }

        protected override void RegisterSetup()
        {
            this.RegisterSetupType<Setup>();
        }

        ///// <summary>
        ///// Does the setup.
        ///// </summary>
        //private void DoSetup()
        //{
        //    UnhandledExceptionHandlers.Initialize(this);

        //    IMvxWpfViewPresenter presenter = new MyWpfPresenter(MainWindow);

        //    Setup setup = new Setup(Dispatcher, presenter);
        //    setup.Initialize();

        //    IMvxAppStart start = Mvx.IoCProvider.Resolve<IMvxAppStart>();
        //    start.Start();

        //    this.setupComplete = true;



        //}

        ///// <summary>
        ///// Raises the <see cref="E:System.Windows.Application.Activated" /> event.
        ///// </summary>
        ///// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        //protected override void OnActivated(EventArgs e)
        //{
        //    if (!this.setupComplete)
        //    {
        //        try
        //        {
        //            DoSetup();
        //        }
        //        catch (Exception ex)
        //        {
        //            if (!Debugger.IsAttached)
        //            {
        //                MessageBox.Show(ex.Message, "Startup Error");
        //                Shutdown(3);
        //            }
        //            else
        //                throw;
        //        }
        //    }

        //    base.OnActivated(e);
        //}
    }
}
