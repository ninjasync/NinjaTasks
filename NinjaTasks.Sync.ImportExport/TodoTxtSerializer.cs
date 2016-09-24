using System;
using System.Globalization;
using System.IO;
using System.Linq;
using NinjaTasks.Model;
using NinjaTasks.Model.ImportExport;
using NinjaTasks.Model.Storage;
using ToDoLib;

namespace NinjaTasks.Sync.ImportExport
{
    public class TodoTxtSerializer : ITasksSerializer
    {
        public TodoDataList Deserialize(Stream s)
        {
            TodoDataList ret = new TodoDataList();
            StreamReader reader = new StreamReader(s);
            string raw;
            while ((raw = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(raw))
                {
                    var todo = new Task(raw);
                    var task = new TodoTask();

                    TodoTxtToLocal(todo, task, ret);

                    ret.Tasks.Add(task);
                }
            }

            return ret;
        }

        public void Serialize(Stream s, TodoDataList data)
        {
            TodoListLookup lookup = new TodoListLookup(data.Lists);
            
            using(StreamWriter w = new StreamWriter(s))
            foreach (var task in data.Tasks)
            {
                var todo = new Task("");
                
                LocalToTodoTask(task, todo, lookup);
                w.Write(todo.ToString() + "\n");
            }
        }

        private void LocalToTodoTask(TodoTask task, Task todo, TodoListLookup lookup)
        {
            todo.Body = task.Description;
            todo.Priority = task.Priority == Priority.High ? "(A)" : "";

            todo.Completed = task.Status == Status.Completed;

            todo.CreationDate = task.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd");

            DateTime? completed = task.CompletedAt;
            if (todo.Completed && completed == null)
                completed = DateTime.UtcNow;
                
            todo.CompletedDate = completed == null ? "" : completed.Value.ToLocalTime().ToString("yyyy-MM-dd");

            string listname = lookup.GetById(task.ListFk).Description;
            if(!string.IsNullOrEmpty(listname))
                todo.Contexts.Add("@" + listname.Replace(" ", ""));
        }

        private void TodoTxtToLocal(Task todo, TodoTask task, TodoDataList ret)
        {
            task.Description = todo.Body;

            task.Priority = !string.IsNullOrWhiteSpace(todo.Priority) ? Priority.High : Priority.Normal;
            task.Status = todo.Completed ? Status.Completed : Status.Pending;

            DateTime dt;
            if (DateTime.TryParseExact(todo.CreationDate, "yyyy-MM-dd", null, DateTimeStyles.AdjustToUniversal, out dt))
                task.CreatedAt = dt;
            else
                task.CreatedAt = DateTime.Now;

            if (DateTime.TryParseExact(todo.CompletedDate, "yyyy-MM-dd", null, DateTimeStyles.AdjustToUniversal, out dt))
                task.CompletedAt = dt;

            string listName = "";
            listName = !string.IsNullOrWhiteSpace(todo.PrimaryContext)
                             ? todo.PrimaryContext : todo.PrimaryProject;
            
            if (listName != null && (listName.StartsWith("+") || listName.StartsWith("@")))
                listName = listName.Substring(1);

            task.ListFk = AddOrGetList(ret, listName);
        }

        private string AddOrGetList(TodoDataList ret, string s)
        {
            if (string.IsNullOrWhiteSpace(s)) s = "";

            var list = ret.Lists.FirstOrDefault(r => r.Description == s);
            if (list == null)
            {
                list = new TodoList {Description = s };
                list.SetNewId();
                ret.Lists.Add(list);
            }
            return list.Id;
        }

       
    }
}
