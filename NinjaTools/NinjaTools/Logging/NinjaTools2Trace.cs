using System;
using System.Reflection;

namespace NinjaTools.Logging
{
    /// <summary>
    /// usage: call Register() once in your app, at startup. 
    /// 
    /// NOTE: this file should be included 
    ///       as reference into your main project.
    /// </summary>
    public class NinjaTools2Trace : ILogManager
    {
        public static void Register()
        {
            LogManager.Instance = new NinjaTools2Trace();
        }

        public ILogger GetLogger(string name)
        {
            return new TraceLogger(name);
        }

        public ILogger GetLogger(Type type)
        {
            return new TraceLogger(type.FullName);
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

        public class TraceLogger : ILogger
        {
            private readonly string _name;

            public TraceLogger(string name)
            {
                _name = name;
            }

            public void Trace(string format, params object[] args)
            {
                System.Diagnostics.Trace.TraceInformation(format, args);
            }

            public void Info(string format, params object[] args)
            {
                System.Diagnostics.Trace.TraceInformation(format, args);
            }

            public void Warn(string format, params object[] args)
            {
                System.Diagnostics.Trace.TraceWarning(format, args);
            }

            public void Debug(string format, params object[] args)
            {
                System.Diagnostics.Trace.TraceInformation(format, args);
            }

            public void Error(string format, params object[] args)
            {
                System.Diagnostics.Trace.TraceError(format, args);
            }

            public void Trace(string msg)
            {
                System.Diagnostics.Trace.TraceInformation(msg);
            }

            public void Info(string msg)
            {
                System.Diagnostics.Trace.TraceInformation(msg);
            }

            public void Warn(string msg)
            {
                System.Diagnostics.Trace.TraceWarning(msg);
            }

            public void Debug(string msg)
            {
                System.Diagnostics.Trace.TraceInformation(msg);
            }

            public void Error(string msg)
            {
                System.Diagnostics.Trace.TraceError(msg);
            }

            public void Error(Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
            }

            public void Error(string msg, Exception ex)
            {
                System.Diagnostics.Trace.TraceError(msg + "\n" + ex.ToString());
            }

            public bool IsTraceEnabled => true;
            public bool IsDebugEnabled => true;
            public bool IsInfoEnabled => true;
            public bool IsWarnEnabled => true;
            public bool IsErrorEnabled => true;
        }

    }
}
