using System.Collections.Generic;
using System.IO;

namespace NinjaTasks.Model.ImportExport
{
    public class TodoDataList
    {
        public IList<TodoList> Lists { get; set; }
        public IList<TodoTask> Tasks { get; set; }

        public TodoDataList()
        {
            Lists= new List<TodoList>();
            Tasks= new List<TodoTask>();
        }
    }

    public class SerializationOptions
    {
        public bool TreatCategoriesAsList { get; set; }
    }

    public interface ITasksSerializer
    {
        TodoDataList Deserialize(Stream s);
        void Serialize(Stream s, TodoDataList data);
    }
}
