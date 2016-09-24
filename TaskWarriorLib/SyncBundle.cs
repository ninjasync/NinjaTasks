using System.Collections.Generic;

namespace TaskWarriorLib
{
    public class SyncBundle
    {
        public IList<TaskWarriorTask> ChangedTasks = new List<TaskWarriorTask>();
        public string SyncId;
    }
}
