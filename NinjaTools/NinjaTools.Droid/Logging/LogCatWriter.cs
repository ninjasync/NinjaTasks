using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Java.IO;
using Java.Lang;

namespace NinjaTools.Droid.Logging
{
    public class LogCatWriter : IDisposable
    {
        private Process mLogcatProcess = null;
        public bool IsRunning { get { return mLogcatProcess != null; }}

        /// <summary>
        /// you will have to have  android.permission.READ_LOGS permission
        /// </summary>
        public LogCatWriter(Context context, params string[] logTags)
        {
            try
            {
                string filter = string.Join(" ", logTags.Select(tag => tag + ":V"));
                File path = context.FilesDir;
                mLogcatProcess = Runtime.GetRuntime().Exec(new string[]
                {
                    "logcat", "-f",  path.AbsolutePath + "/log.log",
                    "-v", "time",
                    "AndroidRuntime:E " + filter + " *:S",
                });
            }
            catch (System.Exception)
            {
                mLogcatProcess = null;
            }
//        reader = new BufferedReader(new InputStreamReader
//(mLogcatProcess.getInputStream()));

//        String line;
//        final StringBuilder log = new StringBuilder();
//        String separator = System.getProperty("line.separator"); 

//        while ((line = reader.readLine()) != null)
//        {
//                log.append(line);
//                log.append(separator);
//        }

//        // do whatever you want with the log.  I'd recommend using Intents to create an email
            //}

            //catch (IOException e)
            //{
            //}

            //finally
            //{
            //        if (reader != null)
            //                try
            //                {
            //                        reader.close();
            //                }
            //                catch (IOException e)
            //                {
            //                        ...
            //                }

            //} 

        }

        public void Dispose()
        {
            if(mLogcatProcess != null)
                mLogcatProcess.Destroy();
            mLogcatProcess = null;
        }
    }
}
