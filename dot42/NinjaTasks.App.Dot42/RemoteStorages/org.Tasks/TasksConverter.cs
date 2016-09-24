using System;
using System.Collections.Generic;
using System.Globalization;
using Android.Content;
using Android.Database;
using NinjaTasks.Model;
using NinjaTools;
using NinjaTools.Dot42.Android;

namespace NinjaTasks.App.Droid.RemoteStorages.org.Tasks
{
    class OrgTasksTodoTask : TodoTask
    {
        public long IdSplitId { get { return GetIdSplitId(this); }}
        public string IdSplitUuid { get { return GetIdSplitUuid(this); } }

        public void SetId(long id, string uuid)
        {
            SetId(this, id, uuid);
        }

        public static long GetIdSplitId(TodoTask task)
        {
            return task.Id == null ? 0 : long.Parse(task.Id.Split(':')[0]);
            
        }

        public static string GetIdSplitUuid(TodoTask task)
        {
            return task.Id == null ? null : task.Id.Split(':')[1]; 
        }


        public static void SetId(TodoTask task, long id, string uuid)
        {
            task.Id = id.ToStringInvariant() + ":" + uuid;
        }
    }

    class OrgTasksTodoList : TodoList
    {
    }

    class OrgTasksTagsTagMetadata
    {
        public int Id;
        public int TaskId;
        //public string ColMetadataKey = "key";
        public string TagName;
        public string TagUuid;
        public string TaskUuid;
    }

    class TasksConverter
    {
        public OrgTasksTodoTask TodoTaskFromCursor(ICursor cursor)
        {
            OrgTasksTodoTask ret = new OrgTasksTodoTask();

            ret.SetId(CursorExtensions.GetLong(cursor, TasksContract.ColId), CursorExtensions.GetString(cursor, TasksContract.ColTaskRemoteId));
            ret.Description = CursorExtensions.GetString(cursor, TasksContract.ColTaskTitle);
            
            ret.CreatedAt = CursorExtensions.GetDateTimeFromUnixMillies(cursor, TasksContract.ColTaskCreated);

            ret.ModifiedAt = CursorExtensions.GetDateTimeFromUnixMillies(cursor, TasksContract.ColTaskModified);
            if (ret.ModifiedAt == default(DateTime)) 
                ret.ModifiedAt = ret.CreatedAt;

            ret.CompletedAt = CursorExtensions.GetDateTimeFromUnixMillies(cursor, TasksContract.ColTaskCompleted);
            if (ret.CompletedAt == default(DateTime)) 
                ret.CompletedAt = null;

            ret.Status = ret.CompletedAt != null ? Status.Completed : Status.Pending;

            TasksContract.Importance imp = (TasksContract.Importance) CursorExtensions.GetInt(cursor, TasksContract.ColTaskImportance);
            ret.Priority = imp > TasksContract.Importance.MustDo ? Priority.Normal : Priority.High;

            return ret;
        }

        public ContentValues ToContentValues(TodoTask task, out string uuidIfNewTask, IList<string> mod = null)
        {
            ContentValues val = new ContentValues();
            uuidIfNewTask = null;
            
            bool forInsert = task.IsNew;

            if (forInsert || mod == null || mod.Contains(TodoTask.ColDescription))
                val.Put(TasksContract.ColTaskTitle, task.Description);

            //if (forInsert || mod == null || mod.Contains(TodoTask.ColListFk))
            //    val.Put(TasksContract.ColTaskDblist, task.ListFk);

            if (forInsert || mod == null || mod.Contains(TodoTask.ColModifiedAt))
                val.Put(TasksContract.ColTaskModified, task.ModifiedAt.FromUtcToMillisecondsUnixTime());

            if (forInsert || mod == null || mod.Contains(TodoTask.ColCreatedAt))
                val.Put(TasksContract.ColTaskCreated, task.CreatedAt.FromUtcToMillisecondsUnixTime());
           
            if (forInsert || mod == null || mod.Contains(TodoTask.ColStatus) || mod.Contains(TodoTask.ColCompletedAt))
            {
                if (task.Status != Status.Completed)
                    val.PutNull(TasksContract.ColTaskCompleted);
                else
                    val.Put(TasksContract.ColTaskCompleted,
                        (task.CompletedAt == null ? DateTime.UtcNow : task.CompletedAt.Value)
                            .FromUtcToMillisecondsUnixTime());
            }

            if (forInsert || mod == null || mod.Contains(TodoTask.ColPriority))
            {
                var importance = task.Priority == Priority.High ? TasksContract.Importance.DoOrDie
                                                                : TasksContract.Importance.ShouldDo;
                val.Put(TasksContract.ColTaskImportance, (int)importance);
            }

            if (forInsert)
            {
                uuidIfNewTask = OrgTasksNewUuid();
                val.Put(TasksContract.ColTaskRemoteId, uuidIfNewTask);
            }

            //if (mod == null || mod.Contains(TodoTask.ColSortPosition))
            //{
            //    val.Put(TasksContract.ColTaskLeft, task.SortPosition);
            //    val.Put(TasksContract.ColTaskRight, task.SortPosition + 1);
            //}

            return val;
        }


        public OrgTasksTagsTagMetadata MetadataFromCursor(ICursor cursor)
        {
            OrgTasksTagsTagMetadata ret = new OrgTasksTagsTagMetadata();
            ret.Id = CursorExtensions.GetInt(cursor, TasksContract.ColId);
            ret.TaskId = CursorExtensions.GetInt(cursor, TasksContract.ColMetadataTask);
            ret.TagName = CursorExtensions.GetString(cursor, TasksContract.ColMetadataValue);
            ret.TagUuid = CursorExtensions.GetString(cursor, TasksContract.ColMetadataValue2);
            ret.TaskUuid = CursorExtensions.GetString(cursor, TasksContract.ColMetadataValue3);
            return ret;
        }


        public static string OrgTasksNewUuid()
        {
            // create a new uuid (by a strange algorithm...)
            const long minUuid = 100000000;
            long newUuid = 0;
            while (newUuid < minUuid)
            {
                var guid = Guid.NewGuid().ToByteArray();
                newUuid = (long)BitConverter.ToUInt64(guid, 0) & 0x7fffffffffffffffL;
            }

            return newUuid.ToString(CultureInfo.InvariantCulture);
        }

    }
}