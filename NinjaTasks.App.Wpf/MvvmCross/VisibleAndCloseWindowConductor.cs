using System;
using System.ComponentModel;
using System.Windows;
using NinjaTools.MVVM;

namespace NinjaTasks.App.Wpf.MvvmCross
{
    /// <summary>
    /// handles visible & closing redirection to the ViewModel, via IActivate & IDeactivate
    /// </summary>
    public class VisibleAndCloseWindowConductor
    {
        private readonly Window _view;

        public VisibleAndCloseWindowConductor(Window view)
        {
            _view = view;

            view.IsVisibleChanged += OnVisibileChanged;
            view.DataContextChanged += OnDataContextChanged;

            HandleIActivateDeactivate();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var model = _view.DataContext;
            
            HandleIActivateDeactivate();

            _view.Closed -= Closed;
            _view.Closing -= Closing;

            var deactivate = model as IDeactivate;
            if (deactivate != null)
            {
                _view.Closed += Closed;
                _view.Closing += Closing;
            }
        }

        private void OnVisibileChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HandleIActivateDeactivate();
        }

        private void HandleIActivateDeactivate()
        {
            if (_view.IsVisible)
            {
                IActivate vm = _view.DataContext as IActivate;
                if (vm != null) vm.OnActivate();
            }
            else
            {
                IDeactivate vm = _view.DataContext as IDeactivate;
                if (vm != null) vm.OnDeactivated(false);
            }
        }

        void Closed(object sender, EventArgs e)
        {
            _view.Closed -= Closed;
            _view.Closing -= Closing;

            var deactivatable = (IDeactivate)_view.DataContext;
            deactivatable.OnDeactivated(true);
        }

        void Closing(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;

            var deactivatable = (IDeactivate)_view.DataContext;
            deactivatable.OnDeactivate();
        }
    }
}