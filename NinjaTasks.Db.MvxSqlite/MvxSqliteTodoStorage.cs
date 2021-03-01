using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NinjaTools.Sqlite;
using NinjaSync.Model;
using NinjaSync.Model.Journal;
using NinjaSync.Storage.MvxSqlite;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools;

namespace NinjaTasks.Db.MvxSqlite
{
    /// <summary>
    /// Note: we use generics to allow us to specify the table names. This is not so pretty.
    /// </summary>
    public class MvxSqliteTodoStorage : MvxSqliteTrackableStorage, ITodoStorage
    {
        private readonly string _tableList;
        private readonly string _tableTask;
        private readonly IPropertyStorage _taskProps;

        public MvxSqliteTodoStorage(ISQLiteConnection sqLite)
            : this(sqLite, null) // important for DI in Dot42 to not have optional parameters
        {
            
        }

        public MvxSqliteTodoStorage(ISQLiteConnection sqLite, string tablePrefix, bool createJournal = true)
            : base(sqLite, tablePrefix)
        {

            bool createdListTable = AddType(TrackableType.List, typeof(TodoList));
            bool createdTaskTable = AddType(TrackableType.Task, typeof(TodoTask), true);

            _taskProps = Types.First(p => p.Key == TrackableType.Task).Value.PropertyStorage;

            _tableTask = tablePrefix + Connection.GetMapping<TodoTask>().TableName;
            _tableList = tablePrefix + Connection.GetMapping<TodoList>().TableName;

            if (createdListTable)
            {
                string deleteTasksOnDeleteList = _tableList + "_delete_tasks";
                Connection.Execute("DROP TRIGGER IF EXISTS " + deleteTasksOnDeleteList);
                var cmd = string.Format(
                    "CREATE TRIGGER {0}\n" +
                    "  AFTER DELETE ON  {1}\n" +
                    "  BEGIN\n" +
                    "    DELETE FROM {2} WHERE {3}=old.{4};\n" +
                    "  END;", deleteTasksOnDeleteList, _tableList, _tableTask, TodoTask.ColListFk, TrackableProperties.ColId);
                Connection.Execute(cmd);
            }

            if (createJournal)
            {
                Connection.EnsureTableCreated<JournalEntry>(tablePrefix + Connection.GetMapping<JournalEntry>().TableName);

                if (createdListTable || createdTaskTable)
                    CreateJournal<JournalEntry>();
            }
        }

        public void DeleteList(TodoList list, List<string> returnDeletedTaskIds = null)
        {
            Connection.RunInTransaction(() =>
            {
                if (returnDeletedTaskIds != null)
                {
                    // don't now if there is a simpler way...
                    var ids = Connection.Query<string>("SELECT Id FROM " + _tableTask + " WHERE ListFk=@1", list.Id);
                    returnDeletedTaskIds.AddRange(ids);
                }

                Connection.Delete<TodoList>(_tableList, list.Id);
            });
        }

        public void DeleteTask(TodoTask task)
        {
            Connection.Delete<TodoTask>(_tableTask, task.Id);
        }

        public IEnumerable<TodoTask> GetTasks(TodoList list = null, bool includeComplete = false,
                                              bool onlyHighPriority = false, params string[] ids)
        {
            if (ids.Length > 0)
                return GetTasks(ids);
            bool includeNormalPriority = !onlyHighPriority;
            bool noList = list == null;
            string listId = noList ? "<BLOCK>" : list.Id;

            var query = "SELECT T.* FROM " + _tableTask + " T \n"
                        + "INNER JOIN  " + _tableList + "  L ON T.ListFk = L.Id \n"
                        + "WHERE (?1 OR T.ListFk = ?2) AND (?3 OR T.Status <> ?4) AND (?5 OR T.Priority = ?6) "
                        + "ORDER BY L.Description<>'', L.SortPosition, L.CreatedAt, \n" +
                        "           T.Status, T.Priority DESC, T.SortPosition, T.CreatedAt ";
            return Connection.DeferredQuery<TodoTask>(
                                query,
                                noList, listId, includeComplete, Status.Completed, includeNormalPriority, Priority.High)
                             .Select(LoadAdditionalProperties);
        }

