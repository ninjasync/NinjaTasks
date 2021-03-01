using System;
using System.Globalization;
using Android.Graphics;
using NinjaTools.Logging;
using MvvmCross.Converters;

namespace NinjaTasks.App.Droid.MvvmCross.Converters
{
    public class LogLevelToBackgroundColorConverter : MvxValueConverter<LogLevel,Color>
    {
        protected override Color Convert(LogLevel value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value <= LogLevel.Warn) return Color.White;
            if (value == LogLevel.Warn) return Color.Yellow;
            return Color.Firebrick;
        }
    }
    public class LogLevelToTextColorConverter : MvxValueConverter<LogLevel, Color>
    {
        protected override Color Convert(LogLevel value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value <= LogLevel.Debug) return Color.DarkGray;
            if (value == LogLevel.Error) return Color.White;
            return Color.Black;
        }
    }
}
