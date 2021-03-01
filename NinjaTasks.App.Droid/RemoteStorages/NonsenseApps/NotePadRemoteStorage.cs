using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Android.Content;
using Android.Database;
using NinjaSync.Exceptions;
using NinjaSync.Model.Journal;
using NinjaTasks.Model;
using NinjaTools;
using NinjaTools.Logging;
using NinjaTools.Progress;
using Exception = System.Exception;

namespace NinjaTasks.App.Droid.RemoteStorages.NonsenseApps
{
    public class NotePadRemoteStorage : ITodoRemoteSlaveStorage
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private const string MyAccountType = "org.ninjatasks.account";

        private readonly ContentProviderClient _provider;
        private readonly NpConverter _convert = new NpConverter();
        private readonly ContentTable _tblTask;
        private readonly ContentTable _tblList;
        private readonly ContentTable _tblRemoteList;
        private readonly ContentTable _tblRemoteTask;

        private readonly static object Sync = new object();

        public NotePadRemoteStorage(ContentProviderClient provider)
        {
            _provider = provider;
            _tblList = new ContentTable(provider, NpContract.UriTaskList, NpContract.ColId, false);
            _tblTask = new ContentTable(provider, NpContract.UriTask, NpContract.ColId, true);
            _tblRemoteList = new ContentTable(provider, NpContract.UriRemoteTaskList, NpContract.ColId, false);
            _tblRemoteTask = new ContentTable(provider, NpContract.UriRemoteTask, NpContract.ColId, false);
        }

        public TrackableRemoteStorageType StorageType
        {
            get { return TrackableRemoteStorageType.RequiresSortPositionMapping; }
        }

        public TrackableType[] SupportedTypes { get { return new[] { TrackableType.List, TrackableType.Task }; } }

        public IList<string> GetMappedColums(TrackableType type)
        {
            if(type == TrackableType.List)
                return new[]
                {
                    TodoList.ColId,TodoList.ColDescription, TodoList.ColModifiedAt,
                    // TodoList.ColCreatedAt, // does'nt support created at.
                };
            if(type == TrackableType.Task)
                return new[]
                {
                    TodoTask.ColId, TodoTask.ColCompletedAt, TodoTask.ColDescription, 
                    TodoTask.ColListFk, TodoTask.ColModifiedAt,TodoTask.ColStatus,
                    //TodoTask.ColSortPosition,
                    // TodoTask.ColCreatedAt, // does'nt support created at.
                };
            return new string[0];
        }

        public CommitList GetModifications(string commitsNewerButExcludingCommitId, IProgress p)
        {
            lock (Sync)
            {
                int syncNum = commitsNewerButExcludingCommitId == null ? 0 : int.Parse(commitsNewerButExcludingCommitId);

                if (syncNum != 0 && IsFreshDatabase())
                    throw new CommitNotFoundException(commitsNewerButExcludingCommitId);

                List<TodoList> lists;
                List<TodoTask> tasks;

                List<TrackableId> deleted = new List<TrackableId>();
                GetListAndTasks(out lists, out tasks, deleted);

                Commit ret = new Commit();

                ret.Modified.AddRange(lists.Select(l => new Modification(l)));
                ret.Modified.AddRange(tasks.Select(l => new Modification(l)));
                ret.Deleted.AddRange(deleted.Select(d => new Modification(d, DateTime.UtcNow)));

                bool wasAndIsFresh = syncNum == 0 && ret.IsEmpty;

                ret.BasedOnCommitId = commitsNewerButExcludingCommitId;
                ret.CommitId = wasAndIsFresh ? null : (++syncNum).ToStringInvariant();

                return new CommitList(ret) {RemoteCommitId = ret.CommitId};
            }
        }

        private bool IsFreshDatabase()
        {
            return _tblRemoteTask.IsEmpty() && _tblRemoteList.IsEmpty();
        }

