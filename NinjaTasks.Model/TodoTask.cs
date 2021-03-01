using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using NinjaTools.Sqlite;
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

        public static readonly string ColId           = nameof(Id);
        public static readonly string ColDescription  = nameof(Description);
        public static readonly string ColListFk       = nameof(ListFk);
        public static readonly string ColStatus       = nameof(Status);
        public static readonly string ColSortPosition = nameof(SortPosition);
        public static readonly string ColPriority     = nameof(Priority);
        public static readonly string ColCreatedAt    = nameof(CreatedAt);
        public static readonly string ColModifiedAt   = nameof(ModifiedAt);
        public static readonly string ColCompletedAt  = nameof(CompletedAt);

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
