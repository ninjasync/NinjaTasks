using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTools.Progress;

namespace NinjaTasks.Model.Storage
{
    /// <summary>
    /// gives the ITrackableRemoteSlaveStorage interface to an ITrackableStorage. 
    /// When used with a local ITrackableStorage, it simulates a "dumb" remote, 
    /// that always sends all data. Usefull for testing the sync-algorithms.
    /// </summary>
    public class TodoRemoteSlaveWithFakeListsStorageWrapper : ITodoRemoteSlaveStorage
    {
        private readonly ITrackableStorage _storage;

        public TodoRemoteSlaveWithFakeListsStorageWrapper(ITrackableStorage storage)
        {
            _storage = storage;
        }

        public CommitList GetModifications(string commitsNewerButExcludingCommitId, IProgress p)
        {
            Debug.Assert(commitsNewerButExcludingCommitId == null);

            var ret = new Commit();
            CommitList retlist = new CommitList();
            retlist.Commits.Add(ret);
            
            TodoListLookup lists = new TodoListLookup(_storage);
            
            var tasks = _storage.GetTrackable(TrackableType.Task).Cast<TodoTask>();
            foreach (var task in tasks)
            {
                var t = new TodoTaskWithListName(task, lists.GetById(task.ListFk).Description);
                ret.Modified.Add(new Modification(t));
            }

            p.Progress = 1;
            return retlist;
        }

        public CommitList SaveModifications(CommitList list, IProgress progress)
        {
            Commit commit = list.Flatten();
            _storage.Delete(SelectionMode.SelectSpecified, commit.Deleted.Select(p => p.Key).ToArray());

            TodoListLookup lists = new TodoListLookup(_storage);
            foreach (var m in commit.Modified)
            {
                if (m.ObjectType != TrackableType.Task)
                    _storage.Save(m.Object, m.ModifiedProperties);
                else
                {
                    TodoTaskWithListName t = (TodoTaskWithListName)m.Object;
                    var localList = lists.GetByName(t.ListName);
                    if (localList == null)
                    {
                        localList = new TodoList {Description = t.ListName};
                        _storage.Save(localList);
                        lists.Add(localList);
                    }

                    t.ListFk = localList.Id;
                    _storage.Save(t, m.ModifiedProperties);
                }
            }

            return list;
        }

        public CommitList SaveModificationsForIds(CommitList commits, IProgress p)
        {
            SaveModifications(commits, p);
            return new CommitList();
        }

        public TrackableRemoteStorageType StorageType { get { return TrackableRemoteStorageType.HasOnlyImplicitLists; } }
        public TrackableType[] SupportedTypes { get { return new[] { TrackableType.List, TrackableType.Task }; } }
        public IList<string> GetMappedColums(TrackableType type) { return null; }
    }
}
