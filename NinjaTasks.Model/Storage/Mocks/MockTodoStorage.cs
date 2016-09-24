using System;
using System.Collections.Generic;
using System.Linq;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;

namespace NinjaTasks.Model.Storage.Mocks
{
    public class MockTodoStorage : ITodoStorage
    {
        public List<TodoList> Lists { get; private set; }
        public List<TodoTask> Tasks { get; private set; }

        public static MockTodoStorage Instance = new MockTodoStorage();

        public MockTodoStorage()
        {
            Tasks = new List<TodoTask>();
            Lists = new List<TodoList>();
            Lists.Add(new TodoList { Description = "liste1", Id="1"});
            Lists.Add(new TodoList { Description = "liste2", Id="2"});
            Lists.Add(new TodoList { Description = "liste3", Id="3"});
            Tasks.Add( new TodoTask{ Description = "task1", ListFk = "1", Id = "1" });
            Tasks.Add(new TodoTask { Description = "task2", ListFk = "1", Id = "2" });
            Tasks.Add(new TodoTask { Description = "task3", ListFk = "1", Id = "3" });
            Tasks.Add(new TodoTask { Description = "task4", ListFk = "1", Id = "4", Status = Status.Completed });
        }

        public IEnumerable<TodoTask> GetTasks(TodoList list = null, bool includeComplete = false, bool onlyHighPriority = false,
            params string[] ids)
        {
            return Tasks.Where(t => (list==null|| t.ListFk == list.Id) 
                                    && (includeComplete || t.Status != Status.Completed));

        }

        public IEnumerable<TodoTask> FindTasks(string searchText)
        {
            return Tasks;
        }

        public int CountTasks(bool includeComplete = false, bool onlyHighPriority = false)
        {
            return Tasks.Count;
        }


        public void SaveList(TodoList list)
        {
        }

        public void SaveTask(TodoTask task)
        {
        }

        public void DeleteList(TodoList list, List<string> returnDeletedTaskIds = null)
        {
        }

       

        public void DeleteTask(TodoTask task)
        {
        }


        public IEnumerable<TodoListWithCount> GetLists(params string[] id)
        {
            foreach (var list in Lists)
            {
                var lwc = new TodoListWithCount();
                lwc.CopyFrom(list);
                lwc.PendingTasksCount = Tasks.Count(p => p.ListFk == list.Id && p.Status == Status.Pending);
                lwc.CompletedTasksCount = Tasks.Count(p => p.ListFk == list.Id && p.Status == Status.Completed);
                yield return lwc;
            }

        }

        public void Save(ITrackable obj, ICollection<string> properties)
        {
        }

        public void Delete(SelectionMode mode, params TrackableId[] id)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            
        }

        public string StorageId { get { return "(none)"; } }

        public ITrackable GetById(TrackableId id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITrackable> GetById(params TrackableId[] ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TrackableId> GetIds(SelectionMode mode, TrackableType type, params string[] ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITrackable> GetTrackable(params TrackableType[] limitToSpecifiedTypes)
        {
            throw new NotImplementedException();
        }

        public void RunInTransaction(Action a)
        {
            a();
        }

        public void RunInImmediateTransaction(Action a)
        {
            a();
        }

        public ITransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }
    }
}