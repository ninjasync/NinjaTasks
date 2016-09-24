// based on: http://stackoverflow.com/questions/3204883/wpf-imagesource-binding-with-custom-converter

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Markup;

namespace NinjaTasks.App.Wpf.Controls
{
    /// <summary>
    /// A converter that accepts <see cref="SwitchConverterCase"/>s and converts them to the 
    /// Then property of the case.
    /// </summary>
    [ContentProperty("Cases")]
    public class SwitchConverter : IValueConverter
    {
        // Converter instances.
        List<SwitchConverterCase> _cases;

        #region Public Properties.
        /// <summary>
        /// Gets or sets an array of <see cref="SwitchConverterCase"/>s that this converter can use to produde values from.
        /// </summary>
        public List<SwitchConverterCase> Cases { get { return _cases; } set { _cases = value; } }
        #endregion
        #region Construction.
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchConverter"/> class.
        /// </summary>
        public SwitchConverter()
        {
            // Create the cases array.
            _cases = new List<SwitchConverterCase>();
        }
        #endregion

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // This will be the results of the operation.
            object results = null;

            //// I'm only willing to convert SwitchConverterCases in this converter and no nulls!
            //if (value == null) throw new ArgumentNullException("value");

            // I need to find out if the case that matches this value actually exists in this converters cases collection.
            if (_cases != null && _cases.Count > 0)
                for (int i = 0; i < _cases.Count; i++)
                {
                    // Get a reference to this case.
                    SwitchConverterCase targetCase = _cases[i];

                    // Check to see if the value is the cases When parameter.
                    if (targetCase.IsMatch(value))
                    {
                        // We've got what we want, the results can now be set to the Then property
                        // of the case we're on.
                        results = targetCase.Then;

                        // All done, get out of the loop.
                        break;
                    }
                }

            // return the results.
            return results;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a case for a switch converter.
    /// </summary>
    [ContentProperty("Then")]
    public class SwitchConverterCase
    {
        /// <summary>
        /// Gets or sets the condition of the case. If null, matches everything (=default; should be the last)
        /// </summary>
        public string When { get; set; }

        /// <summary>
        /// Gets or sets the results of this case when run through a <see cref="SwitchConverter"/>
        /// </summary>
        public object Then { get; set; }

        public bool IsRegex { get; set; }

        /// <summary>
        /// Switches the converter.
        /// </summary>
        public SwitchConverterCase()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchConverterCase"/> class.
        /// </summary>
        /// <param name="when">The condition of the case.</param>
        /// <param name="then">The results of this case when run through a <see cref="SwitchConverter"/>.</param>
        public SwitchConverterCase(string when, object then)
        {
            // Hook up the instances.
            this.Then = then;
            this.When = when;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("When={0}; Then={1}", When, Then);
        }

        public virtual bool IsMatch(object value)
        {
            if (When == null) return true;
            if (value == null) return false;

            if(!IsRegex)
                return value.ToString().ToUpper() == When.ToUpper();
            return Regex.IsMatch(value.ToString(), When);

        }
    }
}