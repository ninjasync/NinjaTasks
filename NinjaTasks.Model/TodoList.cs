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

        public static readonly string ColId           = nameof(Id);
        public static readonly string ColDescription  = nameof(Description);
        public static readonly string ColSortPosition = nameof(SortPosition);
        public static readonly string ColCreatedAt    = nameof(CreatedAt);
        public static readonly string ColModifiedAt   = nameof(ModifiedAt);

        protected static readonly string[] AllProperties = {ColId, ColDescription,  ColSortPosition, 
                                                            ColCreatedAt, ColModifiedAt};
        static TodoList()
        {
            SetupProperties(typeof(TodoList), AllProperties);    
        }

        #endregion
    }
}
