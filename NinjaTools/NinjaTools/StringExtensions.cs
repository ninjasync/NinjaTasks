using System;
using System.Globalization;

namespace NinjaTools
{
    public static class StringExtensions
    {
        /// <summary>
        /// I deem this to be quite practical, even though apperently
        /// allowing to   call a method of a null value probalby 
        /// violates some style rules.
        /// </summary>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// I deem this to be quite practical, even though apperently
        /// allowing to   call a method of a null value probalby 
        /// violates some style rules.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static string ToStringInvariant(this int v)
        {
            return v.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToStringInvariant(this long v)
        {
            return v.ToString(CultureInfo.InvariantCulture);
        }

    }
}
