using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NinjaSync.Model.Journal
{
    public class CommitList 
    {
        public List<Commit> Commits = new List<Commit>();

        public string BasedOnCommitId { get { return Commits.Select(p => p.BasedOnCommitId).FirstOrDefault(); } }
        public string FinalCommitId { get { return Commits.Select(p => p.CommitId).LastOrDefault(); } }
        public bool IsEmpty { get { return Commits.Count == 0 || (Commits.Count == 1 && !Commits[0].IsPlaceholder && Commits[0].IsEmpty); } }

        /// <summary>
        /// This field is here fore convenience with synchronization.
        /// <para>
        /// <bold>Master/Slave sync</bold>: this represents the remote CommitId
        /// </para>
        /// </summary>
        public string RemoteCommitId;

        /// <summary>
        /// this represents the StorageId this CommitList is derived from.
        /// Used with P2P sycnhronization.
        /// </summary>
        public string StorageId { get; set; }

        public CommitList()
        {
        }

        public CommitList(Commit list)
        {
            Commits.Add(list);
        }

        public int ModificationCount { get { return Commits.Where(c => !c.IsPlaceholder).Sum(c => c.Modified.Count); } }
        public int DeletionCount { get { return Commits.Where(c => !c.IsPlaceholder).Sum(c => c.Deleted.Count); } }

       

        public Commit Flatten()
        {
            if (Commits.Count == 1)
                return Commits[0];

            Commit ret = new Commit();
            ret.BasedOnCommitId = BasedOnCommitId;
            ret.CommitId = FinalCommitId;

            Dictionary<TrackableId, Modification> modified = new Dictionary<TrackableId, Modification>();
            
            Modification prev;

            foreach (var del in Commits.SelectMany(p => p.Deleted))
            {
                ret.Deleted.Add(del);
                modified.Add(del.Key, null);
            }

            foreach (var mod in  Commits.SelectMany(p => p.Modified))
            {
                Debug.Assert(mod.ModifiedProperties == null 
                          || mod.Key != null, "can not have a modification without a pre-filled key.");

                
                if (mod.Key == null)
                {
                    // new object.
                    ret.Modified.Add(mod);
                }
                else if (!modified.TryGetValue(mod.Key, out prev))
                {
                    ret.Modified.Add(mod);
                    modified.Add(mod.Key,mod);
                }
                else if (prev == null)
                {
                    // was deleted. ignore.
                    continue;
                }
                else
                {
                    // set modification date to latest.
                    prev.ModifiedAt = mod.ModifiedAt;

                    // don't merge properties: already fully included.
                    if (prev.ModifiedProperties == null) continue;

                    // we can only be a modification.
                    Debug.Assert(mod.ModifiedProperties != null);
                    
                    // merge modified properties.
                    var propList = prev.ModifiedPropertiesEx;
                    
                    foreach (var newprop in mod.ModifiedPropertiesEx)
                    {
                        // remove old entry, if any, than at new one.
                        var prevprop = propList.FirstOrDefault(p => p.Property == newprop.Property);
                        if (prevprop != null) propList.Remove(prevprop);
                        propList.Add(newprop);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// TODO: would be good if I could get rid of these "dummy-commits"
        /// </summary>
        public bool IsDummy
        {
            get
            {
                if (Commits.Count != 1) return false;

                var commit = Commits[0];
                if (!commit.IsPlaceholder && commit.IsEmpty && commit.BasedOnCommitId == commit.CommitId)
                    return true;
                return false;
            }
        }

        public static CommitList CreateDummy(string commitId, string storageId)
        {
            var ret = new CommitList(new Commit { BasedOnCommitId = commitId, CommitId = commitId });
            ret.StorageId = storageId;
            return ret;
        }
    }
}