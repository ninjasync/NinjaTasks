using System;
using JetBrains.Annotations;

namespace NinjaTools.Logging
{
    public interface ILogManager
    {
        ILogger GetLogger(string name);
        ILogger GetLogger(Type type);

        ILogger GetCurrentClassLogger();
    }


    /// <summary>
    /// Yes.I know.
    /// I am not proud to create yet another logging interface. but: NLog does not yet
    /// have a portable class library, so for now it is necessary.
    /// if possible: use nlog directly.
    /// </summary>
    public interface  ILogger
    {
        [StringFormatMethod("format")]
        void Trace(string format, params object[] args);
        [StringFormatMethod("format")]
        void Info(string format, params object[] args);
        [StringFormatMethod("format")]
        void Warn(string format, params object[] args);
        [StringFormatMethod("format")]
        void Debug(string format, params object[] args);
        [StringFormatMethod("format")]
        void Error(string format, params object[] args);

        void Trace(string msg);
        void Info(string msg);
        void Warn(string msg);
        void Debug(string msg);
        void Error(string msg);

        void Error(Exception ex);
        void Error(string msg, Exception ex);

        bool IsTraceEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }

    }

    public class NullLogger : ILogger
    {
        public void Trace(string format, params object[] args)
        {
        }
        public void Debug(string format, params object[] args)
        {
        }

        public void Info(string format, params object[] args)
        {
        }

        public void Warn(string format, params object[] args)
        {
        }

        public void Error(string format, params object[] args)
        {
        }

        public void Trace(string msg)
        {
        }

        public void Info(string msg)
        {
        }

        public void Warn(string msg)
        {
        }

        public void Debug(string msg)
        {
        }

        public void Error(string msg)
        {
        }

        public void Error(Exception ex)
        {
        }

        public void Error(string msg, Exception ex)
        {
        }

        public bool IsTraceEnabled { get { return false; } }
        public bool IsDebugEnabled { get { return false; } }
        public bool IsInfoEnabled { get { return false; } }
        public bool IsWarnEnabled { get { return false; } }
        public bool IsErrorEnabled { get { return false; } }
    }

    public class LogManager : ILogManager
    {
        public static ILogManager Instance = new LogManager();

        public static ILogger GetLogger(string name)
        {
            return Instance.GetLogger(name);
        }

        public static ILogger GetLogger(Type type)
        {
            return Instance.GetLogger(type);
        }

        public static ILogger GetCurrentClassLogger()
        {
            return Instance.GetCurrentClassLogger();
        }

        ILogger ILogManager.GetCurrentClassLogger()
        {
            return new NullLogger();
        }


        ILogger ILogManager.GetLogger(string name)
        {
            return new NullLogger();
        }

        ILogger ILogManager.GetLogger(Type type)
        {
            return new NullLogger();
        }
    }
}
