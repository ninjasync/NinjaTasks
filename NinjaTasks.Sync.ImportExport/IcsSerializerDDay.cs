using System;
using System.IO;
using System.Linq;
using System.Text;
using DDay.iCal;
using NinjaTasks.Model;
using NinjaTasks.Model.ImportExport;
using NinjaTasks.Model.Storage;

namespace NinjaTasks.Sync.ImportExport
{
    public class IcsSerializerDDay : ITasksSerializer
    {
        public bool TreatCategoriesAsList { get; set; }

        public TodoDataList Deserialize(Stream s)
        {
            TodoDataList ret = new TodoDataList();
            
            var serializer = new DDay.iCal.Serialization.iCalendar.iCalendarSerializer();
            iCalendarCollection calcol = (iCalendarCollection)serializer.Deserialize(s, Encoding.UTF8);

            foreach (IICalendar cal in calcol)
            foreach (var item in cal.Todos)
            {
                TodoTask task = new TodoTask();

                IcalToLocalTask(task, item, ret);

                ret.Tasks.Add(task);
            }

            return ret;
        }

        public void Serialize(Stream s, TodoDataList data)
        {
            TodoListLookup lookup = new TodoListLookup(data.Lists);

            iCalendar cal = new iCalendar();

            foreach (var task in data.Tasks)
            {
                var todo = new Todo();
                LocalToICal(task, todo, lookup);
                cal.Todos.Add(todo);
            }

            var serializer = new DDay.iCal.Serialization.iCalendar.iCalendarSerializer();
            serializer.Serialize(cal, s, Encoding.UTF8);
        }

        private void IcalToLocalTask(TodoTask task, ITodo todo, TodoDataList ret)
        {
            task.Description = todo.Summary;
            task.CompletedAt = todo.Completed == null ? (DateTime?) null : todo.Completed.UTC;
            task.ModifiedAt = todo.LastModified != null ? todo.LastModified.UTC : DateTime.UtcNow;
            task.Priority = todo.Priority > 5 ? Priority.High : Priority.Normal;
            task.Status = todo.Status == TodoStatus.Completed || todo.Status == TodoStatus.Cancelled
                ? Status.Completed
                : Status.Pending;

            string listName = "";
            if (TreatCategoriesAsList)
                listName = todo.Categories == null ? "" : todo.Categories.FirstOrDefault();
            task.ListFk = AddOrGetListId(ret, listName);
        }

        private void LocalToICal(TodoTask task, Todo todo, TodoListLookup lookup)
        {
            todo.Summary = task.Description;
            todo.Completed = task.CompletedAt == null ? null : new iCalDateTime(task.CompletedAt.Value);
            todo.LastModified = new iCalDateTime(task.ModifiedAt);
            todo.Priority = task.Priority == Priority.High ? 9 : 0;
            todo.Status = task.Status == Status.Completed ? TodoStatus.Completed : TodoStatus.NeedsAction;

            if (TreatCategoriesAsList)
            {
                string listname = lookup.GetById(task.ListFk).Description;
                if(!string.IsNullOrEmpty(listname))
                    todo.Categories = new[] {listname};
            }
        }

        private string AddOrGetListId(TodoDataList ret, string listName)
        {
            if (string.IsNullOrWhiteSpace(listName)) listName = "";

            var list = ret.Lists.FirstOrDefault(r => r.Description == listName);
            if (list == null)
            {
                list = new TodoList {Description = listName};
                list.SetNewId();
                ret.Lists.Add(list);
            }
            return list.Id;
        }
    }
}
