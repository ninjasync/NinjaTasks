#define FEATURE_FRAGMENTS

using System;
using Android.Content;
using Android.Util;
using NinjaTools.Droid.MvvmCross;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public abstract class BaseLifecycleTrackingControl :  BindingFrameLayout
    {
        public BaseView Activity { get; private set; }

#if FEATURE_FRAGMENTS
        public BaseFragmentView FragmentActivity { get; private set; }
#endif

        public LifecycleState LifecycleState { get; private set; }

        

        

        public BaseLifecycleTrackingControl(int templateId, Context context, IAttributeSet attrs) 
               : base(templateId, context, attrs)
        {
        }

        protected virtual void OnLifecycleChanged(LifecycleState prevState)
        {
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void OnResume()
        {
        }

        protected virtual void OnPause()
        {
        }

        protected virtual void OnStop()
        {
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            var newState = LifecycleState.None;

            var ctx = Context;
            Activity = ctx as BaseView;
            
            if (Activity != null)
            {
                Activity.ResumeCalled += OnResume;
                Activity.PauseCalled += OnPause;
                Activity.StopCalled += OnStop;
                Activity.DestroyCalled += OnDestroy;
                newState = Activity.LifecycleState;
            }

#if FEATURE_FRAGMENTS
            FragmentActivity = ctx as BaseFragmentView;

            if (FragmentActivity != null)
            {
                FragmentActivity.ResumeCalled += OnResume;
                FragmentActivity.PauseCalled += OnPause;
                FragmentActivity.StopCalled += OnStop;
                FragmentActivity.DestroyCalled += OnDestroy;
                newState = FragmentActivity.LifecycleState;
            }
#endif

            SetLifecycleState(newState);
        }

        


        protected override void OnDetachedFromWindow()
        {
            if (Activity != null)
            {
                Activity.ResumeCalled -= OnResume;
                Activity.PauseCalled -= OnPause;
                Activity.StopCalled -= OnStop;
                Activity.DestroyCalled -= OnDestroy;
            }

#if FEATURE_FRAGMENTS
            if (FragmentActivity != null)
            {
                FragmentActivity.ResumeCalled -= OnResume;
                FragmentActivity.PauseCalled -= OnPause;
                FragmentActivity.StopCalled -= OnStop;
                FragmentActivity.DestroyCalled -= OnDestroy;
            }
#endif

            base.OnDetachedFromWindow();

            SetLifecycleState(LifecycleState.Destroyed);
        }

        private void OnDestroy(object sender, EventArgs e)
        {
            SetLifecycleState(LifecycleState.Destroyed);
        }

        private void OnResume(object sender, EventArgs e)
        {
            SetLifecycleState(LifecycleState.Resumed);
        }

        private void OnPause(object sender, EventArgs e)
        {
            SetLifecycleState(LifecycleState.Paused);
        }

        private void OnStop(object sender, EventArgs e)
        {
            SetLifecycleState(LifecycleState.Stopped);
        }

        private void SetLifecycleState(LifecycleState newState)
        {
            var prevState = LifecycleState;
            LifecycleState = newState;

            if (prevState != LifecycleState.None)
            {
                if (newState == LifecycleState.Resumed)
                    OnResume();
                else if (prevState < LifecycleState.Paused && newState >= LifecycleState.Paused)
                    OnPause();
                else if (prevState < LifecycleState.Stopped && newState >= LifecycleState.Stopped)
                    OnStop();
                else if (prevState < LifecycleState.Destroyed && newState >= LifecycleState.Destroyed)
                    OnDestroy();
            }

            OnLifecycleChanged(prevState);
        }
    }
}
