using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NinjaSync.Storage;
using NinjaTasks.Model;
using NinjaTasks.Model.ImportExport;
using NinjaTasks.Model.Storage;

namespace NinjaTasks.Sync.ImportExport
{
    public class FileImportExport : IFileImportExport
    {
        private readonly ITodoStorage _storage;
        private readonly ITasksSerializer _serializer;

        public FileImportExport(ITodoStorage storage, ITasksSerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
        }

        public string Import(string filename)
        {
            TodoDataList data;
            using (var s = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                data = _serializer.Deserialize(s);

            if (!data.Tasks.Any())
                return "no tasks where found.";

            _storage.RunInTransaction(() => Merge(data));

            return string.Format("{0} tasks from {1} lists have been imported.", data.Tasks.Count(), data.Lists.Count());
        }

        public string ExportTo(string filename)
        {
            TodoDataList data = new TodoDataList();
            data.Lists = _storage.GetLists().Cast<TodoList>().ToList();
            data.Tasks = _storage.GetTasks().ToList();

             using (var s = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read))
                 _serializer.Serialize(s, data);

             return string.Format("{0} tasks from {1} lists have been exported.", data.Tasks.Count(), data.Lists.Count());
        }

        private void Merge(TodoDataList data)
        {
            // first merge the lists on lists name.
            var listIdReplacement = MergeLists(data);

            // then import tasks
            foreach (var task in data.Tasks)
            {
                task.ListFk = listIdReplacement[task.ListFk];
                _storage.Save(task);
            }
        }

        private Dictionary<string, string> MergeLists(TodoDataList data)
        {
            TodoListLookup lists = new TodoListLookup(_storage);
            var listIdReplacement = new Dictionary<string, string>();

            foreach (var list in data.Lists)
            {
                var l = lists.GetByName(list.Description);
                string tempId = list.Id;
                Debug.Assert(tempId != null);

                if (l != null)
                    listIdReplacement[tempId] = l.Id;
                else
                {
                    list.Id = null;
                    _storage.Save(list);
                    listIdReplacement[tempId] = list.Id;
                }
            }
            return listIdReplacement;
        }
    }
}
