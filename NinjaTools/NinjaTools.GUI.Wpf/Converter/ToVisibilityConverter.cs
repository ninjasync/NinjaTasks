using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NinjaTools.GUI.Wpf.Converter
{
    public class ToVisibilityConverter : IValueConverter
    {
        public Visibility FalseValue { get; set; }
        public Visibility TrueValue { get; set; }

        public ToVisibilityConverter()
        {
            FalseValue = Visibility.Collapsed;
            TrueValue  = Visibility.Visible;

        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = false;

            if (value is bool?)
            {
                isTrue = (bool?)value == true;
            }
            else if (value is string)
            {
                isTrue = !string.IsNullOrWhiteSpace((string)value);
            }
            else
            {
                isTrue = value != null;
            }

            return isTrue ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}