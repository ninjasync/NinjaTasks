using System.Windows;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Wpf.Views;

namespace NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro
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