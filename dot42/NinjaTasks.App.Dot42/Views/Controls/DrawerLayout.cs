using Android.Content;
using Android.Util;
using Android.Views;

namespace NinjaTasks.App.Droid.Views.Controls
{
    /// <summary>
    /// Extends the drawer Layout from the support library and provides overridable state methods.
    /// </summary>
    public class DrawerLayout : Android.Support.V4.Widget.DrawerLayout, Android.Support.V4.Widget.DrawerLayout.IDrawerListener
    {
        private IDrawerListener _drawerListener;

        public DrawerLayout(Context context) : base(context)
        {
            base.SetDrawerListener(this);
        }

        public DrawerLayout(Context context, IAttributeSet attrs)  : base(context, attrs)
        {
            base.SetDrawerListener(this);
        }

        public DrawerLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            base.SetDrawerListener(this);
        }

        public override void SetDrawerListener(IDrawerListener drawerListener)
        {
            _drawerListener = drawerListener;
        }

        public virtual void OnDrawerSlide(View view, float single)
        {
            if(_drawerListener != null)
                _drawerListener.OnDrawerSlide(view, single);
        }

        public virtual void OnDrawerOpened(View view)
        {
            if (_drawerListener != null)
                _drawerListener.OnDrawerOpened(view);
        }

        public virtual void OnDrawerClosed(View view)
        {
            if (_drawerListener != null)
                _drawerListener.OnDrawerClosed(view);
        }

        public virtual void OnDrawerStateChanged(int state)
        {
            if (_drawerListener != null)
                _drawerListener.OnDrawerStateChanged(state);

        }
    }
}
