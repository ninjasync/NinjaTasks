using Cirrious.MvvmCross.Community.Plugins.Sqlite;


namespace NinjaSync.Model
{
    /// <summary>
    /// this class allows to store a remote representation of an object,
    /// i.e. the origiginal TaskWarrior JSON
    /// </summary>
    public class SyncRemoteRepresentation 
    {
        [PrimaryKey]
        public string AccountId { get; set; }

        [PrimaryKey]
        public string Uuid { get; set; }

        [NotNull]
        public string Representation { get; set; }
    }
}