        public CommitList SaveModifications(CommitList inputCommitList, IProgress progress)
        {
            lock (Sync)
            {
                Commit commit = inputCommitList.Flatten();

                var deletedNinjaListIds =
                    new HashSet<string>(commit.Deleted.OfTypeTodoList().Select(p => p.Key.ObjectId));
                var deletedNinjaTaskIds =
                    new HashSet<string>(commit.Deleted.OfTypeTodoTask().Select(p => p.Key.ObjectId));

                List<string> modifiedNinjaTaskListFks = commit.Modified.OfTypeTodoTask()
                    .Select(p => ((TodoTask) p.Object).ListFk)
                    .ToList();

                var idMapLists = GetRemoteIds(true, commit.Modified.OfTypeTodoList()
                    .Where(p => !p.Object.IsNew)
                    .Select(p => p.Object.Id)
                    .Concat(deletedNinjaListIds)
                    .Concat(modifiedNinjaTaskListFks)
                    )
                    .ToDictionary(p => p.Value, p => p.Key);
                var idMapTasks = GetRemoteIds(false, commit.Modified.OfTypeTodoTask()
                    .Where(p => !p.Object.IsNew)
                    .Select(p => p.Object.Id)
                    .Concat(deletedNinjaTaskIds)
                    )
                    .ToDictionary(p => p.Value, p => p.Key);

                // delete only existing remote list / tasks.
                deletedNinjaListIds.IntersectWith(idMapLists.Keys);
                long[] deleteIds = deletedNinjaListIds.Select(p => idMapLists[p]).ToArray();
                _tblList.DeleteByIds(deleteIds);

                deletedNinjaTaskIds.IntersectWith(idMapTasks.Keys);
                deleteIds = deletedNinjaTaskIds.Select(p => idMapTasks[p]).ToArray();
                _tblTask.DeleteByIds(deleteIds);

                bool hasListInserts = false;
                ContentProviderOperation.Builder build;
                List<ContentProviderOperation> ops = new List<ContentProviderOperation>();

                foreach (var m in commit.Modified.OfTypeTodoList())
                {
                    long notepadId;
                    var todoList = (TodoList) m.Object;

                    if (!idMapLists.TryGetValue(m.Object.Id, out notepadId))
                    {
                        // new object
                        Debug.Assert(!m.Object.Id.IsNullOrEmpty());

                        var cv = _convert.ToContentValues(todoList);
                        build = ContentProviderOperation.NewInsert(_tblList.Uri);
                        build.WithValues(cv);
                        ops.Add(build.Build());

                        // create remote mapping as well.
                        build = ContentProviderOperation.NewInsert(_tblRemoteList.Uri);
                        build.WithValues(CreateNewListIdCv(m.Object.Id, 0));
                        build.WithValueBackReference(NpContract.ColRemoteDbId, ops.Count - 1);
                        build.WithValueBackReference(NpContract.ColRemoteField5, ops.Count - 1);
                        build.WithYieldAllowed(true);
                        ops.Add(build.Build());
                        hasListInserts = true;
                    }
                    else
                    {
                        // existing object.
                        var cv = _convert.ToContentValues(todoList);
                        if (cv.Size() == 0) continue;
                        build = ContentProviderOperation.NewUpdate(_tblList.GetUpdateUri(notepadId));
                        build.WithValues(cv);
                        build.WithYieldAllowed(true);
                        ops.Add(build.Build());
                    }
                }

                if (hasListInserts)
                {
                    // we need the list-ids, to save everything now.
                    _provider.ApplyBatch(ops);
                    ops.Clear();

                    idMapLists = GetRemoteIds(true, modifiedNinjaTaskListFks)
                        .ToDictionary(p => p.Value, p => p.Key);

                }

                // now for the tasks.
                foreach (var m in commit.Modified.OfTypeTodoTask())
                {
                    long notepadId, notepadListDbId;
                    var todoTask = (TodoTask) m.Object;

                    if (!idMapLists.TryGetValue(todoTask.ListFk, out notepadListDbId))
                    {
                        string msg = string.Format("unable to determine dblistid for task {0} listfk {1}", todoTask.Id,
                            todoTask.ListFk);
                        Log.Error(msg);
                        throw new Exception(msg);
                    }

                    if (!idMapTasks.TryGetValue(m.Object.Id, out notepadId))
                    {
                        // new object
                        Debug.Assert(!m.Object.Id.IsNullOrEmpty());

                        var cv = _convert.ToContentValues(todoTask, true, notepadListDbId);
                        build = ContentProviderOperation.NewInsert(_tblTask.Uri);
                        build.WithValues(cv);
                        ops.Add(build.Build());

                        // create remote mapping as well.
                        build = ContentProviderOperation.NewInsert(_tblRemoteTask.Uri);
                        build.WithValues(CreateNewTaskIdCv(m.Object.Id, 0));
                        build.WithValueBackReference(NpContract.ColRemoteDbId, ops.Count - 1);
                        build.WithValueBackReference(NpContract.ColRemoteField5, ops.Count - 1);
                        build.WithYieldAllowed(true);
                        ops.Add(build.Build());
                    }
                    else
                    {
                        // existing object.
                        var cv = _convert.ToContentValues(todoTask, false, notepadListDbId, m.ModifiedProperties);
                        if (cv.Size() == 0) continue;

                        // for the tasks-table and an update where we only set some fields
                        // we need a selection!
                        build = ContentProviderOperation.NewUpdate(_tblTask.Uri);
                        build.WithValues(cv);
                        build.WithSelection(NpContract.ColId + "=?", new[] {notepadId.ToStringInvariant()});
                        build.WithYieldAllowed(true);
                        ops.Add(build.Build());
                    }
                }

                if (ops.Count == 0 && idMapLists.Count == 0 && idMapTasks.Count == 0 && IsFreshDatabase())
                {
                    // create a dummy entry.
                    var cv = CreateNewTaskIdCv("0000", 0);
                    _tblRemoteTask.InsertOrUpdate(cv, 0);
                }

                if (ops.Count > 0)
                    _provider.ApplyBatch(ops);
            }

            return inputCommitList;
        }

