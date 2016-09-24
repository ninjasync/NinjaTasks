using System;
using System.Linq;
using Android.Content;
using Android.Database;
using Uri = Android.Net.Uri;

namespace NinjaTools.Dot42.Android
{
    public static class ContentResolverExtensions
    {
        /// <summary>
        /// if ids is empty, returns all rows.
        /// </summary>
        public static ICursor QueryByIds(this ContentResolver resolver, Uri uri, string[] columns, string colId, long[] ids)
        {
            if (ids.Length == 0)
                return resolver.Query(uri, columns, null, null, null);

            if (ids.Length == 1)
                return resolver.Query(uri, columns, colId + "= ?", new[] { ids[0].ToStringInvariant() }, null);

            string query = string.Format("{0} in ({1})", colId, string.Join(",", ids.Select(i => i.ToStringInvariant())));
            return resolver.Query(uri, columns, query, null, null);
        }

        /// <summary>
        /// if ids is empty, returns all rows.
        /// </summary>
        public static ICursor QueryByIds(this ContentProviderClient provider, Uri uri, string[] columns, string colId, long[] ids)
        {
            if (ids.Length == 0)
                return provider.Query(uri, columns, null, null, null);

            if (ids.Length == 1)
                return provider.Query(uri, columns, colId + "= ?", new[] { ids[0].ToStringInvariant() }, null);

            string query = string.Format("{0} in ({1})", colId, string.Join(",", ids.Select(i => i.ToStringInvariant())));
            return provider.Query(uri, columns, query, null, null);
        }

        /// <summary>
        /// if ids is empty, returns all rows.
        /// </summary>
        public static ICursor QueryByIds(this ContentProviderClient provider, Uri uri, string[] columns, string colId, string[] ids)
        {
            if (ids.Length == 0)
                return provider.Query(uri, columns, null, null, null);

            if (ids.Length == 1)
                return provider.Query(uri, columns, colId + "= ?", new[] { ids[0]}, null);

            string query = string.Format("{0} in ({1})", colId, string.Join(",", ids.Select(DatabaseUtils.SqlEscapeString)));
            return provider.Query(uri, columns, query, null, null);
        }

        

    }
}