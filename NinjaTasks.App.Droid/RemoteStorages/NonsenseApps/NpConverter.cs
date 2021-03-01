using System;
using System.Collections.Generic;
using Android.Content;
using Android.Database;
using NinjaTasks.Model;
using NinjaTools;
using NinjaTools.Logging;

namespace NinjaTasks.App.Droid.RemoteStorages.NonsenseApps
{
    public class NpConverter
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public TodoTask TodoTaskFromCursor(ICursor cursor)
        {
            TodoTask ret = new TodoTask();

            ret.Id = cursor.GetLong(NpContract.ColId).ToStringInvariant();
            ret.Description = cursor.GetString(NpContract.ColTaskTitle);
            ret.ListFk = cursor.GetLong(NpContract.ColTaskDblist).ToStringInvariant();
            
            ret.CompletedAt = cursor.GetDateTimeFromUnixMilliesNullable(NpContract.ColTaskCompleted);
            if (ret.CompletedAt == default(DateTime)) ret.CompletedAt = null;

            ret.ModifiedAt = cursor.GetDateTimeFromUnixMillies(NpContract.ColTaskUpdated);

            if (ret.ModifiedAt == default (DateTime))
                ret.ModifiedAt = ret.CreatedAt;

            ret.SortPosition = cursor.GetInt(NpContract.ColTaskLeft);

            ret.Status = ret.CompletedAt != null
                            ? Status.Completed : Status.Pending;

            Log.Debug("loading task {0} status {1} CompletedAt {2} ModifiedAt {3} ", ret.Id, ret.Status, ret.CompletedAt, ret.ModifiedAt);

            return ret;
        }

        public ContentValues ToContentValues(TodoTask task, bool forInsert, long translatedListFk, IList<string> mod = null)
        {
            ContentValues val = new ContentValues();

            Log.Debug("storing task {0} status {1} CompletedAt {2} ModifiedAt {3} ", task.Id, task.Status, task.CompletedAt, task.ModifiedAt);


            if (forInsert || mod == null || mod.Contains(TodoTask.ColDescription))
                val.Put(NpContract.ColTaskTitle, task.Description);

            if (forInsert || mod == null || mod.Contains(TodoTask.ColListFk))
                val.Put(NpContract.ColTaskDblist, translatedListFk);

            if (forInsert || mod == null || mod.Contains(TodoTask.ColModifiedAt))
            {
                var unixMillies = task.ModifiedAt.FromUtcToMillisecondsUnixTime();
                val.Put(NpContract.ColTaskUpdated, unixMillies);

                // also update the original value to match our data model.
                task.ModifiedAt = unixMillies.FromMillisecondsUnixTimeToUtc();
            }


            if (forInsert || mod == null || mod.Contains(TodoTask.ColStatus) || mod.Contains(TodoTask.ColCompletedAt))
            {
                if (task.Status == Status.Completed)
                {
                    var completedUnixMillies = (task.CompletedAt == null ? DateTime.UtcNow : task.CompletedAt.Value)
                        .FromUtcToMillisecondsUnixTime();
                    val.Put(NpContract.ColTaskCompleted, completedUnixMillies);
                    // update to our data model.
                    task.CompletedAt = completedUnixMillies.FromMillisecondsUnixTimeToUtc();
                }
                else
                {
                    val.PutNull(NpContract.ColTaskCompleted);
                    // update to our data model.
                    task.Status = Status.Pending;
                    task.CompletedAt = null;
                }
            }

            if (forInsert)
            {
                // we've got to give him the non-nullable types as well.
                //val.PutNull(NpContract.ColTaskNote);
                //val.PutNull(NpContract.ColTaskDue);
                val.Put(NpContract.ColTaskLocked, 0);

                val.Put(NpContract.ColTaskLeft, 1);
                val.Put(NpContract.ColTaskRight, 2);
            }

            //if (mod == null || mod.Contains(TodoTask.ColSortPosition))
            //{
            //    val.Put(NpContract.ColTaskLeft, task.SortPosition);
            //    val.Put(NpContract.ColTaskRight, task.SortPosition + 1);
            //}


            return val;
        }

        public TodoList TodoListFromCursor(ICursor cursor)
        {
            TodoList ret = new TodoList();
            ret.Id = cursor.GetLong(NpContract.ColId).ToStringInvariant();
            //ret.CreatedAt = cursor.Get
            ret.Description = cursor.GetString(NpContract.ColListTitle);
            ret.ModifiedAt = cursor.GetDateTimeFromUnixMillies(NpContract.ColListUpdated);
            //ret.SortPosition = cursor.GetInt(NpConstants.ColListSorting);

            // NotePad doesn't have a dedicated inbox, at least not database-wise, so
            // map the Inbox to a dedicated "Inbox". This might not be to every users
            // liking, especially if they don't speak english.
            if (ret.Description == "Inbox") ret.Description = "";

            return ret;
        }

        public ContentValues ToContentValues(TodoList list)
        {
            ContentValues val = new ContentValues();
            
            // Notepad always requires all values for lists. so don't hesitate...

            // NotePad doesn't have a dedicated inbox, at least not database-wise, so
            // map the Inbox to a dedicated "Inbox". This might not be to every users
            // liking, especially if they don't speak english.
            if (string.IsNullOrEmpty(list.Description))
            {
                val.Put(NpContract.ColListTitle, "Inbox");
                list.Description = "";
            }
            else
                val.Put(NpContract.ColListTitle, list.Description);

            var modifiedUnixMillies = list.ModifiedAt.FromUtcToMillisecondsUnixTime();
            val.Put(NpContract.ColListUpdated, modifiedUnixMillies);

            // update according to our data model
            list.ModifiedAt = modifiedUnixMillies.FromMillisecondsUnixTimeToUtc();

            return val;
        }
    }
}