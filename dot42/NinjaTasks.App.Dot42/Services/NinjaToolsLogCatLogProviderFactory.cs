using System;
using NinjaTools.Droid;
using NinjaTools.Logging;

namespace NinjaTasks.App.Droid.Services
{
    public class NinjaToolsLogCatLogProviderFactory : ILogProviderFactory
    {
        public ILogProvider Create(string loggerNameWithWildcards, LogLevel minLogLevel)
        {
            return new LogProvider(loggerNameWithWildcards, minLogLevel);
        }
    }

    public class LogProvider : ILogProvider, IDisposable
    {
        public LogProvider(string loggerNameWithWildcards, LogLevel minLogLevel)
        {
            var lc = LogManager.Instance as NinjaToolsLoggerToLogCat;
            if (lc == null) return;
            lc.Logged += OnLogged;
        }

        private void OnLogged(object sender, LogEventArgs e)
        {
            var h = Log;
            if (h != null)
                h(this, e);
        }

        public event EventHandler<LogEventArgs> Log;

        public void Close()
        {
            var lc = LogManager.Instance as NinjaToolsLoggerToLogCat;
            if (lc == null) return;
            lc.Logged -= OnLogged;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
