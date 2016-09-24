using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using NinjaSync.Model;
using NinjaSync.Model.Journal;
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
        Normal=0,
        High=1,
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{Description} Sort: {SortPosition} ({Id}) ListFk: {ListFk}")]
    public class TodoTask : TrackableBase
    {
        [Track][DataMember]
        public string Description { get; set; }

        [Track, Indexed("ListAndOrder", 0), NotNull][DataMember]
        public string ListFk{ get; set; }

        [Track, Indexed("ListAndOrder", 1), NotNull][DataMember]
        public Status Status { get; set; }

        [Track, Indexed("ListAndOrder", 2), NotNull][DataMember]
        public int SortPosition { get; set; }

        [Track, NotNull][DataMember]
        public Priority Priority { get; set; }

        [Track][DataMember]
        public DateTime? CompletedAt { get; set; }


        [Ignore]
        public override TrackableType TrackableType { get { return TrackableType.Task;} }

        #region Initialization

#if !DOT42
        public static readonly string ColId = ExpressionHelper.GetMemberName<TodoTask>(x => x.Id);
        public static readonly string ColDescription = ExpressionHelper.GetMemberName<TodoTask>(x => x.Description);
        public static readonly string ColListFk = ExpressionHelper.GetMemberName<TodoTask>(x => x.ListFk);
        public static readonly string ColStatus = ExpressionHelper.GetMemberName<TodoTask>(x => x.Status);
        public static readonly string ColSortPosition = ExpressionHelper.GetMemberName<TodoTask>(x => x.SortPosition);
        public static readonly string ColPriority = ExpressionHelper.GetMemberName<TodoTask>(x => x.Priority);
        public static readonly string ColCreatedAt = ExpressionHelper.GetMemberName<TodoTask>(x => x.CreatedAt);
        public static readonly string ColModifiedAt = ExpressionHelper.GetMemberName<TodoTask>(x => x.ModifiedAt);
        public static readonly string ColCompletedAt = ExpressionHelper.GetMemberName<TodoTask>(x => x.CompletedAt);
#else
        public const string ColId = "Id";
        public const string ColDescription = "Description";
        public const string ColListFk = "ListFk";
        public const string ColStatus = "Status";
        public const string ColSortPosition = "SortPosition";
        public const string ColPriority = "Priority";
        public const string ColCreatedAt = "CreatedAt";
        public const string ColModifiedAt = "ModifiedAt";
        public const string ColCompletedAt = "CompletedAt";

#endif

        protected static readonly string[] AllProperties = 
                                   {ColId, ColDescription, ColListFk, ColStatus, 
                                    ColSortPosition, ColPriority, ColCreatedAt, 
                                    ColModifiedAt, ColCompletedAt};

        static TodoTask()
        {
            SetupProperties(typeof(TodoTask), AllProperties);    
        }

        #endregion
    }
}
