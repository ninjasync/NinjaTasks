using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using NinjaSync.Exceptions;
using NinjaSync.Model;
using NinjaTools;

namespace NinjaSync.Storage.MvxSqlite
{
    /// <summary>
    /// Note: we use generics to allow us to specify the table names. This is not so pretty.
    /// </summary>
    public class MvxSqliteJournalStorage : IJournalStorage
    {
        private readonly ISQLiteConnection _connection;

        private readonly SqliteExpressionBuilder _commits;
        private readonly SqliteExpressionBuilder _journal;

        public MvxSqliteJournalStorage(ISQLiteConnection con, string tablePrefix = null)
        {
            _connection = con;

            var journalMapping = _connection.GetMapping<JournalEntry>();
            var commitMapping = _connection.GetMapping<CommitEntry>();

            var journalTable = tablePrefix + journalMapping.TableName;
            var commitTable = tablePrefix + commitMapping.TableName;

            _connection.EnsureTableCreated<JournalEntry>(journalTable);
            _connection.EnsureTableCreated<CommitEntry>(commitTable);

            _commits = new SqliteExpressionBuilder(commitTable);
            _journal = new SqliteExpressionBuilder(journalTable);
        }

        public string CommitChanges(string setCommitId = null, string mergeSecondBaseCommitId = null)
        {
            bool forceCommit = !setCommitId.IsNullOrEmpty();
            string lastCommitId = null;

            _connection.RunInTransaction(() =>
            {
                // get last commit.
                CommitEntry last = GetLastCommitEntry();

                int maxId = GetMaxJournalId();

                var hasNewData = last != null && maxId != last.JournalSmallerAndEqualId
                              || last == null && maxId > 0;

                if (!hasNewData && !forceCommit)
                {
                    if (last != null)
                        lastCommitId = last.CommitId;
                    return; // nothing to do.
                }

                lastCommitId = forceCommit ? setCommitId : SequentialGuid.NewGuidString();

                CommitEntry commit = new CommitEntry
                {
                    CommitId = lastCommitId,
                    BasedOnCommitId = last == null ? null : last.CommitId,
                    MergeSecondCommitId = mergeSecondBaseCommitId,
                    JournalLargerThanId = last == null ? 0 : last.JournalSmallerAndEqualId,
                    JournalSmallerAndEqualId = maxId,
                    Timestamp = DateTime.UtcNow
                };

                // here we implicitly check that nobody inserts a commit with 
                // the same commit id twice.
                _connection.Insert(_commits.TableName, commit);

                CheckDataIntegrity();
            });

            return lastCommitId;
        }

        public string GetLastCommitId()
        {
            var last = GetLastCommitEntry();
            int maxId = GetMaxJournalId();

            if (last == null && maxId > 0)
                return null;

            if (last == null)
                return "";

            bool hasNewData = maxId != last.JournalSmallerAndEqualId;
            if (hasNewData) return null;

            return last.CommitId;
        }

        public IEnumerable<CommitEntry> GetCommits(string sinceButExcludingCommitId)
        {
            int firstCommitDbId = GetCommitDbId(sinceButExcludingCommitId);

            foreach (var commit in _connection.Query<CommitEntry>(_commits.Select().Where("Id>?").OrderBy("Id"),
                                               firstCommitDbId))
            {
                int journalLargerThanId = commit.JournalLargerThanId;
                int journalSmallerAndEqualId = commit.JournalSmallerAndEqualId;
                commit.JournalEntries = _connection.Query<JournalEntry>(_journal.Select().Where("Id>? AND Id<=?"), journalLargerThanId, journalSmallerAndEqualId)
                                                   .ToList();
                yield return commit;
            }
        }

        private CommitEntry GetLastCommitEntry()
        {
            CommitEntry last = _connection.Query<CommitEntry>(_commits.Select().OrderBy("Id DESC").Limit(1))
                                          .FirstOrDefault();
            return last;
        }

