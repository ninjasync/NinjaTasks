using System;
using Android.Content;
using Android.Util;
using Android.Views;
using NinjaTasks.App.Droid.Views.Utils;
using NinjaTools.Droid.MvvmCross;

namespace NinjaTasks.App.Droid.Views.Controls
{
    /// <summary>
    /// This resembles a poor man's fragment. It will call into the ViewModels IActivate/IDeativate
    /// Implementations based on the owning Activity/Fragment lifecycle and it own visibility status.
    /// </summary>
    public class BaseControlView : BaseLifecycleTrackingControl
    {
        private readonly LifecycleToViewModelActivation _lifecycle = new LifecycleToViewModelActivation();

        public BaseControlView(int templateId, Context context, IAttributeSet attrs) : base(templateId, context, attrs)
        {
            BindingContext.DataContextChanged += OnDataContextChanged;
        }

        public object ViewModel
        {
            get { return DataContext; }
            set 
            { 
                var prev = DataContext; 

                DataContext = value; 
                _lifecycle.SetDataContext(value); 

                if(!ReferenceEquals(prev, value))
                    OnViewModelSet(value, prev); 
            }
        }


        private void OnDataContextChanged(object sender, EventArgs e)
        {
            _lifecycle.SetDataContext(BindingContext.DataContext);
        }

        protected override void OnVisibilityChanged(View changedView, int visibility)
        {
            base.OnVisibilityChanged(changedView, visibility);
            _lifecycle.SetVisibility(Visibility == View.VISIBLE);
            
        }

        protected override void OnLifecycleChanged(LifecycleState prevState)
        {
            base.OnLifecycleChanged(prevState);
            _lifecycle.SetLifecycle(LifecycleState);
        }

        protected virtual void OnViewModelSet(object newViewModel, object previousViewModel)
        {
        }
    }

    public class BaseControlView<T> : BaseControlView where T : class
    {
        public BaseControlView(int templateId, Context context, IAttributeSet attrs) : base(templateId, context, attrs)
        {
        }

        public new T ViewModel
        {
            get { return (T)base.ViewModel; }
            set { base.ViewModel = value; } 
        }

        protected sealed override void OnViewModelSet(object newViewModel, object previousViewModel)
        {
            OnViewModelSet((T)newViewModel, (T)previousViewModel);
        }

        protected virtual void OnViewModelSet(T newViewModel, T previousViewModel)
        {
        }
    }
}
