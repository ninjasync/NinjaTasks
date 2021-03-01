using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NinjaSync.Model;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;
using NinjaTools.Logging;

namespace NinjaSync.Journaling
{
    /// <summary>
    /// Assembles a Commit, with all changes since a specific 
    /// database state.
    /// </summary>
    public class ModificationAssembler : IModificationAssembler
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ITrackableStorage _storage;
        private readonly IJournalStorage _journalStorage;

        public ModificationAssembler(ITrackableJournalStorage storage)
        {
            _storage = storage;
            _journalStorage = storage;
        }

        //public ModificationAssembler(ITrackableStorage storage, IJournalStorage journalStorage)
        //{
        //    _storage = storage;
        //    _journalStorage = journalStorage;
        //}

        //public Commit GetModified(string sinceButExcludingCommitId)
        //{
        //    List<CommitEntry> trackings=null;
        //    string lastCommitId=null;

        //    _storage.RunInTransaction(() =>
        //    {
        //        lastCommitId = _journalStorage.CommitChanges();
        //        trackings = _journalStorage.GetCommits(sinceButExcludingCommitId).ToList();
        //    });

        //    var objectCache = new Dictionary<TrackableId, ITrackable>();
            
        //    Commit ret = ToModificationList(trackings.SelectMany(p=>p.JournalEntries).ToList(), 
        //                                    objectCache);


        //    ret.BasedOnCommitId = sinceButExcludingCommitId;
        //    ret.CommitId = lastCommitId;

        //    return ret;
        //}

        public CommitList GetCommits(string sinceButExcludingCommitId)
        {
            CommitList ret = new CommitList {StorageId = _storage.StorageId};

            List<CommitEntry> commits = null;
            string finalCommitId = null;

            _storage.RunInTransaction(() =>
            {
                finalCommitId = _journalStorage.CommitChanges();
                commits = _journalStorage.GetCommits(sinceButExcludingCommitId).ToList();
            });

            HashSet<TrackableId> deleted = new HashSet<TrackableId>(
                                    commits.SelectMany(p=>p.JournalEntries)
                                           .Where(p => p.Change == ChangeType.Deleted)
                                           .Select(d => new TrackableId(d.Type, d.ObjectId)));

            var objectCache = new Dictionary<TrackableId, ITrackable>();

            foreach (var commitEntry in commits)
            {
                Commit commit = ToModificationList(commitEntry.JournalEntries, objectCache, deleted);

                commit.CommitId = commitEntry.CommitId;
                commit.BasedOnCommitId = commitEntry.BasedOnCommitId;
                commit.BasedOnCommitId2 = commitEntry.MergeSecondCommitId;
                
                ret.Commits.Add(commit);
            }

            // record the statistics.
            if (ret.Commits.Count == 0)
            {
                Debug.Assert(sinceButExcludingCommitId == finalCommitId);
                ret = CommitList.CreateDummy(finalCommitId, _storage.StorageId);
            }
            return ret;
        }

        private Commit ToModificationList(IList<JournalEntry> entries, Dictionary<TrackableId, ITrackable> objectCache, ISet<TrackableId> allDeleted=null)
        {
            Commit ret = new Commit();

            var deleted = entries.Where(p => p.Change == ChangeType.Deleted)
                                 .Select(d => new {Key = new TrackableId(d.Type, d.ObjectId), d.Timestamp})
                                 .ToList();

            if(allDeleted==null)
                allDeleted = new HashSet<TrackableId>(deleted.Select(d=>d.Key));
            
            // select created, but join in modified for max of ModifiedAt
            var created = (from creat in entries
                           let id = new TrackableId(creat.Type, creat.ObjectId)    
                           where creat.Change == ChangeType.Created && !allDeleted.Contains(id)
                           join mod in entries on id equals new TrackableId(mod.Type, mod.ObjectId) into mods
                                from m in mods.DefaultIfEmpty()   // left outer join
                           group new { CreatedAt=creat.Timestamp, mods} by id
                           into g
                           select new { g.Key, ModifiedAt = new[]{ g.First().CreatedAt}
                                                                    .Concat(g.SelectMany(k=>k.mods)
                                                                             .Select(k=>k.Timestamp))
                                                                    .Max()}
                          ).ToDictionary(p=>p.Key);
                
                //entries.Where(p => p.Change == ChangeType.Created && ))
                //                   .ToLookup(p=>new TrackableId(p.Type, p.ObjectId));

            var modified = (from t in entries
                            let id = new TrackableId(t.Type, t.ObjectId)
                            where t.Change == ChangeType.Modified && !created.ContainsKey(id) && !allDeleted.Contains(id)
                            group new { t.Member, t.Timestamp} by id
                            into g
                            select new {g.Key, Members = g.Select(p=>new ModifiedProperty(p.Member, p.Timestamp)).ToList(), 
                                                                  Timestamp=g.Max(p=>p.Timestamp)}
                           ).ToList();

            ret.Deleted = deleted.Select(m=>new Modification(m.Key, m.Timestamp))
                                 .ToList();

            ret.Modified = modified.Select(
                                        m =>new Modification(LoadObject(m.Key, objectCache), m.Members)
                                                             { ModifiedAt = m.Timestamp })
                                    .Concat(created.Select(
                                        m => new Modification(LoadObject(m.Key, objectCache))
                                                             { ModifiedAt = m.Value.ModifiedAt }))
                                    .Where(m=>m.Object != null)
                                    .ToList();

            return ret;
        }

        private ITrackable LoadObject(TrackableId key, Dictionary<TrackableId, ITrackable> objectCache)
        {
            ITrackable obj;

            if (objectCache.TryGetValue(key, out obj))
                return obj;
            
            obj = _storage.GetById(key);

            if (obj == null)
            {
                var msg = "there must have been some error in some previous code," +
                          " modified object doesn't exist any more: " + key;
                Log.Error(msg);
                Debug.Assert(false, msg);
            }

            return obj;
        }
    }
}
