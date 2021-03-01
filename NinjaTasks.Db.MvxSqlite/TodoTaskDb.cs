using System;
using System.Collections.Generic;
using NinjaTools.Sqlite;
using NinjaSync.Storage.MvxSqlite;
using NinjaTasks.Model;

namespace NinjaTasks.Db.MvxSqlite
{
    /// <summary>
    /// 
    /// </summary>
    public class TodoTaskDb
    {
        private TodoTask _base;

        [PrimaryKey, AutoIncrement, Column("Id")]
        public int PrimaryId { get; set; }

        [Unique, Column("Uuid")]
        public string Id { get { return _base.Id; } set { _base.Id = value; } }

        public string Description { get { return _base.Description; } set { _base.Description = value; } }

        [Indexed("ListAndOrder", 0), NotNull]
        public int ListFk { get; set; }

        [Indexed("ListAndOrder", 1), NotNull]
        public Status Status { get { return _base.Status; } set { _base.Status = value; } }

        [Indexed("ListAndOrder", 2), NotNull]
        public int SortPosition { get { return _base.SortPosition; } set { _base.SortPosition = value; } }

        [NotNull]
        public Priority Priority { get { return _base.Priority; } set { _base.Priority = value; } }

        [NotNull]
        public DateTime CreatedAt { get { return _base.CreatedAt; } set { _base.CreatedAt = value; } }

        [NotNull]
        public DateTime ModifiedAt { get { return _base.ModifiedAt; } set { _base.ModifiedAt = value; } }

        public DateTime? CompletedAt { get { return _base.CompletedAt; } set { _base.CompletedAt = value; } }

        // WHY DOES THIS NOT WORK?
        //[Ignore]
        //public List<AdditionalProperty> AdditionalProperties { get { return _base.AdditionalProperties; } set { _base.AdditionalProperties = value; }  }


        public TodoTaskDb()
        {
            _base = new TodoTask();
        }

        public void SetBase(TodoTask task)
        {
            _base = task;
        }

    }
}
