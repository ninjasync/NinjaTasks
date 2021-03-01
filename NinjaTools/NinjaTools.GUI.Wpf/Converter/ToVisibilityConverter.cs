using System.Windows;

namespace NinjaTools.GUI.Wpf.Converter
{
    public class ToVisibilityConverter : FromBooleanConverter
    {
        public new Visibility FalseValue
        {
            get { return (Visibility)base.FalseValue; }
            set { base.FalseValue = value; }
        }

        public new Visibility TrueValue
        {
            get { return (Visibility)base.TrueValue; }
            set { base.TrueValue = value; }
        }

        public ToVisibilityConverter()
        {
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }
    }
}