using System;
using System.Collections.Generic;
using NinjaTasks.Model;
using NinjaTools.Logging;
using TaskWarriorLib;
using Priority = TaskWarriorLib.Priority;

namespace NinjaTasks.Sync.TaskWarrior
{
    public class TaskWarriorToNinjaMapper
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public void FromNinja(TaskWarriorTask target, TodoTaskWithListName source, IEnumerable<string> modified)
        {
            HashSet<string> mod = modified == null ? null : new HashSet<string>(modified);

            target.Uuid = source.Id;

            target.Entry = CheckDate(source.CreatedAt, target.Entry);

            if (mod == null || mod.Contains(TodoTask.ColModifiedAt))
                target.Modified = CheckDate(source.ModifiedAt, target.Modified);

            if (mod == null || mod.Contains(TodoTask.ColStatus) || target.Status == TaskWarriorLib.Status.INVALID)
                target.Status = source.Status == Model.Status.Pending ? TaskWarriorLib.Status.pending : TaskWarriorLib.Status.completed;

            if (mod == null || mod.Contains(TodoTask.ColDescription))
                target.Description = source.Description;

            if (mod == null || mod.Contains(TodoTask.ColCompletedAt))
                target.End = CheckDate(source.CompletedAt, target.End);

            if (mod == null || mod.Contains(TodoTask.ColListFk) || mod.Contains(TodoTaskWithListName.ColListName))
                target.Project = source.ListName;

            if (mod == null || mod.Contains(TodoTask.ColPriority))
                target.Priority = source.Priority != Model.Priority.High ? (Priority?)null : Priority.H;

            if (mod == null || mod.Contains(TodoTask.ColSortPosition))
                target.SortPosition = source.SortPosition;
        }

        public void ToNinja(TodoTaskWithListName target, TaskWarriorTask source)
        {
            target.Id = source.Uuid;

            target.CreatedAt = source.Entry;
            target.ListName = source.Project;
            target.ModifiedAt = source.Modified ?? source.Entry;
            target.SortPosition = source.SortPosition == null ? 0 : (int)source.SortPosition.Value;
            target.Description = source.Description;
            target.CompletedAt = source.End;

            target.Priority = (source.Priority != null && (source.Priority.Value == Priority.H || source.Priority == Priority.M)
                    ? Model.Priority.High
                    : Model.Priority.Normal);
            
            target.Status = source.Status == TaskWarriorLib.Status.completed ? Model.Status.Completed : Model.Status.Pending;
        }

        /// <summary>
        /// task warrior chokes and dies on invalid dates. its a slow 
        /// and painful death, so better keep him alive...
        /// [i.e. he wont reject the data, but fail on any subsequent 
        /// synchronization attempts.
        /// </summary>
        public DateTime? CheckDate(DateTime? date, DateTime? def)
        {
            if (date == null) return null;

            if (date.Value == default(DateTime))
            {
                //// this should not happen!
                Log.Warn("received an default date/time value. setting to now, to keep taskwarrior happy.");
                //Debug.Assert(false);
                return def;
            }

            return date;
        }

        /// <summary>
        /// task warrior chokes and dies on invalid dates. its a slow 
        /// and painful death, so better keep him alive...
        /// [i.e. he wont reject the data, but fail on any subsequent 
        /// synchronization attempts.
        /// </summary>
        public DateTime CheckDate(DateTime date, DateTime def)
        {
            if (date == default(DateTime))
            {
                if(def == default(DateTime))
                    return DateTime.UtcNow;
                return def;
            }
            return date;
        }
    }
}
