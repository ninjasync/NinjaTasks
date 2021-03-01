using System;

namespace NinjaSync.Exceptions
{
    public class CommitNotFoundException : Exception
    {
        //public IList<string> PossibleCommitIds { get; set; }

        public CommitNotFoundException(Exception innerException)
            : base("fresh sync required: " + innerException.Message, innerException)
        {
        }

        public CommitNotFoundException(string commitId, string msg)
            : base(msg)
        {

        }

        public CommitNotFoundException(string commitId) 
            :base("could not find commit '" + commitId + "'")
        {
            
        }

        //public CommitNotFoundException(string commitId, IList<string> possibleCommitIds)
        //    :this(commitId)
        //{
        //    PossibleCommitIds = possibleCommitIds;
        //}
    }
}