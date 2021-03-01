using System;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace NinjaTools.Logging
{
    public class NLogLogProviderFactory : ILogProviderFactory
    {
        public ILogProvider Create(string loggerNameWithWildcards, LogLevel minLogLevel)
        {
            return new NLogLogTarget(loggerNameWithWildcards, minLogLevel);
            //return null;
        }

        private class NLogLogTarget : TargetWithContext, ILogProvider
        {
            private readonly LogLevel _minLogLevel;
            private Guid _guid;
            private LoggingRule _rule;

            public NLogLogTarget(string loggerNameWithWildcards, LogLevel minLogLevel)
            {
                _minLogLevel = minLogLevel;
                Register(loggerNameWithWildcards);
                //this.IncludeGdc = true;
                this.IncludeMdc  = true;
                this.IncludeMdlc = true;
            }

            private void Register(string loggerNameWithWildcards)
            {
                LoggingConfiguration config = NLog.LogManager.Configuration;

                _guid = new Guid();
                config.AddTarget(_guid.ToString(), this);
                //_layout = new SimpleLayout("${longdate} ${uppercase:${level}} ${message}");
                NLog.LogLevel logLevel = NLog.LogLevel.FromString(_minLogLevel.ToString());

                _rule = new LoggingRule(loggerNameWithWildcards, logLevel, this);
                Layout = "${message}";
                // insert at beginning!
                config.LoggingRules.Insert(0, _rule);

                //LogManager.Configuration = config;
                NLog.LogManager.ReconfigExistingLoggers();
            }


            private void Unregister()
            {
                LoggingConfiguration config = NLog.LogManager.Configuration;
                config.LoggingRules.Remove(_rule);
                config.RemoveTarget(_guid.ToString());
                NLog.LogManager.Configuration = config;
                NLog.LogManager.ReconfigExistingLoggers();
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
                if (!Enum.TryParse(logEvent.Level.Name, true, out LogLevel level))
                    level = LogLevel.Error;

                h(this, new LogEventArgs
                {
                    Level      = level,
                    Timestamp  = logEvent.TimeStamp,
                    Message    = Layout.Render(logEvent),
                    Properties = GetAllProperties(logEvent).ToDictionary(kv => kv.Key.ToString(), kv => kv.Value)
                });
            }
        }
    }
}
