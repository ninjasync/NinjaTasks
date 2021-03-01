using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Android.Content;
using Android.Database;
using NinjaSync.Model.Journal;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools;
using NinjaTools.Progress;

namespace NinjaTasks.App.Droid.RemoteStorages.org.Tasks
{
    /// <summary>
    /// This code is beta quality.
    /// </summary>
    public class OrgTasksRemoteStorage : ITodoRemoteSlaveStorage
    {
        private readonly ContentProviderClient _provider;
        private readonly TasksConverter _convert = new TasksConverter();
        private TodoListLookup _lists = new TodoListLookup();

        public OrgTasksRemoteStorage(ContentProviderClient provider)
        {
            _provider = provider;
        }

        public TrackableType[] SupportedTypes { get { return new[] { TrackableType.List, TrackableType.Task }; } }

        public IList<string> GetMappedColums(TrackableType type)
        {
            if (type == TrackableType.List)
                return new[]
                {
                    TodoList.ColDescription
                };
            if (type == TrackableType.Task)
                return new[]
                {
                    TodoTask.ColId, TodoTask.ColCreatedAt, TodoTask.ColCompletedAt, TodoTask.ColModifiedAt,
                    TodoTask.ColDescription, TodoTask.ColListFk, TodoTask.ColStatus, TodoTask.ColPriority,
                    //TodoTask.ColSortPosition,

                };
            return new string[0];
        }

        public TrackableRemoteStorageType StorageType { get { return TrackableRemoteStorageType.HasOnlyImplicitLists; } }

        public CommitList GetModifications(string commitsNewerButExcludingCommitId, IProgress p)
        {
            Debug.Assert(commitsNewerButExcludingCommitId == null);

            _lists = new TodoListLookup();
            _lists.Add(new TodoList { Id = SequentialGuid.NewGuidString() });
            Dictionary<string, TodoList> uuidToList = new Dictionary<string, TodoList>();
            uuidToList.Add("", _lists.Lists[0]); // Inbox

            ICursor cursor;

            // Unfortunately, TasksContract.UriTagdata is not accessible through the 
            // content provider.
            //// collect "tags" to convert them to lists. also collect deleted ones to be sure we get all list
            //// names.
            //cursor = _provider.Query(TasksContract.UriTagdata, new[] { TasksContract.ColTagdataName, TasksContract.ColTagdataUuid },
            //                                 null, null, null);

            //if (cursor.MoveToFirst())
            //    do
            //    {
            //        var description = cursor.GetString(0);
            //        var uuid = cursor.GetString(1);

            //        var list = new OrgTasksTodoList { Id = uuid, Description = description };
            //        uuidToList.Add(uuid, list);
            //        _lists.Add(list);
            //    } while (cursor.MoveToNext());


            cursor = _provider.Query(TasksContract.UriMetadata, TasksContract.ColumnsMetadata,
                                     "key='tags-tag' and deleted=0", null, null);

            var metadata = GetMetadatas(cursor)
                .OrderBy(m => m.TagName) // order by name to get a consistent ordering all the time
                .ToList();


            Dictionary<string, TodoList> taskUuidToList = new Dictionary<string, TodoList>();
            foreach (var m in metadata)
            {
                if (!uuidToList.ContainsKey(m.TagUuid))
                {
                    //should actually not happen, but better be on the safe side.
                    var todoList = new OrgTasksTodoList
                    {
                        Id = m.TagUuid,
                        Description = m.TagName,
                    };
                    uuidToList.Add(m.TagUuid, todoList);
                    _lists.Add(todoList);
                }

                if (!taskUuidToList.ContainsKey(m.TaskUuid))
                    taskUuidToList.Add(m.TaskUuid, uuidToList[m.TagUuid]);
            }
            p.Progress = 0.2f;


            Commit mod = new Commit();
            // Add Lists.
            mod.Modified.AddRange(uuidToList.Values
                .OrderBy(l => l.Description)
                .Select(l => new Modification(l)));


            // Convert Tasks.
            cursor = _provider.Query(TasksContract.UriTask, TasksContract.ColumnsTask,
                TasksContract.ColTaskDeleted + "=0", null, null);

            if (cursor.MoveToFirst())
                do
                {
                    var task = _convert.TodoTaskFromCursor(cursor);
                    TodoList list;
                    task.ListFk = taskUuidToList.TryGetValue(task.IdSplitUuid, out list) ? list.Id 
                                                                       : _lists.Lists[0].Id;
                    mod.Modified.Add(new Modification(task));

                } while (cursor.MoveToNext());
            cursor.Close();

            return new CommitList(mod);
        }

        private IEnumerable<OrgTasksTagsTagMetadata> GetMetadatas(ICursor cursor)
        {
            if (cursor.MoveToFirst())
                do
                {
                    var ret = _convert.MetadataFromCursor(cursor);
                    yield return ret;
                } while (cursor.MoveToNext());
            cursor.Close();
        }



