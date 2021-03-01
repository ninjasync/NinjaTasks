using System;
using System.Globalization;
using Android.Views;
using MvvmCross.Converters;

namespace NinjaTasks.App.Droid.Views.Converters
{
    public class FalseToVisibilityGoneConverter : MvxValueConverter<bool, int>
    {
        protected override int Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(value ? ViewStates.Visible : ViewStates.Gone);
        }
    }
}
