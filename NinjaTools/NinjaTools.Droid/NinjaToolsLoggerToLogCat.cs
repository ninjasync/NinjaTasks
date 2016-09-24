using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Android.Util;
using NinjaTools.Logging;

#if !DOT42
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

        private void WriteLog(LogLevel type, string name, string tag, string format, object[] args, Func<string, string, object[], int> func)
        {
#if !DEBUG
            if (type < LogLevel.Warn) return;
#endif
            string msg;
            // never propagate a format exception.
            if (args == null || args.Length == 0)
                msg = format;
            else
            { 
                try { msg = string.Format(format, args); }
                catch (Exception) { msg = format; }
            }

            func(tag, "({0}): {1}", new object[] { Thread.CurrentThread.ManagedThreadId, msg });

            var h = Logged;
            if(h != null)
                h(this, new LogEventArgs { Level = type, Message=msg, Timestamp = DateTime.Now, Logger=name});

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
            Type declaringType;
            string name;
            do
            {
                MethodBase method = new StackTrace().GetFrame(index).GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    name = method.Name;
                    break;
                }
                else
                {
                    ++index;
                    name = declaringType.FullName;
                }
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase)
               || (declaringType.Namespace != null && declaringType.Namespace.StartsWith("NinjaTools.Logging")));

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
                _tag = "ninja." + name.Split('.')[0]; // only use first part of namespace..
            }

            public void Trace(string format, params object[] args)
            {
                Write(LogLevel.Trace, format, args, Log.Verbose);
            }


            public void Info(string format, params object[] args)
            {
                Write(LogLevel.Info, format, args, Log.Info);
            }

            public void Warn(string format, params object[] args)
            {
                Write(LogLevel.Warn, format, args, Log.Warn);
            }

            public void Debug(string format, params object[] args)
            {
                Write(LogLevel.Debug, format, args, Log.Debug);
            }


            public void Error(string format, params object[] args)
            {
                Write(LogLevel.Error, format, args, Log.Error);
            }

            public void Trace(string msg)
            {
                Write(LogLevel.Trace, msg, null, Log.Verbose);
            }

            public void Info(string msg)
            {
                Write(LogLevel.Info, msg, null, Log.Info);
            }

            public void Warn(string msg)
            {
                Write(LogLevel.Warn, msg, null, Log.Warn);
            }

            public void Debug(string msg)
            {
                Write(LogLevel.Debug, msg, null, Log.Debug);
            }

            public void Error(string msg)
            {
                Write(LogLevel.Error, msg, null, Log.Error);
            }

            public void Error(Exception ex)
            {
                Write(LogLevel.Error, "{0}: {1}:\n{2}", new object[] { ex.GetType().Name, ex.Message, ex.StackTrace }, Log.Error);
            }

            public void Error(string msg, Exception ex)
            {
                Write(LogLevel.Error, "{0}:\n{1}: {2}:\n{3}", new object[] { msg, ex.GetType().Name, ex.Message, ex.StackTrace }, Log.Error);
            }

            private void Write(LogLevel type, string format, object[] args, Func<string, string, object[], int> func)
            {
                _target.WriteLog(type, _name, _tag, format, args, func);
            }


            public bool IsTraceEnabled { get { return Log.IsLoggable(_tag, LogPriority.Verbose); } }
            public bool IsDebugEnabled { get { return Log.IsLoggable(_tag, LogPriority.Debug); } }
            public bool IsInfoEnabled { get { return Log.IsLoggable(_tag, LogPriority.Info); ; } }
            public bool IsWarnEnabled { get { return Log.IsLoggable(_tag, LogPriority.Warn); ; } }
            public bool IsErrorEnabled { get { return Log.IsLoggable(_tag, LogPriority.Error); ; } }
        }
    }

}
#endif