using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.OS;

namespace NinjaTools.Droid
{
    public class ApplicationEx : Application,
                Application.IActivityLifecycleCallbacks,
                IIsAppActive
    {
        public bool IsInForeground { get; private set; }

        public override void OnCreate()
        {
            base.OnCreate();
            RegisterActivityLifecycleCallbacks(this);
        }

        public override void OnTrimMemory(int level)
        {
            if (level >= IComponentCallbacks2Constants.TRIM_MEMORY_COMPLETE)
                GC.Collect();
            if (level == IComponentCallbacks2Constants.TRIM_MEMORY_UI_HIDDEN)
                IsInForeground = false;

            base.OnTrimMemory(level);
        }

        public void OnActivityPaused(Activity activity)
        {
            Trace("OnActivityPaused: {0}", activity.GetType().Name);
            //IsInForeground = false;
        }

        public void OnActivityResumed(Activity activity)
        {
            Trace("OnActivityResumed: {0}", activity.GetType().Name);
            IsInForeground = true;
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            Trace("OnActivityCreated: {0}", activity.GetType().ToString());
        }

        public void OnActivityDestroyed(Activity activity)
        {
            Trace("OnActivityDestroyed: {0}", activity.GetType().Name);
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
            Trace("OnActivitySaveInstanceState: {0}", activity.GetType().Name);
        }

        public void OnActivityStarted(Activity activity)
        {
            Trace("OnActivityStarted: {0}", activity.GetType().Name);
        }

        public void OnActivityStopped(Activity activity)
        {
            Trace("OnActivityStopped: {0}", activity.GetType().Name);
        }

        private void Trace(string p1, params object[] format)
        {
            //var trace = Mvx.Resolve<IMvxTrace>();
            //if (trace != null)
            //    trace.Trace(MvxTraceLevel.Diagnostic, "livecycle", p1, format);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}