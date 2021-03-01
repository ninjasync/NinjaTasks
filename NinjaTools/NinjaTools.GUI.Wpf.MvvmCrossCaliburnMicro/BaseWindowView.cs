using System.Windows;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using NinjaTools.GUI.Wpf.Behaviors;

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

        public IMvxBindingContext BindingContext { get; set; }
    }
}