        public CommitList SaveModificationsForIds(CommitList commits, IProgress p)
        {
            SaveModifications(commits, p);
            return new CommitList();
        }

        private void GetListAndTasks(out List<TodoList> lists, out List<TodoTask> tasks, List<TrackableId> deletedObjects=null)
        {
            List<string> deletedListIds= new List<string>();
            List<string> deletedTaskIds = new List<string>();

            var idMapLists = GetRemoteIds(true, null, deletedListIds);
            var idMapTasks = GetRemoteIds(false, null, deletedTaskIds);

            if (deletedObjects != null)
            {
                deletedObjects.AddRange(deletedListIds.Select(id => new TrackableId(TrackableType.List, id)));
                deletedObjects.AddRange(deletedTaskIds.Select(id => new TrackableId(TrackableType.Task, id)));
            }

            lists = new List<TodoList>();
            tasks = new List<TodoTask>();

            List<ContentValues> cvs = new List<ContentValues>();

            // Lists First
            ICursor c = _tblList.QueryAll(NpContract.ColumnsTaskList);

            while (c.MoveToNext())
            {
                var list = _convert.TodoListFromCursor(c);
                SetNinjaId(list, idMapLists, cvs, CreateNewListIdCv);
                lists.Add(list);
            }
            c.Close();

            if (cvs.Count > 0)
                _provider.BulkInsert(_tblRemoteList.Uri, cvs.ToArray());
            cvs.Clear();

            // now tasks.
            c = _tblTask.QueryAll(NpContract.ColumnsTask);

            while (c.MoveToNext())
            {
                var task = _convert.TodoTaskFromCursor(c);

                // translate ListFk
                string ninjaListId;
                int notepadListDbId = int.Parse(task.ListFk);
                if (!idMapLists.TryGetValue(notepadListDbId, out ninjaListId))
                {
                    Log.Error("unable to determine ninja list id for tasks {0} (listdbid={1})", task.Id, task.ListFk);
                    continue;
                }
                task.ListFk = ninjaListId;

                SetNinjaId(task, idMapTasks, cvs, CreateNewTaskIdCv);
                tasks.Add(task);
            }
            c.Close();

            if (cvs.Count > 0)
                _provider.BulkInsert(_tblRemoteTask.Uri, cvs.ToArray());
        }

