using System;
using System.Collections.Generic;
using System.Linq;

namespace NinjaTools.Logging
{
    public class LogManager : ILogManager
    {
        //public static ILogManager Instance = new LogManager();
        public static ILogManager Instance = new NinjaTools2NLog();

        static LogManager()
        {
        }

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