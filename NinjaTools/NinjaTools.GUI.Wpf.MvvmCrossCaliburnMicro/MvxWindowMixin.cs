using MvvmCross;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using System;
using System.Windows;

namespace NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro
{
    public class MvxWindowMixin : IMvxWindow, IMvxWpfView, IDisposable
    {
        private readonly Window _window;
        private IMvxViewModel _viewModel;
        private IMvxBindingContext _bindingContext;
        private bool _unloaded = false;

        public string Identifier { get; set; }

        public MvxWindowMixin(Window window)
        {
            _window = window;
            _window.Closed += MvxWindow_Closed;
            _window.Unloaded += MvxWindow_Unloaded;
            _window.Loaded += MvxWindow_Loaded;
            _window.Initialized += MvxWindow_Initialized;
        }

        public IMvxViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;
                DataContext = value;
                BindingContext.DataContext = value;
            }
        }

        public IMvxBindingContext BindingContext
        {
            get
            {
                if (_bindingContext != null)
                    return _bindingContext;

                if (Mvx.IoCProvider != null)
                    this.CreateBindingContext();

                return _bindingContext;
            }
            set => _bindingContext = value;
        }

        public object DataContext { get => _window.DataContext; set => _window.DataContext = value; }

        private void MvxWindow_Initialized(object sender, EventArgs e)
        {
            if (_window == Application.Current.MainWindow)
            {
                (Application.Current as MvvmCross.Platforms.Wpf.Views.MvxApplication).ApplicationInitialized();
            }
        }

        private void MvxWindow_Closed(object sender, EventArgs e) => Unload();

        private void MvxWindow_Unloaded(object sender, RoutedEventArgs e) => Unload();

        private void MvxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel?.ViewAppearing();
            ViewModel?.ViewAppeared();
        }

        private void Unload()
        {
            if (!_unloaded)
            {
                ViewModel?.ViewDisappearing();
                ViewModel?.ViewDisappeared();
                ViewModel?.ViewDestroy();
                _unloaded = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MvxWindowMixin()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _window.Unloaded -= MvxWindow_Unloaded;
                _window.Loaded -= MvxWindow_Loaded;
                _window.Closed -= MvxWindow_Closed;
                _window.Initialized -= MvxWindow_Initialized;
            }
        }
    }
}
