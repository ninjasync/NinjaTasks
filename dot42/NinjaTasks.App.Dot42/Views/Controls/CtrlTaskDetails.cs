using System;
using Android.Content;
using Android.Util;
using Android.Views;
using NinjaTasks.App.Droid.Views.Utils;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public class CtrlTaskDetails : BaseControlView
    {
        private readonly EditOnClickController _edit;

        public CtrlTaskDetails(Context context, IAttributeSet attrs)
            : base(R.Layout.TaskDetails, context, attrs)
        {
            _edit = new EditOnClickController(R.Id.description_view, R.Id.description_edit);
        }

        protected override void OnFinishDelayInflate()
        {
            FindViewById<View>(R.Id.description_view).Click += OnDescriptionViewClick;

            base.OnFinishDelayInflate();
        }

        private void OnDescriptionViewClick(object sender, EventArgs e)
        {
            _edit.StartEdit(this);
        }
    }

}
