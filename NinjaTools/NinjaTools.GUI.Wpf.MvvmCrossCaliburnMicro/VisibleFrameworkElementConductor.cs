using System;
using System.Windows;
using NinjaTools.MVVM;

namespace NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro
{
    /// <summary>
    /// handles visible & closing redirection to the ViewModel, via IActivate & IDeactivate & IActivateByGuard
    /// </summary>
    public class VisibleFrameworkElementConductor
    {
        private readonly FrameworkElement _view;
        private bool _hadErrorsOnVisibleChanged;
        private GuardToken _visibilityToken;

        public VisibleFrameworkElementConductor(FrameworkElement view)
        {
            _view = view;

            view.IsVisibleChanged += OnVisibileChanged;
            view.DataContextChanged += OnDataContextChanged;

            HandleIActivateDeactivate();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HandleIActivateDeactivate();

            if (e.OldValue != null)
            {
                IDeactivate vm = e.OldValue as IDeactivate;
                vm?.OnDeactivated(false);
            }
        }

        private void OnVisibileChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnVisibleChanged();
        }

        private void OnVisibleChanged()
        {
            if (_hadErrorsOnVisibleChanged) // prevent endless activate/deactivate loops
                return;

            try
            {
                HandleIActivateDeactivate();
            }
            catch (Exception)
            {
                _hadErrorsOnVisibleChanged = true;
                throw;
            }
        }

        protected void HandleIActivateDeactivate()
        {
            if (_view.IsVisible)
            {
                var vmByGuard = _view.DataContext as IActivateByGuard;
                if (vmByGuard != null)
                {
                    var token = vmByGuard.IsActiveGuard.Use();
                    _visibilityToken?.Dispose();
                    _visibilityToken = token;
                }

                IActivate vm = _view.DataContext as IActivate;
                vm?.OnActivate();
            }
            else
            {
                _visibilityToken?.Dispose();
                _visibilityToken = null;

                IDeactivate vm = _view.DataContext as IDeactivate;
                vm?.OnDeactivated(false);
            }
        }
    }
}