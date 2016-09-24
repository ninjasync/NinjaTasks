using System;
using System.Globalization;
using System.Windows.Data;

namespace NinjaTools.GUI.Wpf.Converter
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanNotConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (targetType == typeof(bool))
            {
                if (value == null) return false;
                return !(bool)value;
            }
            if (targetType == typeof(bool?))
            {
                if (value == null) return null;
                return !(bool)value;
            }
            if (targetType == typeof(object))
            {
                if (value == null) return false;
                return !(bool)value;
            }

            throw new InvalidOperationException("The target must be a boolean");

        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }

        #endregion
    }
}