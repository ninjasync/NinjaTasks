using System.Threading;

namespace NinjaTools.Progress
{
    public class PartitialProgress : IProgressDisposable
    {
        public PartitialProgress(IProgress progress, float rangeStart, float rangeEnd, bool takeOwnership=false)
        {
            this.rangeStart = rangeStart;
            this.rangeEnd = rangeEnd;
            _takeOwnership = takeOwnership;
            this.progress = progress;
        }

        public PartitialProgress(IProgress progress, bool takeOwnership = false)
        {
            this.rangeStart = 0;
            this.rangeEnd = 0;
            this.progress = progress;
            _takeOwnership = takeOwnership;
        }

        public void NextStep(float range)
        {
            rangeStart = rangeEnd;
            rangeEnd += range;
            Progress = 0;
        }
        public void NextStep(float range, string title)
        {
            NextStep(range);
            stepTitle = title;
            Title = "";
        }

        public float Progress { 
            set 
            { 
                progress.Progress = rangeStart + (value * (rangeEnd - rangeStart)); 
            } 
        }
        public string Title
        {
            set
            {
                string v = stepTitle;
                if (v != string.Empty && value != string.Empty)
                    v += ": ";
                progress.Title = v + value;
            }
        }

        public float ProgressLeft
        {
            get { return 1 - rangeEnd; }
        }

        private float rangeStart, rangeEnd;
        private readonly IProgress progress;
        private readonly bool _takeOwnership;
        private string stepTitle = string.Empty;

        public bool Cancel
        {
            get { return progress.Cancel; }
        }

        public CancellationToken CancelToken { get { return progress.CancelToken; } } 
        public void Dispose()
        {
            if(_takeOwnership && progress is IProgressDisposable)
                ((IProgressDisposable)progress).Dispose();
        }
    }

    public static class PartitialProgressExtentions
    {
        public static PartitialProgress AsPartitial(this IProgress progress)
        {
            return new PartitialProgress(progress, false);
        }
        public static PartitialProgress AsPartitial(this IProgressDisposable progress)
        {
            return new PartitialProgress(progress, true);
        }
    }
}
