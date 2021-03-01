using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NinjaTools.Logging;
using NinjaTools.GUI.MVVM;

namespace NinjaTasks.Core.ViewModels
{
    public class LogLine : INotifyPropertyChanged
    {
        public LogLevel Level { get; set; }
        public string Line { get; set; }
        public DateTime Timestamp { get; set; }

        #pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore CS0067
    }

    public class LogViewModel : BaseViewModel, IActivate, IDeactivate
    {
        private readonly ILogProviderFactory _log;
        private bool _isActive = false;

        private ILogProvider _logger;
        public bool IsEnabled { get; set; }
        public bool IsVerbose { get; set; }
        
        public LogLevel MinLogLevel { get; set; }

        public ObservableCollection<LogLine> Log { get; private set; }

        public LogViewModel(ILogProviderFactory log)
        {
            _log = log;
            Log = new ObservableCollection<LogLine>();
            MinLogLevel = LogLevel.Info;
        }

        public bool CanClear { get { return Log.Count > 0; } }
        public void Clear()
        {
            Log.Clear();
        }

        private void OnIsEnabledChanged()
        {
            bool shouldLog = IsEnabled && _isActive;

            if (shouldLog && _logger == null)
            {
                _logger = _log.Create("*", LogLevel.Trace);
                _logger.Log += AddLogLine;
            }
            else if(!shouldLog && _logger != null)
            {
                _logger.Log -= AddLogLine;
                _logger.Close();
                _logger = null;
            }
        }

        private void AddLogLine(object sender, LogEventArgs e)
        {
            if (e.Level < MinLogLevel && !IsVerbose) return;

            var logLine = new LogLine { Level = e.Level, Line = e.Message, Timestamp = e.Timestamp};
            InvokeOnMainThread(() => Log.Add(logLine));
#if !DOT42
            RaisePropertyChanged(()=>CanClear);
#else
            RaisePropertyChanged("CanClear");
#endif
        }

        public void OnActivate()
        {
            _isActive = true;
            OnIsEnabledChanged();
        }

        public void OnDeactivate()
        {
            _isActive = false;
            OnIsEnabledChanged();
        }

        public void OnDeactivated(bool destroying)
        {
            _isActive = false;
            OnIsEnabledChanged();
        }
    }
}