        public int CountTasks(bool includeComplete = false, bool onlyHighPriority = false)
        {
            bool ignorePriority = !onlyHighPriority;
            return Connection.NxTable<TodoTask>(_tableTask)
                             .Where("? OR Status=?", includeComplete, Status.Pending)
                             .Where("? OR Priority=?", ignorePriority, Priority.High)
                             .Count();
            //.Count(t => (includeComplete || t.Status == Status.Pending) 
            //          && (ignorePriority || t.Priority == Priority.High));
        }


        public IEnumerable<TodoTask> FindTasks(string searchText)
        {
            return Connection.Query<TodoTask>(
                "SELECT T.* From  " + _tableTask + " T "
              + "INNER JOIN  " + _tableList + "  L ON T.ListFk = L.Id "
              + "WHERE T.Description LIKE ? "
              + "ORDER BY L.SortPosition, L.CreatedAt, T.Status, T.Priority DESC, T.SortPosition, T.CreatedAt ",
                "%" + searchText + "%");
        }

        public IEnumerable<TodoListWithCount> GetLists(params string[] id)
        {
            string whereClause = id.Length == 0 ? "" : SQLiteHelpers.MakeIdWhereClause(id);

            string cmd = string.Format(
                "SELECT L.*, \n" +
                "    (SELECT COUNT(*) from {0} T where T.{1}=L.{2} AND T.{3}={4}) as {6}, \n" +
                "    (SELECT COUNT(*) from {0} T where T.{1}=L.{2} AND T.{3}={5}) as {7} \n" +
                "FROM {8} L\n" +
                " {9} \n", _tableTask,
                 TodoTask.ColListFk, TrackableProperties.ColId, TodoTask.ColStatus,
                 (int)Status.Pending, (int)Status.Completed,
                 TodoListWithCount.ColPendingTasksCount, TodoListWithCount.ColCompletedTasksCount,
                 _tableList, whereClause);


            return Connection.Query<TodoListWithCount>(cmd)
                             .AsEnumerable()
                             .OrderBy(k => k.SortPosition)
                             .ThenBy(k => k.Description)
                             .ThenBy(k => k.CreatedAt);
        }

        private IEnumerable<TodoTask> GetTasks(params string[] id)
        {
            if (id.Length == 0)
                return Connection.NxTable<TodoTask>(_tableTask)
                                 .AsEnumerable()
                                 .Select(LoadAdditionalProperties);

            var cmd = "SELECT * FROM  " + _tableTask + SQLiteHelpers.MakeIdWhereClause(id) + "";
            return Connection.Query<TodoTask>(cmd)
                              .Select(LoadAdditionalProperties);
        }


        protected void CreateJournal<TJournalType>() where TJournalType : JournalEntry
        {
            new MvxSqliteTodoStorageJournalTriggers<TodoList>().CreateTriggers(Connection, TablePrefix);
            new MvxSqliteTodoStorageJournalTriggers<TodoTask>().CreateTriggers(Connection, TablePrefix, true);
        }

        public override void Save(ITrackable obj, ICollection<string> properties)
        {
#if DEBUG
            if (obj.TrackableType == TrackableType.Task)
            {
                if (properties == null || properties.Contains(TodoTask.ColListFk))
                    Debug.Assert(!((TodoTask)obj).ListFk.IsNullOrEmpty());
            }
#endif
            base.Save(obj, properties);
        }

        private TodoTask LoadAdditionalProperties(TodoTask arg)
        {
            foreach (var prop in _taskProps.GetProperties(arg.Id))
                arg.SetProperty(prop.Item1, prop.Item2);
            return arg;
        }
    }
}
