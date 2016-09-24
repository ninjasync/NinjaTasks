using System;
using Cirrious.CrossCore.Converters;

namespace NinjaTasks.App.Droid.MvvmCross.Converters
{
    public class ProgressConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)((float) value)*100;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}