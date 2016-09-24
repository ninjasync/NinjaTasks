using System;
using System.Collections.Generic;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTasks.Model;
using NinjaTools.Progress;

namespace NinjaTasks.Sync
{
    public class IdMappingTodoRemoteSlaveStorageAdapter : ITodoRemoteSlaveStorage
    {
        private readonly ITodoRemoteSlaveStorage _remote;
        private readonly NinjaTasksIdMapper _mapper;

        public TrackableRemoteStorageType StorageType { get { return _remote.StorageType&~TrackableRemoteStorageType.RequiresIdMapping; } }
        public TrackableType[] SupportedTypes { get { return _remote.SupportedTypes; } }
        public IList<string> GetMappedColums(TrackableType type) { return _remote.GetMappedColums(type); }

        public IdMappingTodoRemoteSlaveStorageAdapter(ITodoRemoteSlaveStorage remote, IStringMappingStorage mapper, bool doNotMapLists)
        {
            _remote = remote;
            _mapper = new NinjaTasksIdMapper(mapper, doNotMapLists);

        }

        public CommitList GetModifications(string commitsNewerButExcludingCommitId, IProgress progress)
        {
            var remoteCommmits = _remote.GetModifications(commitsNewerButExcludingCommitId, progress);
            return _mapper.TranslateRemoteToLocal(remoteCommmits);
        }

        public CommitList SaveModifications(CommitList list, IProgress progress)
        {
            var localMappedCommits = _mapper.TranslateLocalToRemote(list, _remote.SaveModificationsForIds);
            var ret = _remote.SaveModifications(localMappedCommits, progress);
            _mapper.CommitLocalToRemote();
            return _mapper.TranslateRemoteToLocal(ret);
        }

        public CommitList SaveModificationsForIds(CommitList commits, IProgress progress)
        {
            throw new NotImplementedException("should not be called here.");
        }
    }
}
