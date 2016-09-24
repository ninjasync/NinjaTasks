using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace NinjaTools.GUI.Wpf.Converter
{
    [ValueConversion(typeof(Enum), typeof(IEnumerable<KeyValuePair<Enum, string>>))]
    public class EnumToCollectionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetAllValuesAndDescriptions(value.GetType());
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public static string Description(Enum eValue)
        {
            var nAttributes = eValue.GetType().GetField(eValue.ToString())
                                    .GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (nAttributes.Any())
                return ((DescriptionAttribute) nAttributes.First()).Description;

            // If no description is found, the least we can do is replace underscores with spaces
            TextInfo oTI = CultureInfo.CurrentCulture.TextInfo;
            return oTI.ToTitleCase(oTI.ToLower(eValue.ToString().Replace("_", " ")));
        }

        public static IEnumerable<KeyValuePair<Enum, string>> GetAllValuesAndDescriptions<T>() where T : struct, IConvertible, IComparable, IFormattable
        {
            return GetAllValuesAndDescriptions(typeof(T));
        }
        public static IEnumerable<KeyValuePair<Enum, string>> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException("T must be an enum type");

            return Enum.GetValues(t)
                       .Cast<Enum>()
                       .Select((e) => new KeyValuePair<Enum, string>(e, Description(e))).ToList();
        }

    }
}
