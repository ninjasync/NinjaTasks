using System;
using System.Globalization;
using System.Windows.Data;

namespace NinjaTools.GUI.Wpf.Converter
{
    public class FromBooleanConverter : IValueConverter
    {
        public object FalseValue { get; set; }
        public object TrueValue { get; set; }

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
            if (Equals(value, TrueValue))
                return true;
            if (Equals(value, FalseValue))
                return false;
            return null;
        }
    }
}
