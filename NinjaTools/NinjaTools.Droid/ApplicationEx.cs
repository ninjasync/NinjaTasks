using System;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace NinjaTools.Droid
{
    public class ApplicationEx : Application,
                Application.IActivityLifecycleCallbacks,
                IIsAppActive
    {
        private readonly HiddenReference<PropertyChangedEventHelper> _event = new HiddenReference<PropertyChangedEventHelper>();
        public bool IsInForeground { get; private set; }

        public ApplicationEx(IntPtr handle, JniHandleOwnership owner)
            : base(handle, owner)
        {
            _event.Value = new PropertyChangedEventHelper();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            RegisterActivityLifecycleCallbacks(this);
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            if (level >= TrimMemory.Complete)
                GC.Collect();

            if (level == TrimMemory.UiHidden)
            {
                IsInForeground = false;
                OnPropertyChanged(nameof(IsInForeground));
            }

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
            OnPropertyChanged(nameof(IsInForeground));
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
            //var trace = Mvx.IoCProvider.Resolve<IMvxTrace>();
            //if (trace != null)
            //    trace.Trace(MvxTraceLevel.Diagnostic, "livecycle", p1, format);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            _event.Value.OnPropertyChanged(propertyName);
        }

        public class PropertyChangedEventHelper : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public virtual void OnPropertyChanged(string propName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged 
        {   
            add => _event.Value.PropertyChanged += value;
            remove => _event.Value.PropertyChanged -= value;
        }
    }
}
