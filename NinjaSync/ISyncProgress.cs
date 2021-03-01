using System.Threading;
using NinjaTools.Progress;

namespace NinjaSync
{
    public interface ISyncProgress : IProgress
    {
        int LocalDeleted { get; set; }
        //int LocalCreated { get; set; }
        int LocalModified { get; set; }

        int RemoteDeleted { get; set; }
        //int RemoteCreated { get; set; }
        int RemoteModified { get; set; }
    }

    public class NullSyncProgress : NullProgress, ISyncProgress
    {
        public int LocalDeleted { get; set; }
        //public int LocalCreated { get; set; }
        public int LocalModified { get; set; }
        public int RemoteDeleted { get; set; }
        //public int RemoteCreated { get; set; }
        public int RemoteModified { get; set; }

        public NullSyncProgress()
        {
        }

        public NullSyncProgress(CancellationToken cancel)
            : base(cancel)
        {

        }
    }

    public class WrappingSyncProgress : ISyncProgress
    {
        private readonly IProgress _p;

        public WrappingSyncProgress(IProgress p)
        {
            _p = p;
        }

        public int LocalDeleted { get; set; }
        //public int LocalCreated { get; set; }
        public int LocalModified { get; set; }
        public int RemoteDeleted { get; set; }
        //public int RemoteCreated { get; set; }
        public int RemoteModified { get; set; }

        public float Progress { set { _p.Progress = value; } }
        public string Title { set { _p.Title = value; } }
        public bool Cancel { get { return _p.Cancel; } }
        public CancellationToken CancelToken { get { return _p.CancelToken; } }
    }

    public class RedirectingSyncProgress : ISyncProgress
    {
        private readonly IProgress _p;
        private readonly ISyncProgress _sp;

        public RedirectingSyncProgress(IProgress p, ISyncProgress sp)
        {
            _p = p;
            _sp = sp;
        }


        public float Progress { set { _p.Progress = value; } }
        public string Title { set { _p.Title = value; } }
        public bool Cancel { get { return _p.Cancel; } }
        public CancellationToken CancelToken { get { return _p.CancelToken; } }

        public int LocalDeleted { get { return _sp.LocalDeleted; } set { _sp.LocalDeleted = value; } }
        public int LocalModified { get { return _sp.LocalModified; } set { _sp.LocalModified = value; } }
        public int RemoteDeleted { get { return _sp.RemoteDeleted; } set { _sp.RemoteDeleted = value; } }
        public int RemoteModified { get { return _sp.RemoteModified; } set { _sp.RemoteModified = value; } }
    }
}