using System;
using Android.Util;
using Cirrious.CrossCore.Platform;

namespace NinjaTools.Droid.MvvmCross
{
    public class LogTrace : IMvxTrace
    {
        public void Trace(MvxTraceLevel level, string tag, Func<string> message)
        {
            Trace(level, tag, message());
        }
        public void Trace(MvxTraceLevel level, string tag, string message)
        {
            tag = "ninja." + tag;
            if(level == MvxTraceLevel.Error)
                Log.Error(tag, message);
            else if(level == MvxTraceLevel.Warning)
                Log.Warn(tag, message);
            else
                Log.Info(tag, message);
        }

        public void Trace(MvxTraceLevel level, string tag, string message, params object[] args)
        {
            try
            {
                Trace(level, tag, string.Format(message, args));
            }
            catch (FormatException)
            {
                Trace(MvxTraceLevel.Error, tag, "FormatException during trace");
                Trace(level, tag, message);
            }
        }
    }
}