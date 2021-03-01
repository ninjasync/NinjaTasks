using NinjaTools.Sqlite;
using NinjaSync.Model.Journal;
using System;

namespace NinjaTasks.Db.MvxSqlite
{
    [Table("TodoList")]
    public class TodoListDb 
    {
        [PrimaryKey, AutoIncrement, Column("Id")]
        public int PrimaryId { get; set; }

        [Unique, Column("Uuid")]
        public string Id { get; set; }

        [Track]
        public string Description { get; set; }

        [Track, Indexed, NotNull]
        public int SortPosition { get; set; }
        
        [NotNull]
        public DateTime CreatedAt { get; set; }

        [Track, Indexed, NotNull]
        public DateTime ModifiedAt { get; set; }
       
    }
}
