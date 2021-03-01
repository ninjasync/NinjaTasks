using System.Collections.Generic;
using System.Linq;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;
using NinjaTools.Progress;

namespace NinjaTasks.Sync
{
    /// <summary>
    /// this class helps with remotes that do not have a first-class notion of TodoLists.
    /// (e.g. TaskWarrior).
    /// it translates all 'TodoTask' in 'TodoTasksWithListName'. Changes to 'Description' 
    /// of TodoLists are propagated to all tasks.
    /// </summary>
    public class ListMappingTodoRemoteSlaveStorageAdapter : ITodoRemoteSlaveStorage
    {
        private readonly ITodoRemoteSlaveStorage _remote;
        private readonly ITodoStorage _local;
        private readonly NinjaTasksListMapping _listMapping;

        public TrackableRemoteStorageType StorageType { get { return _remote.StorageType&~TrackableRemoteStorageType.HasOnlyImplicitLists;}}
        public TrackableType[] SupportedTypes { get { return new[] {TrackableType.List, TrackableType.Task}; } }

        public ListMappingTodoRemoteSlaveStorageAdapter(ITodoRemoteSlaveStorage remote, ITodoStorage local)
        {
            _remote = remote;
            _local = local;
            _listMapping = new NinjaTasksListMapping(local);
        }

        public IList<string> GetMappedColums(TrackableType type)
        {
            return _remote.GetMappedColums(type);
        }


        public CommitList GetModifications(string commitsNewerButExcludingCommitId, IProgress progress)
        {
            var remoteCommmits = _remote.GetModifications(commitsNewerButExcludingCommitId, progress);
            return _listMapping.TranslateRemoteToLocal(remoteCommmits);
        }

        public CommitList SaveModifications(CommitList list, IProgress progress)
        {
            var lists = new TodoListLookup(_local);
            var localMappedCommits = _listMapping.TranslateLocalToRemote(list, lists);
            var ret = _remote.SaveModifications(localMappedCommits, progress);
            return _listMapping.TranslateRemoteToLocal(ret, lists);
        }

        public CommitList SaveModificationsForIds(CommitList commits, IProgress progress)
        {
            // NOTE: this could be problematic if we ever need SaveModificationsForIds 
            //       with any other type than TodoLists, and Id-Mapping is acutally
            //       required.
            foreach(var mod in commits.Commits.SelectMany(m=>m.Modified)
            //                                  .OfTypeTodoList()
                                              .Where(m=>m.Object.IsNew))
                mod.Object.SetNewId();

            return commits;
        }
    }
}
