using System;

namespace NinjaTools.Logging
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error
    }

    public class LogEventArgs : EventArgs
    {
        public LogLevel Level { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string Logger { get; set; }
    }

    public interface ILogProvider
    {
        event EventHandler<LogEventArgs> Log;

        void Close();
    }

    public interface ILogProviderFactory
    {
        ILogProvider Create(string loggerNameWithWildcards="*", LogLevel minLogLevel= LogLevel.Trace);
    }

}
