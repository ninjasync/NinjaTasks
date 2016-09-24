using System;
using System.ComponentModel;
using NinjaTasks.App.Wpf.Properties;
using NinjaTools.GUI.Wpf;

namespace NinjaTasks.App.Wpf
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Closing += Window_Closing;
        }

        protected override void OnSourceInitialized(EventArgs eventArgs)
        {
            base.OnSourceInitialized(eventArgs);
            this.SetPlacement(Settings.Default.MainWindowPlacement);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.MainWindowPlacement = this.GetPlacement();
            Settings.Default.Save();
        }
    }
}
