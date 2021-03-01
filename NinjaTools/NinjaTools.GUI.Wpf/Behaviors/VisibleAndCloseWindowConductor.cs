using System;
using System.ComponentModel;
using System.Windows;
using NinjaTools.GUI.MVVM;

namespace NinjaTools.GUI.Wpf.Behaviors
{
    /// <summary>
    /// handles visible + closing redirection to the ViewModel, via IActivate + IDeactivate
    /// </summary>
    public class VisibleAndCloseWindowConductor : VisibleFrameworkElementConductor
    {
        private readonly Window _view;

        public VisibleAndCloseWindowConductor(Window view) : base(view)
        {
            _view = view;

            view.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var model = _view.DataContext;

            _view.Closed -= Closed;
            _view.Closing -= Closing;

            var deactivate = model as IDeactivate;
            if (deactivate != null)
            {
                _view.Closed += Closed;
                _view.Closing += Closing;
            }
        }

        private void Closed(object sender, EventArgs e)
        {
            _view.Closed -= Closed;
            _view.Closing -= Closing;

            var deactivatable = (IDeactivate)_view.DataContext;
            deactivatable.OnDeactivated(true);
        }

        private void Closing(object sender, CancelEventArgs e)
        {
            if (e.Cancel)
                return;

            var deactivatable = (IDeactivate)_view.DataContext;
            deactivatable.OnDeactivate();
        }
    }
}