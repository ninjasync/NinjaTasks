using Cirrious.MvvmCross.Wpf.Views;

namespace NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro
{
    public abstract class BaseView : MvxWpfView
    {
        protected BaseView()
        {
            // TODO: this is it probably not yet.
            new VisibleFrameworkElementConductor(this);
        }
    }
}
