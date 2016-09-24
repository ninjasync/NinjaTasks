using System;
using System.Globalization;
using Android.Views;
using Cirrious.CrossCore.Converters;

namespace NinjaTasks.App.Droid.Views.Converters
{
    public class TrueToVisibilityGoneConverter : MvxValueConverter<bool, int>
    {
        protected override int Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ? View.GONE : View.VISIBLE;
        }
    }
}
