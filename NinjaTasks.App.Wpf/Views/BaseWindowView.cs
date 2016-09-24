using System.Windows;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Wpf.Views;
using NinjaTasks.App.Wpf.MvvmCross;

namespace NinjaTasks.App.Wpf.Views
{
    public class BaseWindowView : Window, IMvxWpfView
    {
        private IMvxViewModel _viewModel;

        public BaseWindowView()
        {
            new VisibleAndCloseWindowConductor(this);
        }

        public IMvxViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                DataContext = value;
            }
        }
    }
}