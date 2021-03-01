using Android.Content;
using Android.Util;
using Android.Widget;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public class CtrlTaskListsListAdd : FrameLayout
    {
        public CtrlTaskListsListAdd(Context context, IAttributeSet attrs) 
             : base(context, attrs)
        {
            Inflate(context, Resource.Layout.TaskListsListAdd, this);
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
        }
    }
}
