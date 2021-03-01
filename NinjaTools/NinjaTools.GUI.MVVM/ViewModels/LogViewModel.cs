using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NinjaTools.Logging;

namespace NinjaTools.GUI.MVVM.ViewModels
{
    public class LogLine : INotifyPropertyChanged
    {
        public LogLevel Level     { get; set; }
        public string   Line      { get; set; }
        public DateTime Timestamp { get; set; }

#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
    }

    public interface ILogLineFactory
    {
        LogLine CreateLogLine(LogEventArgs e);
    }

    public class DefaultLogLineFactory : ILogLineFactory
    {
        public virtual LogLine CreateLogLine(LogEventArgs e)
        {
            return new LogLine
            {
                Level     = e.Level,
                Line      = e.Message,
                Timestamp = e.Timestamp
            };
        }
    }

    public class LogViewModel : BaseViewModel, IActivate, IDeactivate, IHaveDisplayName
    {
        public string DisplayName { get; } = "Log";

        private readonly ILogProviderFactory _log;
        private readonly ILogLineFactory _formatter;
        private          bool                _isActive = false;

        private ILogProvider _logger;
        public  bool         IsEnabled      { get; set; } = true;
        public  bool         LogIfNonActive { get; set; } = false;

        public LogLevel MinLogLevel { get; set; }
        public bool     IsVerbose   { get; set; }

        public ObservableCollection<LogLine> Log { get; private set; }

        public LogLine SelectedLine { get; set; }

        public LogViewModel(ILogProviderFactory log, ILogLineFactory logLineFactory = null)
        {
            _log        = log;
            _formatter  = logLineFactory ?? new DefaultLogLineFactory();
            Log         = new ObservableCollection<LogLine>();
            MinLogLevel = LogLevel.Info;
        }

        public bool CanClear => Log.Count > 0;

        public void Clear()
        {
            Log.Clear();
        }

        private void OnLogIfNonActiveChanged()
        {
            OnIsEnabledChanged();
        }

        private void OnIsEnabledChanged()
        {
            bool shouldLog = IsEnabled && (_isActive || LogIfNonActive);

            if (shouldLog && _logger == null)
            {
                _logger     =  _log.Create("*", LogLevel.Trace);
                _logger.Log += AddLogLine;
            }
            else if (!shouldLog && _logger != null)
            {
                _logger.Log -= AddLogLine;
                _logger.Close();
                _logger = null;
            }
        }

        private void AddLogLine(object sender, LogEventArgs e)
        {
            if (e.Level < MinLogLevel && !IsVerbose) return;

            var logLine = _formatter.CreateLogLine(e);

            InvokeOnMainThread(() => Log.Add(logLine));
            RaisePropertyChanged(nameof(CanClear));
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