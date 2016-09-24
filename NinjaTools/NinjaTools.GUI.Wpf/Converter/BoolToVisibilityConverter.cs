using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NinjaTools.GUI.Wpf.Converter
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = false;
            try { val = System.Convert.ToBoolean(value); }
            catch (Exception)
            {}
            
            if (Invert) val = !val;
            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
