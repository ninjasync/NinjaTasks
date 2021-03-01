using Android.Content;
using Android.Util;
using Android.Views;

namespace NinjaTasks.App.Droid.Views.Controls
{
    /// <summary>
    /// Extends the drawer Layout from the support library and provides overridable state methods.
    /// </summary>
    public class DrawerLayout : AndroidX.DrawerLayout.Widget.DrawerLayout, AndroidX.DrawerLayout.Widget.DrawerLayout.IDrawerListener
    {
        public DrawerLayout(Context context) : base(context)
        {
            base.AddDrawerListener(this);
        }

        public DrawerLayout(Context context, IAttributeSet attrs)  : base(context, attrs)
        {
            base.AddDrawerListener(this);
        }

        public DrawerLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            base.AddDrawerListener(this);
        }

        public virtual void OnDrawerSlide(View view, float single)
        {
        }

        public virtual void OnDrawerOpened(View view)
        {
        }

        public virtual void OnDrawerClosed(View view)
        {
        }

        public virtual void OnDrawerStateChanged(int state)
        {
        }
    }
}
