using System.Windows;
using System.Windows.Controls;
using NinjaTools.MVVM;

namespace NinjaTasks.App.Wpf.MvvmCross
{
    /// <summary>
    /// handles visible & closing redirection to the ViewModel, via IActivate & IDeactivate
    /// </summary>
    public class VisibleUserControlConductor
    {
        private readonly UserControl _view;

        public VisibleUserControlConductor(UserControl view)
        {
            _view = view;

            view.IsVisibleChanged += OnVisibileChanged;
            view.DataContextChanged += OnDataContextChanged;

            HandleIActivateDeactivate();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HandleIActivateDeactivate();
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
    }
}