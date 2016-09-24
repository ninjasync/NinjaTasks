using System.Runtime.Serialization;
using NinjaTools;

namespace NinjaTasks.Model
{
    public class TodoListWithCount : TodoList
    {
        [DataMember]
        public int PendingTasksCount { get; set; }
        [DataMember]
        public int CompletedTasksCount { get; set; }

#if !DOT42
        public static readonly string ColPendingTasksCount = ExpressionHelper.GetMemberName<TodoListWithCount>(x => x.PendingTasksCount);
        public static readonly string ColCompletedTasksCount = ExpressionHelper.GetMemberName<TodoListWithCount>(x => x.CompletedTasksCount);
#else
        public const string ColPendingTasksCount = "PendingTasksCount";
        public const string ColCompletedTasksCount = "CompletedTasksCount";
#endif

        static TodoListWithCount()
        {
            SetupProperties(typeof(TodoListWithCount), AllProperties);    
        }
    }
}
