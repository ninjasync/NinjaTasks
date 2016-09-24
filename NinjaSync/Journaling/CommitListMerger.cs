using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NinjaSync.Model.Journal;
using NinjaTools;
using NinjaTools.Logging;

namespace NinjaSync.Journaling
{
    public enum MergeResult
    {
        Reject,
        Accept,
        AreEqual
        //MergedBoth
    }

    public delegate MergeResult MergeConflictProc(string modifiedProperty,
                                          ITrackable objFirst, DateTime modifiedFirst,
                                          ITrackable objSecond, DateTime modifiedSecond);

    /// <summary>
    /// Central class containing logic to merge one commit list on top of another,
    /// with conflict detection.
    /// </summary>
    public class CommitListMerger
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// note that mergeCommit is only good for creating JournalEntries. its object 
        /// might be randomly either the old or new one, only good for its TrackableId
        /// </summary>
        public void CreateMerge(CommitList baseList, Commit toBeMerged, out Commit accepted, out Commit mergeCommit)
        {
            mergeCommit = new Commit();
            accepted = toBeMerged;

            if (baseList.IsEmpty || toBeMerged.IsEmpty)
                return;

            // since the order of tha based commits is irrelevant when applying the
            // merge commit, use the newest!
            int ancestorIdx1 = baseList.Commits.FindIndex(p => p.CommitId == toBeMerged.BasedOnCommitId);
            int ancestorIdx2 = baseList.Commits.FindIndex(p => p.CommitId == toBeMerged.BasedOnCommitId2);
            var maxAncestorIdx = Math.Max(ancestorIdx1,ancestorIdx2);

            var relevantCommits = baseList.Commits.Skip(maxAncestorIdx+1).ToList();

            if (relevantCommits.Count == 0)
                return;

            // merge commit always re-records deletions of all commits.
            mergeCommit.Deleted.AddRange(relevantCommits.SelectMany(p => p.Deleted));
            mergeCommit.Deleted.AddRange(toBeMerged.Deleted);

            CommitList newlyAccepted = new CommitList();
            
            DoCreateDiff(relevantCommits, new[] {toBeMerged}, newlyAccepted, mergeCommit, MergeByModificationDate);

            accepted = newlyAccepted.Commits[0];
        }

