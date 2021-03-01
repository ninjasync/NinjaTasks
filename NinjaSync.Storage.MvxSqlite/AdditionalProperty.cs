using NinjaTools.Sqlite;

namespace NinjaSync.Storage.MvxSqlite
{
    public class AdditionalProperty
    {
        [PrimaryKey]
        public string Id { get; set; }

        [PrimaryKey]
        public string Member { get; set; }

        public string Value { get; set; }
    }

    public class NullProperty : AdditionalProperty
    {

    }
}
