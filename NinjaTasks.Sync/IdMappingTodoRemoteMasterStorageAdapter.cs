using System;
using System.Collections.Generic;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTasks.Model;
using NinjaTools.Progress;

namespace NinjaTasks.Sync
{
    public class IdMappingTodoRemoteMasterStorageAdapter : ITodoRemoteMasterStorage
    {
        private readonly ITodoRemoteMasterStorage _remote;
        private readonly NinjaTasksIdMapper _mapper;

        public TrackableRemoteStorageType StorageType { get { return _remote.StorageType&~TrackableRemoteStorageType.RequiresIdMapping; } }
        public TrackableType[] SupportedTypes { get { return _remote.SupportedTypes; } }
        public IList<string> GetMappedColums(TrackableType type) { return _remote.GetMappedColums(type); }

        public IdMappingTodoRemoteMasterStorageAdapter(ITodoRemoteMasterStorage remote, IStringMappingStorage mapper, bool doNotMapLists)
        {
            _remote = remote;
            _mapper = new NinjaTasksIdMapper(mapper, doNotMapLists);

        }

        public CommitList MergeModifications(CommitList myModifications, IProgress progress)
        {
            var localMappedCommits = _mapper.TranslateLocalToRemote(myModifications, _remote.SaveModificationsForIds);
            
            var remoteCommmits = _remote.MergeModifications(localMappedCommits, progress);
            
            _mapper.CommitLocalToRemote();

            return _mapper.TranslateRemoteToLocal(remoteCommmits);
        }

        public CommitList SaveModificationsForIds(CommitList commits, IProgress progress)
        {
            throw new NotImplementedException("should not be called here.");
        }
    }
}
