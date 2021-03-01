using System;
using System.Collections.Generic;
using System.Linq;
using NinjaSync.Model.Journal;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools;

namespace NinjaTasks.Sync
{
    /// <summary>
    /// Some remotes do not support first class lists, so they cannot provide
    /// a reliable ListFk for tasks. 
    /// <para/>
    /// This class will remove all changes to lists in the commit, and replace 
    /// all 'TodoTask' with 'TodoTaskWithListName'; and do the reverse as well.
    /// </summary>
    public class NinjaTasksListMapping// : ICommitListMapper
    {
        private readonly ITodoStorage _local;

        public NinjaTasksListMapping(ITodoStorage local)
        {
            _local = local;
        }

        /// <summary>
        /// makes sure the remote has all mentioned lists at its hand.
        /// <para/>
        /// will modify the original list an return it updated.
        /// </summary>
        /// <param name="localList"></param>
        /// <param name="lists"></param>
        /// <returns></returns>
        public CommitList TranslateLocalToRemote(CommitList localList, TodoListLookup lists=null)
        {
            if (localList.Commits.Count == 0) return localList;

            if(lists == null)
                lists = new TodoListLookup();

            // collect first commit index where an object has a full change, i.e. was newly created.
            Dictionary<string, int> fullChangeAtCommitIdx = new Dictionary<string, int>();
            for (int idx = localList.Commits.Count-1; idx >= 0; --idx)
            {
                foreach (var mod in localList.Commits[idx].Modified.OfTypeTodoTask()
                    .Where(p => p.ModifiedProperties == null))
                    fullChangeAtCommitIdx[mod.Object.Id] = idx;
            }

            

            // if a list property has changes (e.g. renamed), 
            // convert the change to an ListFk change.
            for (int idx = 0; idx < localList.Commits.Count; ++idx)
            {
                Commit commit = localList.Commits[idx];

                TransformListDescriptionChangeToTask_ListFkChange(commit, idx, fullChangeAtCommitIdx);

                // remove lists.
                commit.Deleted = commit.Deleted.Where(p => p.ObjectType != TrackableType.List).ToList();
                
                foreach(var mod in commit.Modified.OfTypeTodoList())
                    lists.Add((TodoList) mod.Object);

                commit.Modified = commit.Modified.Where(p => p.ObjectType != TrackableType.List).ToList();

                // transform all tasks
                for (int i = 0; i < commit.Modified.Count; ++i)
                {
                    var mod = commit.Modified[i];
                    if (mod.ObjectType != TrackableType.Task) continue;

                    TodoTask task = (TodoTask) mod.Object;

                    TodoList list = lists.GetById(task.ListFk);
                    if (list == null)
                    {
                        list = _local.GetLists(task.ListFk).FirstOrDefault();
                        if(list == null)
                            throw new Exception("could not find list " + task.ListFk);
                    }

                    var newTask = new TodoTaskWithListName(task, list.Description);

                    mod = new Modification(newTask, mod.ModifiedPropertiesEx) {ModifiedAt = mod.ModifiedAt};

                    if(mod.ModifiedProperties != null && mod.ModifiedProperties.Contains(TodoTask.ColListFk))
                        mod.ModifiedPropertiesEx.Add(new ModifiedProperty(TodoTaskWithListName.ColListName, mod.ModifiedPropertiesEx.First(m=>m.Property==TodoTask.ColListFk).ModifiedAt));
                    
                    commit.Modified[i] = mod;
                }
            }

            return localList;
        }

        /// <summary>
        /// translate a remote commit list to a local one, reconstructing 
        /// list fks from list names. Non existent lists will be recorded
        /// as new on-the-fly.
        /// <para/>
        /// will modify the original commit list and return it updated.
        /// </summary>
        /// <returns></returns>
        public CommitList TranslateRemoteToLocal(CommitList commits, TodoListLookup localLists=null)
        {
            if(localLists == null)
                localLists = new TodoListLookup(_local);

            List<TodoList> newLists = new List<TodoList>();

            foreach (var remote in commits.Commits)
            {
                foreach (var mod in remote.Modified)
                {
                    if (mod.ObjectType == TrackableType.Task)
                    {
                        var remoteTask = mod.Object as TodoTaskWithListName;
                        if (remoteTask == null)
                        {
                            if(((TodoTask)mod.Object).ListFk.IsNullOrEmpty())
                                throw new Exception("remote should provide ListFk or TodoTaskWithListName");
                            continue;
                        }

                        var list = localLists.GetByName(remoteTask.ListName);

                        if (list == null)
                        {
                            list = new TodoList {Description = remoteTask.ListName, CreatedAt = DateTime.UtcNow};
                            list.SetNewId();
                            // insert at beginning.
                            newLists.Add(list);
                            localLists.Add(list);
                        }

                        remoteTask.ListFk = list.Id;
                    }
                }

                remote.Modified.InsertRange(0, newLists.Select(l=>new Modification(l)));
            }

            if (commits.BasedOnCommitId.IsNullOrEmpty())
            {
                // this is a full sync. make sure our local lists don't get deleted.
                commits.Commits[0].Modified.AddRange(localLists.Lists.Select(l=>new Modification(l)));
            }
            return commits;
        }

        private void TransformListDescriptionChangeToTask_ListFkChange(Commit changes, int commitIdx, Dictionary<string, int> tasksFullChangeAtCommitIdx)
        {
            var deleted = changes.Deleted.ToDictionary(p => p.Key);
            var modified = changes.Modified.ToDictionary(p => new TrackableId(p.Object));

            for (int i = 0; i < changes.Modified.Count; i++)
            {
                var mod = changes.Modified[i];
                // only step in if Description of list was changed.
                if (mod.Object.TrackableType != TrackableType.List 
                    || (mod.ModifiedProperties != null && !mod.ModifiedProperties.Contains(TodoList.ColDescription)))
                    continue;

                foreach (var task in _local.GetTasks((TodoList)mod.Object))
                {
                    var key = new TrackableId(task);
                    if (deleted.ContainsKey(key)) continue; // can this happen?

                    // ignore tasks that have been created later than the list change.
                    int fullChangeAtIdx;
                    if (tasksFullChangeAtCommitIdx.TryGetValue(task.Id, out fullChangeAtIdx) 
                        && fullChangeAtIdx > commitIdx)
                        continue;


                    Modification modTask;
                    if (modified.TryGetValue(key, out modTask))
                    {
                        // change exists. Add "ListFk"-Change.
                        if (modTask.ModifiedPropertiesEx != null)
                            modTask.ModifiedPropertiesEx.Add(new ModifiedProperty(TodoTask.ColListFk, mod.ModifiedAt));
                        modTask.ModifiedAt = Max(modTask.ModifiedAt, mod.ModifiedAt);
                    }
                    else
                    {
                        // add new
                        modTask = new Modification(task, new List<ModifiedProperty> { new ModifiedProperty(TodoTask.ColListFk, mod.ModifiedAt) });
                        modTask.ModifiedAt = mod.ModifiedAt;
                        changes.Modified.Add(modTask);
                    }
                }
            }
        }

        private DateTime Max(DateTime ts1, DateTime ts2)
        {
            return ts1 > ts2 ? ts1 : ts2;
        }
    }
}
