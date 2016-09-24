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
    [DataContract]
    [DebuggerDisplay("{Description} Sort: {SortPosition} ({Id})")]
    public class TodoList : TrackableBase
    {
        [Track][DataMember]
        public string Description { get; set; }

        [Track, Indexed, NotNull][DataMember]
        public int    SortPosition { get; set; }
        
        [Ignore]
        public override TrackableType TrackableType { get { return TrackableType.List; } }

        #region Initialization

#if !DOT42
        public static readonly string ColId = ExpressionHelper.GetMemberName<TodoList>(x => x.Id);
        public static readonly string ColDescription = ExpressionHelper.GetMemberName<TodoList>(x => x.Description);
        public static readonly string ColSortPosition = ExpressionHelper.GetMemberName<TodoList>(x => x.SortPosition);
        public static readonly string ColCreatedAt = ExpressionHelper.GetMemberName<TodoList>(x => x.CreatedAt);
        public static readonly string ColModifiedAt = ExpressionHelper.GetMemberName<TodoList>(x => x.ModifiedAt);
#else
        public const string ColId = "Id";
        public const string ColDescription = "Description";
        public const string ColSortPosition = "SortPosition";
        public const string ColCreatedAt = "CreatedAt";
        public const string ColModifiedAt = "ModifiedAt";
#endif

        protected static readonly string[] AllProperties = {ColId, ColDescription,  ColSortPosition, 
                                                            ColCreatedAt, ColModifiedAt};
        static TodoList()
        {
            SetupProperties(typeof(TodoList), AllProperties);    
        }

        #endregion
    }
}
