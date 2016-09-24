using System;
using System.Globalization;
using System.Windows.Data;

namespace NinjaTools.GUI.Wpf.Converter
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class ToBooleanConverter : IValueConverter
    {
        public bool NonEmptyStringIsTrue { get; set; }

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (value == null)
                return false;

            if (NonEmptyStringIsTrue)
            {
                return !string.IsNullOrEmpty(value.ToString());
            }

            try
            {
                return global::System.Convert.ToBoolean(value);
            }
            catch (Exception)
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}