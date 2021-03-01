using System;
using System.Threading;

namespace NinjaTools.Progress
{
    public interface IProgress
    {
        // set progress from 0-1
        float Progress { set; }
        string Title { set; }
        bool Cancel { get; }

        CancellationToken CancelToken { get; }
    }

    public interface IProgressDisposable : IProgress, IDisposable { }
}
