using System.Collections.Generic;
using System.Linq;
using NinjaSync.Model.Journal;
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
    public class ListMappingTodoRemoteMasterStorageAdapter : ITodoRemoteMasterStorage
    {
        private readonly ITodoRemoteMasterStorage _mapped;
        private readonly NinjaTasksListMapping _listMapping;

        public TrackableRemoteStorageType StorageType { get { return _mapped.StorageType&~TrackableRemoteStorageType.HasOnlyImplicitLists;}}
        public TrackableType[] SupportedTypes { get { return new[] {TrackableType.List, TrackableType.Task}; } }

        public ListMappingTodoRemoteMasterStorageAdapter(ITodoRemoteMasterStorage mapped, ITodoStorage local)
        {
            _mapped = mapped;
            _listMapping = new NinjaTasksListMapping(local);
        }

        public IList<string> GetMappedColums(TrackableType type)
        {
            return _mapped.GetMappedColums(type);
        }

        public CommitList MergeModifications(CommitList myModifications, IProgress progress)
        {
            var localMappedCommits = _listMapping.TranslateLocalToRemote(myModifications);
            var remoteCommmits = _mapped.MergeModifications(localMappedCommits, progress);
            return _listMapping.TranslateRemoteToLocal(remoteCommmits);
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
