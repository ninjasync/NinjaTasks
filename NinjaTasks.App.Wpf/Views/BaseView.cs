using Cirrious.MvvmCross.Wpf.Views;
using NinjaTasks.App.Wpf.MvvmCross;

namespace NinjaTasks.App.Wpf.Views
{
    public abstract class BaseView : MvxWpfView
    {
        public BaseView()
        {
            // TODO: this is it probably not yet.
            new VisibleUserControlConductor(this);
        }
    }
}
