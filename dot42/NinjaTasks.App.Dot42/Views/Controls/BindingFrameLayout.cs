using System;
using Android.Content;
using Android.Util;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public class BindingFrameLayout : MvxFrameControl
    {
        private bool _delayInflated;

        public event EventHandler FinishDelayInflate;

        protected BindingFrameLayout(int templateId, Context context, IAttributeSet attrs) 
            : base(templateId, context, attrs)
        {
            this.DelayBind(() =>
            {
                if (!_delayInflated)
                    OnFinishDelayInflate();
                _delayInflated = true;
            });
        }

        protected virtual void OnFinishDelayInflate()
        {
            var handler = FinishDelayInflate;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
