using System.Threading;

namespace NinjaTools.Progress
{
    public class NullProgress : IProgressDisposable
    {
        private readonly CancellationToken _cancel;

        public NullProgress()
        {
        }

        public NullProgress(CancellationToken cancel)
        {
            _cancel = cancel;
        }

        float IProgress.Progress
        {
            set {  }
        }

        string IProgress.Title
        {
            set {  }
        }

        public bool Cancel
        {
            get { return _cancel.IsCancellationRequested; }
        }

        public CancellationToken CancelToken { get { return _cancel; } }

        public void Dispose()
        {   
        }
    }
}