        private Dictionary<long, string> GetRemoteIds(bool forLists, IEnumerable<string> limitToTheseNinjaIds = null, List<string> nonexistentIds=null)
        {
            Dictionary<long, string> idMap = new Dictionary<long, string>();
            // get all remote tasks so we know our ids.

            string query = string.Format("{0}='{1}'", NpContract.ColRemoteAccount, MyAccountType);

            if (limitToTheseNinjaIds != null)
            {
                string inClause =  string.Join(",", limitToTheseNinjaIds.Select(DatabaseUtils.SqlEscapeString));
                
                if (inClause.IsNullOrEmpty())  // empty clausewill give empty result.
                    return idMap;
                if(inClause.Length < 8000)
                    query += string.Format(" AND {0} in ({1})", NpContract.ColRemoteRemoteId, inClause);
            }

            ContentTable table = forLists ? _tblRemoteList: _tblRemoteTask;

            // query the ids. We use 'field5' since notepad sets the 'dbid' to -99 if a taskis moved
            // to another list for some unknown reason
            ICursor c = _provider.Query(table.Uri, new[] { NpContract.ColRemoteField5, NpContract.ColRemoteRemoteId },
                                        query, null, null);
            while (c.MoveToNext())
                idMap.Add(c.GetLong(0), c.GetString(1));
            c.Close();

            // remove the dummy-entry if it was created.
            idMap.Remove(0);

            // only return a mapping for objects that acually exist.
            ContentTable tableObjs = forLists ? _tblList: _tblTask;
            HashSet<long> nonFoundObjects = new HashSet<long>(idMap.Keys);

            c = tableObjs.QueryByIds(new[] { NpContract.ColId }, idMap.Keys.ToArray());
            while (c.MoveToNext())
                nonFoundObjects.Remove(c.GetLong(0));
            c.Close();

            if (nonFoundObjects.Count > 0)
            {
                if(Log.IsWarnEnabled)
                    Log.Warn("found stale remote id mappings on {0}: {1}", table.Uri, string.Join(",", nonFoundObjects));
                foreach (var remove in nonFoundObjects)
                {
                    if(nonexistentIds != null)
                        nonexistentIds.Add(idMap[remove]);

                    idMap.Remove(remove);
                }
            }

            return idMap;
        }

        private static void SetNinjaId(ITrackable obj, Dictionary<long, string> idMap, List<ContentValues> cvs, Func<string, long, ContentValues> createNewCv)
        {
            long notepadId = long.Parse(obj.Id);

            string ninjaId;

            if (!idMap.TryGetValue(notepadId, out ninjaId))
            {
                // create a new id.
                ninjaId = SequentialGuid.NewGuidString();
                var cv = createNewCv(ninjaId, notepadId);
                cvs.Add(cv);
            }
            
            obj.Id = ninjaId;
        }

        private static ContentValues CreateNewListIdCv(string ninjaId, long notepadId)
        {
            var cv = new ContentValues();
            cv.Put(NpContract.ColRemoteAccount, MyAccountType);
            cv.Put(NpContract.ColRemoteService, "");
            cv.Put(NpContract.ColRemoteRemoteId, ninjaId);
            cv.Put(NpContract.ColRemoteUpdated, 0);
            if (notepadId != 0)
            {
                cv.Put(NpContract.ColRemoteDbId, notepadId);
                cv.Put(NpContract.ColRemoteField5, notepadId);
            }
            return cv;
        }

        private static ContentValues CreateNewTaskIdCv(string ninjaId, long notepadId)
        {
            var cv = new ContentValues();
            cv.Put(NpContract.ColRemoteAccount, MyAccountType);
            cv.Put(NpContract.ColRemoteService, "");
            cv.Put(NpContract.ColRemoteRemoteId, ninjaId);
            cv.Put(NpContract.ColRemoteUpdated, 0);
            if (notepadId != 0)
            {
                cv.Put(NpContract.ColRemoteDbId, notepadId);
                cv.Put(NpContract.ColRemoteField5, notepadId);
            }
            cv.Put(NpContract.ColRemoteTask_ListDbId, 0);
            return cv;
        }
    }
}
