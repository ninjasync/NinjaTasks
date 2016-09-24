using System;
using System.Collections.Generic;
using System.ComponentModel;
using NinjaTasks.Model.Journal;
using NinjaTools;

namespace NinjaTasks.Model
{
    public class TodoList : INotifyPropertyChanged, ITrackable
    {
        public string Id { get; set; }

        [Track]
        public string Description { get; set; }

        [Track]
        public int SortPosition { get; set; }
        
        public DateTime CreatedAt { get; set; }

        [Track]
        public DateTime ModifiedAt { get; set; }

        JournalType ITrackable.JournalType { get { return JournalType; } }
        public static readonly JournalType JournalType = JournalType.List;

        public static readonly string ColId = ExpressionHelper.GetMemberName<TodoList>(x => x.Id);
        public static readonly string ColDescription = ExpressionHelper.GetMemberName<TodoList>(x => x.Description);
        public static readonly string ColSortPosition = ExpressionHelper.GetMemberName<TodoList>(x => x.SortPosition);
        public static readonly string ColCreatedAt = ExpressionHelper.GetMemberName<TodoList>(x => x.CreatedAt);
        public static readonly string ColModifiedAt = ExpressionHelper.GetMemberName<TodoList>(x => x.ModifiedAt);

        public static readonly IList<string> AllColumns = new[]
        {
            ColId, ColDescription, ColSortPosition, 
            ColCreatedAt, ColModifiedAt
        };

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
