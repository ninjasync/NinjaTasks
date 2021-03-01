using System;
using System.Collections.Generic;
using NinjaSync.Model;
using NinjaSync.Model.Journal;

namespace NinjaSync.Storage
{
    public class TrackableJournalStorageAdapter : ITrackableJournalStorage
    {
        private readonly ITrackableStorage _storage;
        private readonly IJournalStorage _journal;

        public TrackableJournalStorageAdapter(ITrackableStorage storage, IJournalStorage journal)
        {
            _storage = storage;
            _journal = journal;
        }

        public ITrackableStorage Storage { get { return _storage; } }
        public IJournalStorage Journal { get { return _journal; } }

        public string StorageId { get { return _storage.StorageId; } } 

        public ITrackable GetById(TrackableId id)
        {
            return Storage.GetById(id);
        }

        public IEnumerable<ITrackable> GetById(params TrackableId[] ids)
        {
            return Storage.GetById(ids);
        }

        public IEnumerable<TrackableId> GetIds(SelectionMode mode, TrackableType type, params string[] ids)
        {
            return Storage.GetIds(mode, type, ids);
        }

        public IEnumerable<ITrackable> GetTrackable(params TrackableType[] limitToSpecifiedTypes)
        {
            return Storage.GetTrackable(limitToSpecifiedTypes);
        }

        public void Save(ITrackable obj, ICollection<string> properties)
        {
            _storage.Save(obj, properties);
        }

        public void Delete(SelectionMode mode, params TrackableId[] id)
        {
            Storage.Delete(mode, id);
        }

        public void Clear()
        {
            _storage.RunInTransaction(() =>
            {
                _storage.Clear();
                _journal.Clear();
            });
        }


        public void RunInTransaction(Action a)
        {
            Storage.RunInTransaction(a);
        }

        public void RunInImmediateTransaction(Action a)
        {
            Storage.RunInImmediateTransaction(a);
        }

        public ITransaction BeginTransaction()
        {
            return _storage.BeginTransaction();
        }

        public string CommitChanges(string setCommitId = null, string mergeSecondBaseCommitId = null)
        {
            return Journal.CommitChanges(setCommitId, mergeSecondBaseCommitId);
        }

        public string GetLastCommitId()
        {
            return _journal.GetLastCommitId();
        }

        public IEnumerable<CommitEntry> GetCommits(string sinceButExcludingCommitId)
        {
            return Journal.GetCommits(sinceButExcludingCommitId);
        }

        public void AddOrReplaceJournalEntry(JournalEntry journalEntry)
        {
            Journal.AddOrReplaceJournalEntry(journalEntry);
        }

        public IList<string> GetCommitIdsSince(string lastCommonCommitId)
        {
            return Journal.GetCommitIdsSince(lastCommonCommitId);
        }

        public bool HasCommit(string commitId)
        {
            return Journal.HasCommit(commitId);
        }

        void IJournalStorage.Clear()
        {
            _journal.Clear();
        }

        public override string ToString()
        {
            return _storage.ToString();
        }
    }
}
