using System;
using System.Collections.Generic;
using System.Linq;
using Android.Util;
using NLog;
using NLog.Targets;

namespace NinjaTools.Droid.Logging
{
    [Target("LogCat")]
    public sealed class NLogLogCatTarget : TargetWithLayout
    {
        public static void Register()
        {
            Target.Register<NLogLogCatTarget>("LogCat");
        }

        public NLogLogCatTarget()
        {
            this.Tag = "Ninja";
        }

        //[RequiredParameter]
        public string Tag { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            string logMessage = this.Layout.Render(logEvent);

            SendTheMessageToLogCat(this.Tag, logMessage, logEvent.Level);
        }

        private void SendTheMessageToLogCat(string tag, string message, LogLevel level)
        {
            if(string.Equals(level.Name, "trace", StringComparison.OrdinalIgnoreCase))
                Log.Verbose(tag, message);
            else if (string.Equals(level.Name, "debug", StringComparison.OrdinalIgnoreCase))
                Log.Debug(tag, message);
            else if (string.Equals(level.Name, "info", StringComparison.OrdinalIgnoreCase))
                Log.Info(tag, message);
            else if (string.Equals(level.Name, "warn", StringComparison.OrdinalIgnoreCase))
                Log.Warn(tag, message);
            else if (string.Equals(level.Name, "erro", StringComparison.OrdinalIgnoreCase))
                Log.Error(tag, message);
            else if (string.Equals(level.Name, "fatal", StringComparison.OrdinalIgnoreCase))
                Log.Wtf(tag, message);
        }
    }
}