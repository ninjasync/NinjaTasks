using System;
using System.ComponentModel;
using System.Windows;
using CustomChromeLibrary;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using NinjaTasks.App.Wpf.Properties;
using NinjaTools.GUI.Wpf;
using NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro;

namespace NinjaTasks.App.Wpf
{
    public partial class MainWindow : CustomChromeWindow, IMvxWindow, IMvxWpfView, IDisposable
    {
        private MvxWindowMixin _mixin;

        public MainWindow()
        {
            _mixin = new MvxWindowMixin(this);

            InitializeComponent();

            Closing += Window_Closing;
        }

        public string Identifier { get => _mixin.Identifier; set => _mixin.Identifier = value; }
        public IMvxViewModel ViewModel { get => _mixin.ViewModel; set => _mixin.ViewModel = value; }
        public IMvxBindingContext BindingContext { get => _mixin.BindingContext; set => _mixin.BindingContext = value; }

        public void Dispose()
        {
            _mixin.Dispose();
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
