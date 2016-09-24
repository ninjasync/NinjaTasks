using System;
using Android.Database;

namespace NinjaTools.Dot42.Android
{
    public static class CursorExtensions
    {
        public static int GetInt(this ICursor cursor, string columnName)
        {
            return cursor.GetInt(cursor.GetColumnIndexOrThrow(columnName));
        }

        public static string GetString(this ICursor cursor, string columnName)
        {
            return cursor.GetString(cursor.GetColumnIndexOrThrow(columnName));
        }

        public static long GetLong(this ICursor cursor, string columnName)
        {
            return cursor.GetLong(cursor.GetColumnIndexOrThrow(columnName));
        }

        public static DateTime GetDateTimeFromUnixMillies(this ICursor cursor, string columnName)
        {
            int idx = cursor.GetColumnIndexOrThrow(columnName);
            
            var unixTime = cursor.GetLong(idx);
            if (unixTime == 0) return default(DateTime);
            return FromMillisecondsUnixTimeToUtc(unixTime);
        }

        public static DateTime? GetDateTimeFromUnixMilliesNullable(this ICursor cursor, string columnName)
        {
            int idx = cursor.GetColumnIndexOrThrow(columnName);
            if (cursor.IsNull(idx)) return null;
            
            var unixTime = cursor.GetLong(idx);
            if (unixTime == 0) return default(DateTime);
            return FromMillisecondsUnixTimeToUtc(unixTime);
        }

        private static readonly DateTime UnixZeroUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromMillisecondsUnixTimeToUtc(this long milliseconds)
        {
            DateTime dateTime = UnixZeroUtc;
            dateTime = dateTime.AddMilliseconds((double)milliseconds);
            return dateTime;
        }
        public static long FromUtcToMillisecondsUnixTime(this DateTime dateTimeUtc)
        {
            return (long)(dateTimeUtc - UnixZeroUtc).TotalMilliseconds;
        }
    }
}