        public CommitList SaveModifications(CommitList inputCommitList, IProgress progress)
        {
            ContentProviderOperation.Builder build;
            List<ContentProviderOperation> ops = new List<ContentProviderOperation>();

            Commit mods = inputCommitList.Flatten();
            // delete old...
            foreach (var del in mods.Deleted.OfTypeTodoTask())
            {
                build = ContentProviderOperation.NewUpdate(TasksContract.UriTask);
                build.WithValue(TasksContract.ColTaskDeleted, del.ModifiedAt.FromUtcToMillisecondsUnixTime());
                ops.Add(build.Build());
            }

            foreach (var m in mods.Modified.OfTypeTodoTask())
            {
                var task = (TodoTask) m.Object;
                if (task.IsNew) continue; // handle new tasks later.

                string newUuid;
                var cv = _convert.ToContentValues(task, out newUuid, m.ModifiedProperties);

                build = ContentProviderOperation.NewUpdate(TasksContract.UriTask).WithValues(cv);
                ops.Add(build.Build());
            }

            // collect uuids from tasks where we will set the listfk
            List<TodoTask> updateListFk = mods.Modified.OfTypeTodoTask()
                .Where(m => m.ModifiedProperties == null || m.ModifiedProperties.Contains(TodoTask.ColListFk))
                .Select(t => t.Object)
                .Cast<TodoTask>()
                .ToList();

            ISet<string> newTaskUuids = InsertNewTasksAndUpdateTheirIds(mods);


            // make the list-update code.
            foreach (var task in updateListFk)
            {
                string uuid = OrgTasksTodoTask.GetIdSplitUuid(task);
                bool isNew = newTaskUuids.Contains(uuid);

                if (!isNew)
                {
                    // delete old tags.
                    build = ContentProviderOperation.NewDelete(TasksContract.UriMetadata)
                        .WithSelection(string.Format("{0}={1} AND {2}='tags-tag'",
                                            TasksContract.ColMetadataTask,
                                            OrgTasksTodoTask.GetIdSplitId(task), 
                                          TasksContract.ColMetadataKey), null);
                    ops.Add(build.Build());
                }
                //create new tag.
                var todoList = (OrgTasksTodoList) _lists.GetById(task.ListFk);
                if (!todoList.Description.IsNullOrEmpty())
                {
                    build = ContentProviderOperation.NewInsert(TasksContract.UriMetadata)
                        .WithValue(TasksContract.ColMetadataKey, "tags-tag")
                        .WithValue(TasksContract.ColMetadataValue, todoList.Description)
                        .WithValue(TasksContract.ColMetadataValue2, todoList.Id)
                        .WithValue(TasksContract.ColMetadataTask, task.Id)
                        .WithValue(TasksContract.ColMetadataValue3, uuid);
                    ops.Add(build.Build());
                }
            }

            // apply all operations
            var result = _provider.ApplyBatch(ops);
            if(result.Length != ops.Count)
                throw new Exception("ContentProvider did not accept all changes.");

            return inputCommitList;
        }

        private ISet<string> InsertNewTasksAndUpdateTheirIds(Commit mods)
        {
            List<ContentValues> newTasks = new List<ContentValues>();
            Dictionary<string, TodoTask> newUuidToTasks = new Dictionary<string, TodoTask>();

            // insert new tasks in a bulk operation
            foreach (var m in mods.Modified.OfTypeTodoTask().Where(t => t.Object.IsNew))
            {
                var todoTask = (TodoTask)m.Object;
                string newUuid;
                var cv = _convert.ToContentValues(todoTask, out newUuid, m.ModifiedProperties);
                
                newTasks.Add(cv);
                newUuidToTasks.Add(newUuid, todoTask);
            }

            if (newTasks.Count <= 0) return new HashSet<string>();

            _provider.BulkInsert(TasksContract.UriTask, newTasks.ToArray());
            string uuidQuery = string.Format("{0} in ({1})", TasksContract.ColTaskRemoteId,
                                                    string.Join(",", newUuidToTasks.Keys.Select(DatabaseUtils.SqlEscapeString)));
            var cursor = _provider.Query(TasksContract.UriTask, new[] {TasksContract.ColTaskRemoteId,
                                                                       TasksContract.ColId},
                                         uuidQuery, null, null);
            if (cursor.MoveToFirst())
                do
                {
                    var uuid = cursor.GetString(0);
                    var id = cursor.GetInt(1).ToStringInvariant();
                    TodoTask newTask = newUuidToTasks[uuid];
                    
                    newTask.Id = id;
                } while (cursor.MoveToNext());

            return new HashSet<string>(newUuidToTasks.Keys);
        }

        public CommitList SaveModificationsForIds(CommitList commits, IProgress progress)
        {
            foreach (TodoList newList in commits.Commits.SelectMany(o=>o.Modified)
                                              .OfTypeTodoList()
                                              .Where(p => p.Object.IsNew)
                                              .Select(p => p.Object)
                                              .Cast<TodoList>())
            {
                newList.Id = SequentialGuid.NewGuidString();
                var list = new OrgTasksTodoList
                {
                    Id = newList.Id, 
                    Description = newList.Description,
                };
                _lists.Add(list);
                
                // unfortunately, this table is not accessible.
                //ContentValues cv = new ContentValues();
                //cv.Put(TasksContract.ColTagdataName, list.Description);
                //cv.Put(TasksContract.ColTagdataUuid, list.Uuid);
                //_provider.Insert(TasksContract.UriTagdata, cv);
            }
            return commits;
        }

        public CommitList MergeModifications(CommitList myModifications, IProgress progress)
        {
            throw new NotImplementedException();
        }

    }
}
