using System;
using System.ComponentModel;
using System.Timers;
using Android.Content;
using NinjaTools.GUI.MVVM.Services;
using NinjaTools.Npc;

namespace NinjaTools.Droid.Services
{
    /// <summary>
    /// adds a timeout to the app-active service; 
    /// The App's Application object must be based on 'ApplicationEx'
    /// </summary>
    internal class TimeoutIsAppActiveService : IIsAppActive
    {
        public bool IsInForeground { get; private set; }

        private readonly IIsAppActive _base;
        private readonly IDisposable _baseToken;
        private readonly Timer _timer;

        public TimeSpan Timeout { get { return TimeSpan.FromMilliseconds(_timer.Interval); }  set { _timer.Interval = value.TotalMilliseconds;}} 

        public TimeoutIsAppActiveService(IWeakTimerService timer, Context context)
        {
            _base = ((ApplicationEx) context.ApplicationContext);
            _baseToken = _base.SubscribeWeak(x => x.IsInForeground, OnBaseForegroundChanged);
            _timer = new Timer { Interval = 500, AutoReset = false };
            _timer.Elapsed += OnElapsed;

            OnBaseForegroundChanged();
        }

        private void OnBaseForegroundChanged()
        {
            if (IsInForeground == _base.IsInForeground) return;

            if (_base.IsInForeground)
            {
                _timer.Enabled = false;
                IsInForeground = _base.IsInForeground;
                return;
            }
            // got deactivated. set up timer.
            _timer.Enabled = true;
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            IsInForeground = _base.IsInForeground;
            _timer.Enabled = false;
        }

        #pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore CS0067
    }
}
