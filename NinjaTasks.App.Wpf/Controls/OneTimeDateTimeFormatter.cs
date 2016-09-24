using System;
using System.Globalization;
using System.Windows.Data;

namespace NinjaTasks.App.Wpf.Controls
{
    public class OneTimeDateTimeFormatter : IValueConverter
    {
        public bool WithNewLine { get; set; }

        public OneTimeDateTimeFormatter()
        {
            WithNewLine = true;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = ((DateTime) value);
            if (dateTime == DateTime.MinValue)
                return "(never)";

            DateTime localTime = dateTime.ToLocalTime();

            return WithNewLine
                ? localTime.ToString("yyyy-MM-dd\nHH:mm \\h")
                : localTime.ToString("yyyy-MM-dd HH:mm \\h");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
