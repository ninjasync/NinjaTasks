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

        public static readonly string ColPendingTasksCount   = nameof(PendingTasksCount);
        public static readonly string ColCompletedTasksCount = nameof(CompletedTasksCount);

        static TodoListWithCount()
        {
            SetupProperties(typeof(TodoListWithCount), AllProperties);
        }
    }
}