        /// <summary>
        /// create a diff. 'accepted' can be applied on top of baseList. 'rejected' is a commit
        /// that needs to be applied/refreshed after accepted, to restore so that the events 
        /// appeared to have happened - in logical time - after accepted is applied.
        /// Usually it is enough to create JournalEntries for the rejection.
        /// </summary>
        public void CreateDiff(CommitList baseList, CommitList newList, out CommitList newlyAccepted, out Commit keptFromBase)
        {
            newlyAccepted = new CommitList();
            keptFromBase = new Commit();

            newlyAccepted.RemoteCommitId = newList.RemoteCommitId;
            newlyAccepted.StorageId = newList.StorageId;

            if (newList.IsEmpty || baseList.IsEmpty)
            {
                newlyAccepted = newList;
                keptFromBase = baseList.Flatten();
                return;
            }
            // TODO: think this over. 

            Dictionary<string, int> baseCommitsToIdx = baseList.Commits
                                                          .Select((c,idx)=>new {CommitIdUntil = c.CommitId, Idx = idx})
                                                          .ToDictionary(c=>c.CommitIdUntil, c=>c.Idx);
            Debug.Assert(baseCommitsToIdx.Count == baseList.Commits.Count);
            Debug.Assert(!baseCommitsToIdx.ContainsKey(""));

            if (!baseList.BasedOnCommitId.IsNullOrEmpty()) // add "from"
                baseCommitsToIdx.Add(baseList.BasedOnCommitId, -1);

            

            // don't process any commits older and including the common ancestor.
            Tuple<int, int> ancestor = GetCommonAncestor(baseCommitsToIdx, newList.Commits.Select(p=>p.CommitId).ToList());

            // Some optimizations in case there is nothing to do.
            if (ancestor.Item2 == newList.Commits.Count - 1)
            {
                // common ancestor is newLists last item. 
                // newlyAccepted will be empty.
                keptFromBase = baseList.Flatten();
                return;
            }
            if (ancestor.Item1 == baseList.Commits.Count - 1)
            {
                // common ancestor is baseLists lastitem.
                // everything in newList is - from logical time - 
                // newer than baseList.
                newlyAccepted = newList; // accept everything.
                return;
            }

            var baseCommits = baseList.Commits.Skip(ancestor.Item1+1).ToList();
            var newCommits = newList.Commits.Skip(ancestor.Item2+1).ToList();

            Commit conflicts = new Commit();

            // calculate accepted changes.
            DoCreateDiff(baseCommits, newCommits, newlyAccepted, conflicts, MergeByModificationDate);

            // subtract accepted changes from baseCommits.
            CommitList keptFromBaseList = new CommitList();
            DoCreateDiff(newlyAccepted.Commits, baseCommits, keptFromBaseList, conflicts, MergeAlwaysRejectSecond);
            // TODO: think about which CommitIds to give.
            keptFromBase = keptFromBaseList.Flatten();
            return;
        }
        /// <summary>
        /// note that conflicts is only good for creating JournalEntries. its object 
        /// might be either the old or new one, not dependant on which object/properties
        /// acually be taken. 
        /// </summary>
        private void DoCreateDiff(List<Commit> baseCommits, IEnumerable<Commit> newCommits,
                                  CommitList newlyAccepted, 
                                  Commit conflicts,
                                  MergeConflictProc mergeConflictProc)
        {
            Dictionary<TrackableId, DateTime> prevDel = baseCommits.SelectMany(d=>d.Deleted)
                                                                  .ToDictionary(d=>d.Key, d=>d.ModifiedAt);
            
            Dictionary<TrackableId, ITrackable> latestObject = new Dictionary<TrackableId, ITrackable>();
            Dictionary<TrackableId, Dictionary<string,DateTime>> prevModByObject = new Dictionary<TrackableId, Dictionary<string, DateTime>>();
            Dictionary<TrackableId, DateTime> prevCreated = new Dictionary<TrackableId, DateTime>();


            // (a) Preparations.
            // Track the latest instance of each object, 
            // and the latest modification date of each property.
            foreach (var mod in baseCommits.SelectMany(p => p.Modified))
            {
                if (mod.Key == null)
                {
                    // new object cannot trigger a merge.
                    continue;
                }
                
                latestObject[mod.Key] = mod.Object;

                if (mod.ModifiedPropertiesEx == null)
                {
                    ModifiedProperty prop = new ModifiedProperty(null, mod.ModifiedAt);
                    prevCreated[mod.Key] = prop.ModifiedAt;
                    //prevModByObject[mod.Key][""] = prop.ModifiedAt;
                }
                else
                {
                    foreach (var prop in mod.ModifiedPropertiesEx)
                    {
                        Dictionary<string, DateTime> dict;
                        if (!prevModByObject.TryGetValue(mod.Key, out dict))
                            dict = prevModByObject[mod.Key] = new Dictionary<string, DateTime>();
                        dict[prop.Property] = prop.ModifiedAt;
                    }
                }
            }

            // (b) now merge the data.

            foreach (var newCommit in newCommits)
            {
                Commit accepted = new Commit { BasedOnCommitId = newCommit.BasedOnCommitId, CommitId = newCommit.CommitId, BasedOnCommitId2 = newCommit.BasedOnCommitId2};
                newlyAccepted.Commits.Add(accepted);

                foreach (var del in newCommit.Deleted)
                {
                    // nothing to do. just drop the change.
                    if (prevDel.ContainsKey(del.Key)) 
                        continue;
                    // always accept deletions. 
                    accepted.Deleted.Add(del);
                    conflicts.Deleted.Add(del);
                }

                foreach (var mod in newCommit.Modified)
                {
                    if (mod.Key == null)
                    {
                        // this can happen on new objects, that did not get an id assigned 
                        // by the remote site. always accept.
                        accepted.Modified.Add(mod);
                        continue;
                    }

                    // 1: object was previously deleted.
                    if (prevDel.ContainsKey(mod.Key))
                    {
                        if(Log.IsInfoEnabled)
                            Log.Info("dropping change of previously deleted object {0}: {1}", mod.Key, mod.ModifiedProperties==null?("all"):string.Join(",",mod.ModifiedProperties));

                        conflicts.Deleted.Add(new Modification(mod.Key, prevDel[mod.Key]));
                        //rejected.Deleted.Add(new Modification(mod.Key, prevDel[mod.Key]));
                        continue;
                    }


                    DateTime prevCreatedAt;
                    Dictionary<string, DateTime> prevProps;

                    prevCreated.TryGetValue(mod.Key, out prevCreatedAt);
                    prevModByObject.TryGetValue(mod.Key, out prevProps);
                    
                    // 2: simple case: no previous changes. just accept.
                    if (prevCreatedAt == default (DateTime) && prevProps == null)
                    {
                        accepted.Modified.Add(mod);
                        continue;
                    }

                    // 3. change to an existing object.
                    //    make a property by property merge.

                    // treat "new" as "changed all columns".
                    if (prevProps == null)
                        prevProps = latestObject[mod.Key].Properties.ToDictionary(p => p, p => prevCreatedAt);

                    IList<ModifiedProperty> curProps;
                    if (mod.ModifiedProperties != null)
                        curProps = mod.ModifiedPropertiesEx;
                    else
                    {
                        var modDate = mod.ModifiedAt;
                        curProps = mod.Object.Properties.Select(p => new ModifiedProperty(p, modDate)).ToList();
                    }

                    List<ModifiedProperty> acceptedProperties = new List<ModifiedProperty>();
                    List<ModifiedProperty> conflictedProperties = new List<ModifiedProperty>();

                    // column by column
                    foreach (var prop in curProps)
                    {
                        if (prop.Property == TrackableProperties.ColId)
                            continue;
                        
                        DateTime prevPropChangedAt;
                        bool hasPrevVal = prevProps.TryGetValue(prop.Property, out prevPropChangedAt);

                        if (!hasPrevVal)
                            acceptedProperties.Add(prop);
                        else
                        {
                            // HERE IS THE CRITICAL MERGE.
                            MergeResult merge = mergeConflictProc(prop.Property, 
                                                          latestObject[mod.Key], prevPropChangedAt,
                                                          mod.Object, prop.ModifiedAt);
                            if (merge == MergeResult.Accept)
                                acceptedProperties.Add(prop);

                            conflictedProperties.Add(prop);
                        }
                    }

                    if (acceptedProperties.Any())
                    {
                        Modification accept = new Modification(mod.Object, acceptedProperties);
                        accepted.Modified.Add(accept);
                    }

                    if (conflictedProperties.Any())
                    {
                        Modification conflict = new Modification(latestObject[mod.Key], 
                                                                 conflictedProperties
                                                                 .ToList());
                        conflicts.Modified.Add(conflict);
                    }
                    continue;
                }
            }
        }

     
        /// <summary>
        /// returns the second object, if it is newer or of same age as first property.
        /// </summary>
        private MergeResult MergeByModificationDate(string modifiedProperty, 
                                                    ITrackable objBase, DateTime modifiedAtBase, 
                                                    ITrackable objNew,DateTime modifiedAtNew)
        {
            if (Equals(objBase.GetProperty(modifiedProperty), objNew.GetProperty(modifiedProperty)))
            {
                if (Log.IsWarnEnabled) Log.Warn("merge conflict on {0} property {1}, values are equal, skipping, local/remote timestamps: {2}/{3}",
                                                 new TrackableId(objBase), modifiedProperty, modifiedAtBase, modifiedAtNew);
                return MergeResult.AreEqual;
            }
                

            var ret = modifiedAtNew >= modifiedAtBase ? MergeResult.Accept : MergeResult.Reject;

            if(Log.IsWarnEnabled) Log.Warn("merge conflict on {0} property {1}, will {2} remote, local/remote timestamps: {3}/{4}", 
                                            new TrackableId(objBase), modifiedProperty, ret, modifiedAtBase, modifiedAtNew);

            return ret;
        }

        private MergeResult MergeAlwaysRejectSecond(string modifiedProperty,
                                                   ITrackable objFirst, DateTime modifiedFirst,
                                                   ITrackable objSecond, DateTime modifiedSecond)
        {
            return MergeResult.Reject;
        }

        /// <summary>
        /// will return -1,1 if no common ancestor found.
        /// </summary>
        private Tuple<int, int> GetCommonAncestor(Dictionary<string, int> baseCommits, List<string> newCommits)
        {
            for (int i = newCommits.Count-1; i>=0; --i)
            {
                var commitId = newCommits[i];
                if(baseCommits.ContainsKey(commitId)) 
                    return new Tuple<int, int>(baseCommits[commitId], i);
            }
            return new Tuple<int, int>(-1,-1);
        }
    }
}
