using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTools;

namespace NinjaSync.Model.Journal
{
    public class Commit
    {
        private List<Modification> _deleted;
        private List<Modification> _modified;

        /// <summary>
        /// Note that an object can not be deleted an modified at the same time.
        /// </summary>
        public List<Modification> Deleted
        {
            get
            {
                if(IsPlaceholder) throw new Exception("cannot get modifications of placeholder commit");
                return _deleted;
            }
            set
            {
                if (IsPlaceholder) throw new Exception("cannot set modifications of placeholder commit");
                _deleted = value;
            }
        }

        /// <summary>
        /// Note that an object can not be deleted and modified at the same time.
        /// </summary>
        public List<Modification> Modified
        {
            get
            {
                if (IsPlaceholder) throw new Exception("cannot get modifications of placeholder commit");
                return _modified;
            }
            set
            {
                if (IsPlaceholder) throw new Exception("cannot set modifications of placeholder commit");
                _modified = value;
            }
        }

        /// <summary>
        /// This is the CommitId. If this list represents multiple Commits, 
        /// this is the final CommitId
        /// </summary>
        public string CommitId { get; set; }

        /// <summary>
        /// this list contains all modifications from, 
        /// but not including BasedOnCommitId
        /// </summary>
        public string BasedOnCommitId { get; set; }

        /// <summary>
        /// if this is a merge commit, BasedOnCommitId2 contains 
        /// the second Commit this one was based on.
        /// </summary>
        public string BasedOnCommitId2 { get; set; }

        public Commit()
        {
            Deleted = new List<Modification>();
            Modified = new List<Modification>();
        }

        /// <summary>
        /// this is a convinience concat of Deleted and Modified.
        /// </summary>
        public IEnumerable<Modification> DeletedAndModified { get { return Deleted.Concat(Modified); } }

        public bool IsEmpty
        {
            get
            {
                if(IsPlaceholder) throw new Exception("cannot determine if skeleton commit is empty.");
                return Deleted.Count == 0 && Modified.Count == 0;
            }
        }
        public bool IsMergeCommit { get { return !BasedOnCommitId2.IsNullOrEmpty(); } }

        /// <summary>
        /// if true, only records CommitId, BasedOnCommitId and BasedOnCommitId2. Deleted and Modified
        /// are not accesibble 
        /// </summary>
        public bool IsPlaceholder { get; private set; }

        public Commit ToPlaceholder()
        {
            return new Commit { CommitId = CommitId, BasedOnCommitId = BasedOnCommitId, BasedOnCommitId2 = BasedOnCommitId2, IsPlaceholder = true};
        }

        public Commit CloneToEmpty()
        {
            return new Commit { CommitId = CommitId, BasedOnCommitId = BasedOnCommitId, BasedOnCommitId2 = BasedOnCommitId2};
        }
    }

    public static class ModificationListExtensions
    {
        public static IEnumerable<Modification> OfTypeTodoList(this IEnumerable<Modification> list)
        {
            return list.Where(p => p.ObjectType == TrackableType.List);
        }
        public static IEnumerable<Modification> OfTypeTodoTask(this IEnumerable<Modification> list)
        {
            return list.Where(p => p.ObjectType == TrackableType.Task);
        }
    }
}
