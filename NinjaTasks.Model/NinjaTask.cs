using System;
using System.Collections.Generic;
using System.ComponentModel;
using Cirrious.NinjaTools.Sqlite;
using NinjaTasks.Model.Journal;
using NinjaTools;

namespace NinjaTasks.Model
{
    public enum Status
    {
        Pending=0,
        Completed=1,
    }

    public enum Priority
    {
        Normal,
        High,
    }

    public class AdditionalProperty
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public AdditionalProperty()
        {
            
        }

        public AdditionalProperty(string type, string key, string value)
        {
            Type = type;
            Key = key;
            Value = value;
        }

        /// <summary>
        /// this can be "TaskWarrior" or "CalDav" or something.
        /// </summary>
        public string Type { get; set; }  
    }

    /// <summary>
    /// 
    /// </summary>
    public class TodoTask : INotifyPropertyChanged, ITrackable // note: weaved by propertychanged.fody
    {
        public string Id { get; set; }
        
        public DateTime CreatedAt { get; set; }

        [Track]
        public string Description { get; set; }

        [Track]
        public string ListId{ get; private set; }

        [Track]
        public string ListName { get; private set; }

        [Track]
        public Status Status { get; set; }

        [Track]
        public int SortPosition { get; set; }

        [Track]
        public Priority Priority { get; set; }

        [Track]
        public DateTime ModifiedAt { get; set; }

        [Track]
        public DateTime? CompletedAt { get; set; }

        #region Journaling
        [Ignore]
        JournalType ITrackable.JournalType { get { return JournalType; } }
        public static readonly JournalType JournalType = JournalType.Task;
        #endregion
        /// <summary>
        /// this contains all unsupported but preserved external properties.
        /// </summary>
        /// 
        public List<AdditionalProperty> AdditionalProperties { get; set; }

        public TodoTask()
        {
            AdditionalProperties = new List<AdditionalProperty>();
        }

        public void SetList(string listId, string listName)
        {
            ListId = listId;
            ListName = listName;
        }

        public static readonly string ColId = ExpressionHelper.GetMemberName<TodoTask>(x => x.Id);
        public static readonly string ColDescription = ExpressionHelper.GetMemberName<TodoTask>(x => x.Description);
        public static readonly string ColListFk = ExpressionHelper.GetMemberName<TodoTask>(x => x.ListFk);
        public static readonly string ColStatus = ExpressionHelper.GetMemberName<TodoTask>(x => x.Status);
        public static readonly string ColSortPosition = ExpressionHelper.GetMemberName<TodoTask>(x => x.SortPosition);
        public static readonly string ColPriority = ExpressionHelper.GetMemberName<TodoTask>(x => x.Priority);
        public static readonly string ColCreatedAt = ExpressionHelper.GetMemberName<TodoTask>(x => x.CreatedAt);
        public static readonly string ColModifiedAt = ExpressionHelper.GetMemberName<TodoTask>(x => x.ModifiedAt);
        public static readonly string ColCompletedAt = ExpressionHelper.GetMemberName<TodoTask>(x => x.CompletedAt);

        public static readonly IList<string> AllColumns = new[]
        {
            ColId, ColDescription, ColListFk, ColStatus, ColSortPosition, ColPriority, 
            ColCreatedAt, ColModifiedAt, ColCompletedAt
        };

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
