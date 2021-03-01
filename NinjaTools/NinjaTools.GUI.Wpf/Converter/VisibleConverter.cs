using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NinjaTools.GUI.Wpf.Converter
{
    /// <summary>
    /// This will convert bools, ints and strings to Visibility.
    /// </summary>
    public class VisibleConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool notval = value == null
                           || (value is string && ((string)value) == "")
                           || (value is int && ((int) value) == 0)
                           || (value is long && ((long) value) == 0)
                           || (value is float && ((float) value) == 0)
                           || (value is double && ((double) value) == 0)
                           || (value is decimal && ((decimal) value) == 0)
                           || (value is bool && ((bool) value) == false);

            if (!Invert) notval = !notval;
            return notval ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
