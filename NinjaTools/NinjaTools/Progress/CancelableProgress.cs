using System.Threading;

namespace NinjaTools.Progress
{
    public class CancelableProgress : IProgress
    {
        private readonly CancellationToken _token;

        public CancelableProgress(CancellationToken token)
        {
            _token = token;
        }

        public float Progress { set; private get; }
        public string Title { set; private get; }
        public bool Cancel { get { return _token.IsCancellationRequested; } }
        public CancellationToken CancelToken { get { return _token; } }
    }
}