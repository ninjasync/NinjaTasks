using System;
using System.Globalization;
using System.Windows.Data;

namespace NinjaTools.GUI.Wpf.Converter
{
    public class DoubleToStringConverter : IValueConverter
    {
        public bool ZeroAsEmptyString { get; set; }
        public bool NotANumberAsEmptyString { get; set; }
        public double Multiplier { get; set; }
        public string StringFormat { get; set; }

        public DoubleToStringConverter()
        {
            Multiplier = 1;
            StringFormat = "F4"; // default 4 digits.
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val;
            if (value is double)
                val = (double)value;
            else
            { 
                try { val = System.Convert.ToDouble(value); }
                catch { return Binding.DoNothing; }
            }
            
            if (ZeroAsEmptyString && val == 0) return "";
            if (NotANumberAsEmptyString && double.IsNaN(val)) return "";

            if (Multiplier != 1)
                val *= Multiplier;
            
            return val.ToString(StringFormat, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value as string;
            if(NotANumberAsEmptyString && string.IsNullOrWhiteSpace(val)) 
                return double.NaN;
            if (/*ZeroAsEmptyString &&*/ string.IsNullOrWhiteSpace(val)) 
                return 0;

            double parse;
            if (!double.TryParse(val, out parse) && !double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out parse))
                return Binding.DoNothing;
            return parse / Multiplier;
        }
    }
}