        private int GetCommitDbId(string sinceButExcludingCommitId)
        {
            if (sinceButExcludingCommitId.IsNullOrEmpty())
                return 0;

            // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault : the wrapper has no FirstOrDefault optimization.
            var commit = _connection.Query<CommitEntry>(_commits.Select().Where("CommitId=?"), sinceButExcludingCommitId)
                                    .FirstOrDefault();
            if (commit == null)
                throw new CommitNotFoundException(sinceButExcludingCommitId);

            return commit.Id;
        }

        public void AddOrReplaceJournalEntry(JournalEntry entry)
        {
            // always update the id, be keep only last one.
            if (entry.Member == null)
                entry.Member = "";

            // TODO: check if this is actually used.
            if (entry.Change == ChangeType.Created)
            {
                // drop any possibly previous deletions of the object
                // to allow for recreation of objects.
                _connection.Execute(_journal.Delete().Where("Type=?1 and ObjectId=?2 AND Change=?3"),
                                    entry.Type, entry.ObjectId, ChangeType.Deleted);
            }

            // note: the extra spaces are there to fool the wrapper library to not! include the 'Id'-field.
            //       this is a very dirty workaround that might break at some point.
            InsertEntry(entry, "OR REPLACE   ");

            if (entry.Id == 0)
                throw new Exception("error INSERT OR REPLACE not working as expected.");

        }

        public IList<string> GetCommitIdsSince(string lastCommonCommitId)
        {
            int firstCommitDbId = GetCommitDbId(lastCommonCommitId);

            string cmd = _commits.Select("CommitId")
                                 .Where("Id>?")
                                 .OrderBy("Id");
            return _connection.Query<string>(cmd, firstCommitDbId)
                              .AsEnumerable()
                              .ToList();
        }

        public bool HasCommit(string commitId)
        {
            if (commitId.IsNullOrEmpty())
                return true;

            string cmd = _commits.Select("COALESCE(COUNT(*),0)")
                                 .WhereEquals("CommitId", "?");
            int count = _connection.ExecuteScalar<int>(cmd, commitId);
            return count > 0;
        }

        public void Clear()
        {
            _connection.RunInTransaction(() =>
            {
                _connection.Execute(_journal.Delete());
                _connection.Execute(_commits.Delete());
            });
        }

        public IList<string> GetLastCommitIds(int limit = Int32.MaxValue)
        {
            var commits = _connection.Query<string>(_commits.Select("CommitId")
                                                            .OrderBy("Id DESC")
                                                            .Limit(limit))
                                     .ToList();
            commits.Reverse();
            return commits;
        }


        private void InsertEntry(JournalEntry tracking, string extra = null)
        {
            Debug.Assert(tracking.ObjectId != null);
            _connection.Insert(_journal.TableName, tracking, extra);
        }

        private int GetMaxJournalId()
        {
            return _connection.ExecuteScalar<int>("SELECT COALESCE(MAX(Id), 0) FROM " + _journal.TableName);
        }

        //public IEnumerable<JournalEntry> GetEntries(int largerThanId, int smallerAndEqualId)
        //{
        //    return _connection.Table<TJournalEntry>()
        //                      .Where(p => p.Id > largerThanId && p.Id <= smallerAndEqualId);
        //}

        private void CheckDataIntegrity()
        {
#if DEBUG
            // for each modification there must be a creation, in logial time before.
            string cmd = string.Format(
                "select COUNT(*) from {0} Mod \n" +
                "   left outer join {0} Creat on Mod.ObjectId is Creat.ObjectId \n" +
                "WHERE Creat.Id is NULL or (Mod.Change=1 and Creat.Change=0  and Mod.Id<Creat.Id)",
                _journal.TableName);
            int rows = _connection.ExecuteScalar<int>(cmd);
            Debug.Assert(rows == 0);
#endif
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", _journal.TableName, Path.GetFileName(_connection.DatabasePath));
        }
    }
}
