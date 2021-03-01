using System.Windows;
using Caliburn.Micro;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;

namespace NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro
{
    public class MvxToCaliburnMicroWpfViewsContainer : MvxWpfViewsContainer
    {
        public override FrameworkElement CreateView(MvxViewModelRequest request)
        {
            var ret = base.CreateView(request);
            var view = ret as IMvxWpfView;

            if (view != null)
                ViewModelBinder.Bind(view.ViewModel, ret, null);
            return ret;
        }
    }
}
