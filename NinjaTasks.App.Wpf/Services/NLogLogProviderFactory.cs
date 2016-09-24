using System;
using NinjaTools.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;
using LogLevel = NinjaTools.Logging.LogLevel;
using LogManager = NLog.LogManager;

namespace NinjaTasks.App.Wpf.Services
{
    public class NLogLogProviderFactory : ILogProviderFactory
    {
        public ILogProvider Create(string loggerNameWithWildcards, LogLevel minLogLevel )
        {
            return new NLogLogTarget(loggerNameWithWildcards, minLogLevel);
            //return null;
        }

        private class NLogLogTarget : TargetWithLayout, ILogProvider
        {
            private readonly LogLevel _minLogLevel;
            private Guid _guid;
            private LoggingRule _rule;

            public NLogLogTarget(string loggerNameWithWildcards, LogLevel minLogLevel)
            {
                _minLogLevel = minLogLevel;
                Register(loggerNameWithWildcards);
            }

            private void Register(string loggerNameWithWildcards)
            {
                LoggingConfiguration config = LogManager.Configuration;

                _guid = new Guid();
                config.AddTarget(_guid.ToString(), this);
                //_layout = new SimpleLayout("${longdate} ${uppercase:${level}} ${message}");
                NLog.LogLevel logLevel = NLog.LogLevel.FromString(_minLogLevel.ToString());

                _rule = new LoggingRule(loggerNameWithWildcards, logLevel, this);
                Layout = "${message}";
                // insert at beginning!
                config.LoggingRules.Insert(0, _rule);

                //LogManager.Configuration = config;
                LogManager.ReconfigExistingLoggers();
            }


            private void Unregister()
            {
                LoggingConfiguration config = LogManager.Configuration;
                config.LoggingRules.Remove(_rule);
                config.RemoveTarget(_guid.ToString());
                LogManager.Configuration = config;
                LogManager.ReconfigExistingLoggers();
            }

            public event EventHandler<LogEventArgs> Log;

            public void Close()
            {
                if (_rule != null)
                    Unregister();
                _rule = null;
            }



            protected override void Write(LogEventInfo logEvent)
            {
                var h = Log;
                if (h == null) return;
                LogLevel level;
                if (!Enum.TryParse(logEvent.Level.Name, true, out level))
                    level = LogLevel.Error;

                h(this, new LogEventArgs
                {
                    Level = level,
                    Timestamp = logEvent.TimeStamp,
                    Message = Layout.Render(logEvent)
                });

            }
        }
    
    }

    
}
