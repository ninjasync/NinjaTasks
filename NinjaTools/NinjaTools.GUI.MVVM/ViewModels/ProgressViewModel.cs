using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MvvmCross.ViewModels;
using NinjaTools.Progress;

namespace NinjaTools.GUI.MVVM.ViewModels
{
    public class ProgressViewModel : MvxNotifyPropertyChanged, IProgressFactory
    {
        public bool IsInProgress { get; private set; }

        public bool IsIndeterminate { get; private set; }

        public bool CanPause { get; private set; }
        public bool IsDelay { get; private set; }

        public string Title { get; internal set; }
        public float Progress { get; internal set; }
        public bool WasCancelled { get { return _cancel.IsCancellationRequested; } }

        public bool CanCancel { get; private set; }

        private CancellationTokenSource _cancel = new CancellationTokenSource();

        private readonly List<MyProgress> _activeProgresses = new List<MyProgress>();

        public void Cancel()
        {
            _cancel.Cancel();
        }

        public IProgressDisposable Use(string title = "", ProgressOptions options = ProgressOptions.Default)
        {
            CanCancel = (options & ProgressOptions.AllowCancel) == ProgressOptions.AllowCancel;
            CanPause = (options & ProgressOptions.AllowPause) == ProgressOptions.AllowPause;
            IsDelay = (options & ProgressOptions.DelayPopup) == ProgressOptions.DelayPopup;
            IsIndeterminate = (options & ProgressOptions.IsIndeterminate) == ProgressOptions.IsIndeterminate;

            MyProgress progress;

            lock (_activeProgresses)
            {
                if (_activeProgresses.Count == 0)
                {
                    _cancel = new CancellationTokenSource();
                    IsInProgress = true;
                    Title = title;
                    Progress = 0;
                    progress = new MyProgress(this) { IsActive = true, LastTitle = title };

                }
                else    // allow only one user at a time.
                    progress = new MyProgress(this) { IsActive = false, LastTitle = title };

                _activeProgresses.Add(progress);
            }

            return progress;
        }

        internal void Free(MyProgress progress)
        {
            string newTitle = null;
            float newProgress = 0;
            lock (_activeProgresses)
            {
                int idx = _activeProgresses.IndexOf(progress);
                Debug.Assert(idx >= 0);
                _activeProgresses.RemoveAt(idx);

                if (idx == 0)
                {
                    progress.IsActive = false;

                    if (_activeProgresses.Count == 0)
                    {
                        _cancel = new CancellationTokenSource();
                        IsInProgress = false;
                    }
                    else
                    {
                        // activate previous last-progress.
                        progress = _activeProgresses[0];
                        if (!progress.WasDisposed)
                        {
                            newTitle = progress.LastTitle;
                            newProgress = progress.LastProgress;
                            progress.IsActive = true;
                        }
                    }
                }
            }
            if (newTitle != null)
            {
                Title = newTitle;
                Progress = newProgress;
            }
        }

        public class MyProgress : IProgressDisposable
        {
            private readonly ProgressViewModel _model;

            public MyProgress(ProgressViewModel model)
            {
                WasDisposed = false;
                _model = model;
            }

            public float Progress { set { LastProgress = value; if (IsActive) _model.Progress = value; } }

            public float LastProgress { get; set; }
            public string LastTitle { get; set; }

            public string Title { set { LastTitle = value; if (IsActive) _model.Title = value; } }

            public bool Cancel { get { return _model.WasCancelled; } }
            public CancellationToken CancelToken { get { return _model._cancel.Token; } }
            public bool IsActive { get; set; }

            public bool WasDisposed { get; set; }

            public void Dispose()
            {
                if (WasDisposed) return;
                WasDisposed = true;
                _model.Free(this);
            }
        }

        
    }
}
