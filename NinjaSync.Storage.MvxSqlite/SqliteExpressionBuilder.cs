using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NinjaTools;

namespace NinjaSync.Storage.MvxSqlite
{
    /// <summary>
    /// just a very simple helper.
    /// </summary>
    public class SqliteExpressionBuilder
    {
        public string TableName { get; private set; }

        public class SqliteExpression
        {
            private readonly string _tableName;
            private readonly StringBuilder _bld = new StringBuilder();

            public SqliteExpression(string tableName, string initialClause)
            {
                _tableName = tableName;
                _bld.AppendLine(initialClause);
            }

            public SqliteExpression Where(string expression = "")
            {
                _bld.AppendLine(" WHERE " + expression);
                return this;
            }

            public SqliteExpression Equals(string column, string value)
            {
                _bld.Append(" ");
                _bld.Append(column);
                _bld.Append("=");
                _bld.Append(value);
                _bld.Append(" ");
                return this;
            }

            public SqliteExpression Not()
            {
                _bld.Append(" NOT ");
                return this;
            }

            public SqliteExpression And()
            {
                _bld.Append(" And ");
                return this;
            }

            public SqliteExpression NotIn(string column, IEnumerable<string> values)
            {
                _bld.Append(" NOT ");
                return In(column, values);
            }

            public SqliteExpression NotIn(string column, IEnumerable<int> values)
            {
                _bld.Append(" NOT ");
                return In(column, values);
            }
            public SqliteExpression In(string column, IEnumerable<int> values)
            {
                _bld.Append(" ");
                _bld.Append(column);
                _bld.Append(" IN ('");
                _bld.Append(string.Join(",", values.Select(i => i.ToString(CultureInfo.InvariantCulture))));
                _bld.Append(") ");
                return this;
            }

            public SqliteExpression In(string column, IEnumerable<string> values)
            {
                _bld.Append(" ");

                var value = string.Join(",", string.Join("','", values.Select(i => i.Replace("'", "''"))));

                if (value.Length == 0)
                {
                    _bld.Append(" 0 ");
                }
                else
                {
                    _bld.Append(column);
                    _bld.Append(" IN ('");
                    _bld.Append(value);
                    _bld.Append("') ");
                }

                return this;
            }


            public SqliteExpression WhereEquals(string column, string value)
            {
                _bld.Append(" WHERE ");
                Equals(column, value);
                _bld.AppendLine();
                return this;
            }

            public SqliteExpression WhereIn(string column, IEnumerable<string> values)
            {
                _bld.Append(" WHERE ");
                In(column, values);
                _bld.AppendLine();
                return this;
            }

            public SqliteExpression WhereIn(string column, IEnumerable<int> values)
            {
                _bld.Append(" WHERE ");
                In(column, values);
                _bld.AppendLine();
                return this;
            }


            public SqliteExpression OrderBy(string expression)
            {
                _bld.AppendLine(" ORDER BY " + expression);
                return this;
            }

            public SqliteExpression Limit(int count)
            {
                _bld.AppendLine(" LIMIT " + count.ToStringInvariant());
                return this;
            }

            public string Build()
            {
                return _bld.ToString();
            }

            public static implicit operator string(SqliteExpression x)
            {
                return x.Build();
            }
        }

        public SqliteExpressionBuilder(string tableName)
        {
            TableName = tableName;
        }

        public SqliteExpression Select()
        {
            return new SqliteExpression(TableName, string.Format("SELECT * FROM '{0}' ", TableName));
        }

        public SqliteExpression Select(string columns = "*", string tableAlias = "")
        {
            return new SqliteExpression(TableName,
                string.Format("SELECT {0} {1} FROM '{2}' ", columns, tableAlias, TableName));
        }

        public SqliteExpression Delete()
        {
            return new SqliteExpression(TableName,
                string.Format("DELETE FROM '{0}' ", TableName));
        }
    }
}