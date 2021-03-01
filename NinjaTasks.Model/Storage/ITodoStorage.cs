using System.Collections.Generic;
using NinjaSync.Storage;

namespace NinjaTasks.Model.Storage
{
    /// <summary>
    /// List and Task CRUD
    /// </summary>
    public interface ITodoStorage : ITrackableStorage
    {
        IEnumerable<TodoListWithCount> GetLists(params string[] ids);
        /// <summary>
        /// when ids != null, all other parameters are ignored.
        /// </summary>
        IEnumerable<TodoTask> GetTasks(TodoList list=null, bool includeComplete=true, bool onlyHighPriority=false, params string[] ids);

        IEnumerable<TodoTask> FindTasks(string searchText);

        int CountTasks(bool includeComplete = false, bool onlyHighPriority = false);

        /// <summary>
        /// this will also delete all associated tasks.
        /// </summary>
        void DeleteList(TodoList list, List<string> returnDeletedTaskIds = null);

        /// <summary>
        /// deletes a single task.
        /// </summary>
        /// <param name="task"></param>
        void DeleteTask(TodoTask task);
    }

   
}
