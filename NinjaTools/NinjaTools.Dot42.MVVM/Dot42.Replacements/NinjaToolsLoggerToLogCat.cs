using System;
using System.Threading;
using Android.Util;
using NinjaTools.Logging;

namespace NinjaTools.Droid
{
    /// <summary>
    /// usage: call Register() once in your app, at startup. 
    /// 
    /// NOTE: this file should be included 
    ///       as reference into your main project.
    /// </summary>
    public class NinjaToolsLoggerToLogCat : ILogManager
    {
        public static void Register()
        {
            LogManager.Instance = new NinjaToolsLoggerToLogCat();
        }

        protected NinjaToolsLoggerToLogCat()
        {

        }
        public ILogger GetLogger(string name)
        {
            return new LoggerWrapper(name, this);
        }

        public ILogger GetLogger(Type type)
        {
            return new LoggerWrapper(type.FullName, this);
        }

        public event EventHandler<LogEventArgs> Logged;

        private void WriteLog(LogLevel type, string logger, string tag, string format, object[] args, Func<string, string, int> log)
        {
#if !DEBUG
            if (type < LogLevel.Warn) return;
#endif
            string msg;
            if (args == null || args.Length == 0)
                msg = format;
            else
            {
                // never propagate a format exception.
                try { msg = string.Format(format, args); }
                catch (Exception) { msg = format; }
            }

            msg = string.Format("{0:000}|{1}|{2}", Thread.CurrentThread.Id, logger, msg);
            
            log(tag, msg);

            var h = Logged;
            if (h != null)
                h(this, new LogEventArgs { Level = type, Message = msg, Timestamp = DateTime.Now, Logger = logger });
        }

        private void WriteLog(LogLevel type, string logger, string tag, string format, object[] args, Func<string, string, Exception, int> log, Exception ex)
        {
#if !DEBUG
            if (type < LogLevel.Warn) return;
#endif
            string msg;
            if (args == null || args.Length == 0)
                msg = format;
            else
            {
                // never propagate a format exception.
                try { msg = string.Format(format, args); }
                catch (Exception) { msg = format; }
            }

            //log("({0}): {1}", new object[] { Thread.CurrentThread.ManagedThreadId, msg });
            log(tag, msg, ex);

            var h = Logged;
            if (h != null)
                h(this, new LogEventArgs { Level = type, Message = msg, Timestamp = DateTime.Now, Logger = logger });
        }

        /// <summary>
        /// Gets the logger named after the currently-being-initialized class.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// The logger.
        /// </returns>
        /// 
        /// <remarks>
        /// This is a slow-running method.
        ///             Make sure you're not doing this in a loop.
        /// </remarks>
        public ILogger GetCurrentClassLogger()
        {
            int index = 1;
            string name = "default";
            var stackTrace = new Exception().JavaStackTrace;
            do
            {
                if (index >= stackTrace.Length)
                    break;

                ++index;
                name = stackTrace[index].ClassName;
            }
            while (name != null 
                && (name.StartsWith("NinjaTools.Logging", StringComparison.OrdinalIgnoreCase)
                || (name.StartsWith("ninjaTools_Dot42.NinjaTools.Logging", StringComparison.OrdinalIgnoreCase))));

            return GetLogger(name);
        }

        private class LoggerWrapper : ILogger
        {
            private readonly string _name;
            private readonly NinjaToolsLoggerToLogCat _target;
            private readonly string _tag;

            public LoggerWrapper(string name, NinjaToolsLoggerToLogCat target)
            {
                _name = name;
                _target = target;
                //_tag = "ninja." + name.Split('.')[0]; // only use first part of namespace..
                _tag = "ninja";
            }

            public void Trace(string format, params object[] args)
            {
                if(IsTraceEnabled)
                    _target.WriteLog(LogLevel.Trace, _name, _tag , format, args, Log.V);
            }

            public void Trace(string msg)
            {
                if (IsTraceEnabled)
                    _target.WriteLog(LogLevel.Trace, _name, _tag, msg, null, Log.E);
            }

            public void Info(string format, params object[] args)
            {
                if(IsInfoEnabled)
                    _target.WriteLog(LogLevel.Info, _name, _tag, format, args, Log.I);
            }

            public void Info(string msg)
            {
                if (IsInfoEnabled)
                    _target.WriteLog(LogLevel.Info, _name, _tag, msg, null, Log.I);
            }

            public void Debug(string format, params object[] args)
            {
                if (IsDebugEnabled)
                    _target.WriteLog(LogLevel.Debug, _name, _tag, format, args, Log.D);
            }

            public void Debug(string msg)
            {
                if (IsDebugEnabled)
                    _target.WriteLog(LogLevel.Debug, _name, _tag, msg, null, Log.D);
            }

            public void Warn(string format, params object[] args)
            {
                if (IsWarnEnabled)
                    _target.WriteLog(LogLevel.Warn, _name, _tag, format, args, Log.W);
            }

            public void Warn(string msg)
            {
                if (IsWarnEnabled)
                    _target.WriteLog(LogLevel.Warn, _name, _tag, msg, null, Log.W);
            }

            public void Error(string format, params object[] args)
            {
                _target.WriteLog(LogLevel.Error, _name, _tag, format, args, Log.E);
            }

            public void Error(string msg)
            {
                _target.WriteLog(LogLevel.Error, _name, _tag, msg, null, Log.E);
            }

            public void Error(Exception ex)
            {
                _target.WriteLog(LogLevel.Error, _name, _tag, "", null, Log.E, ex);
            }

            public void Error(string msg, Exception ex)
            {
                _target.WriteLog(LogLevel.Error, _name, _tag, msg, null, Log.E, ex);
            }

            public bool IsTraceEnabled { get { return Log.IsLoggable(_tag, LogPriority.Verbose); } }
            public bool IsDebugEnabled { get { return Log.IsLoggable(_tag, LogPriority.Debug); } }
            public bool IsInfoEnabled { get { return Log.IsLoggable(_tag, LogPriority.Info); } }
            public bool IsWarnEnabled { get { return Log.IsLoggable(_tag, LogPriority.Warn); } }
            public bool IsErrorEnabled { get { return Log.IsLoggable(_tag, LogPriority.Error); } }
        }
    }

}
