using System;
using System.Diagnostics;
using System.Reflection;
using NLog;

namespace NinjaTools.Logging
{
    /// <summary>
    /// usage: call Register() once in your app, at startup. 
    /// 
    /// NOTE: this file should be included 
    ///       as reference into your main project.
    /// </summary>
    public class NinjaTools2NLog : ILogManager
    {
        public static void Register()
        {
            LogManager.Instance = new NinjaTools2NLog();
        }

        internal NinjaTools2NLog()
        {

        }
        public ILogger GetLogger(string name)
        {
            return new LoggerWrapper(NLog.LogManager.GetLogger(name));
        }

        public ILogger GetLogger(Type type)
        {
            return new LoggerWrapper(NLog.LogManager.GetLogger(type.FullName));
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
               || (declaringType.Namespace != null && declaringType.Namespace.StartsWith("NinjaTools.Logging", StringComparison.Ordinal)));

            return GetLogger(name);
        }

        private class LoggerWrapper : ILogger
        {
            private readonly Logger _logger;

            public LoggerWrapper(Logger logger)
            {
                _logger = logger;
            }

            public LoggerWrapper(Type type)
            {
                _logger = NLog.LogManager.GetLogger(type.FullName);
            }

            public void Trace(string format, params object[] args)
            {
                _logger.Trace(format, args);
            }

            public void Info(string format, params object[] args)
            {
                _logger.Info(format, args);
            }

            public void Warn(string format, params object[] args)
            {
                _logger.Warn(format, args);
            }

            public void Debug(string format, params object[] args)
            {
                _logger.Debug(format, args);
            }

            public void Error(string format, params object[] args)
            {
                _logger.Error(format, args);
            }

            public void Trace(string msg)
            {
                _logger.Trace(msg);
            }

            public void Info(string msg)
            {
                _logger.Info(msg);
            }

            public void Warn(string msg)
            {
                _logger.Warn(msg);
            }

            public void Debug(string msg)
            {
                _logger.Debug(msg);
            }

            public void Error(string msg)
            {
                _logger.Error(msg);
            }

            public void Error(Exception ex)
            {
                _logger.Error(ex);
            }

            public void Error(string msg, Exception ex)
            {
                _logger.Error(ex, msg);
            }

            public void Error(Exception ex, string msg)
            {
                _logger.Error(ex, msg);
            }


            public bool IsTraceEnabled { get { return _logger.IsTraceEnabled; } }
            public bool IsDebugEnabled { get { return _logger.IsDebugEnabled; } }
            public bool IsInfoEnabled { get { return _logger.IsInfoEnabled; } }
            public bool IsWarnEnabled { get { return _logger.IsWarnEnabled; } }
            public bool IsErrorEnabled { get { return _logger.IsErrorEnabled; } }
        }

    }

}
