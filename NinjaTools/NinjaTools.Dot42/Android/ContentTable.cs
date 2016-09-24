using System.Globalization;
using System.Linq;
using Android.Content;
using Android.Database;
using Android.Net;

namespace NinjaTools.Dot42.Android
{
    /// <summary>
    /// helps with common table operations. assumes that the Id column is an integer.
    /// </summary>
    public class ContentTable
    {
        private readonly ContentProviderClient _provider;
        private readonly bool _updateUsingWhere;

        public Uri Uri { get; private set; }
        public string ColId { get; private set; }

        public ContentTable(ContentProviderClient provider, Uri tableUri, string colId, bool updateUsingWhere)
        {
            _provider = provider;
            _updateUsingWhere = updateUsingWhere;
            Uri = tableUri;
            ColId = colId;
        }

        /// <summary>
        /// inserts if idOrZero is null, else updates the row at id.
        /// </summary>
        /// <param name="cv"></param>
        /// <param name="idOrZero"></param>
        /// <returns></returns>
        public long InsertOrUpdate(ContentValues cv, long idOrZero)
        {
            if (idOrZero == 0)
            {
                Uri inserted = _provider.Insert(Uri, cv);
                string insertId = inserted.PathSegments[1];
                return int.Parse(insertId, CultureInfo.InvariantCulture);
            }
            else
            {
                if (!_updateUsingWhere)
                {
                    var updateUri = GetUpdateUri(idOrZero);
                    _provider.Update(updateUri, cv, null, null);
                }
                else
                {
                    _provider.Update(Uri, cv, ColId + "=?",
                                    new[] { idOrZero.ToStringInvariant() });
                }
            }
            return idOrZero;
        }

        public Uri GetUpdateUri(long id)
        {
            return Uri.WithAppendedPath(Uri, id.ToStringInvariant());
        }

        public ICursor QueryByIds(string[] columns, params long[] ids)
        {
            return _provider.QueryByIds(Uri, columns, ColId, ids);
        }

        public ICursor QueryByIds(string[] columns, params string[] ids)
        {
            return _provider.QueryByIds(Uri, columns, ColId, ids);
        }

        public ICursor QueryAll(params string[] columns)
        {
            return _provider.Query(Uri, columns, null, null, null);
        }

        public void DeleteByIds(params long[] ids)
        {
            if (ids.Length == 0) return;
            if (ids.Length == 1)
                _provider.Delete(Uri, ColId + "=?", new[] {ids[0].ToStringInvariant()});
            else
            {
                string query = string.Format("{0} in ({1})", ColId, string.Join(",", ids.Select(i => i.ToStringInvariant())));
                _provider.Delete(Uri, query, null);
            }
        }

        public void DeleteByIds(params string[] ids)
        {
            if (ids.Length == 0) return;
            if (ids.Length == 1)
                _provider.Delete(Uri, ColId + "= ?", new[] { ids[0] });
            else
            {
                string query = string.Format("{0} in ({1})", ColId, string.Join(",", ids.Select(DatabaseUtils.SqlEscapeString)));
                _provider.Delete(Uri, query, null);
            }
        }

        public bool IsEmpty()
        {
            ICursor c = _provider.Query(Uri, new []{ColId}, null, null, null);
            bool hasValues = c.MoveToFirst();
            c.Close();
            return !hasValues;
        }
    }
}