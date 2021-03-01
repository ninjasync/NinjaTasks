using System;
using Android.App;
using AndroidX.AppCompat.App;
using AndroidX.DrawerLayout.Widget;
using Android.Views;

namespace NinjaTasks.App.Droid.Views.Utils
{
    internal class ActionBarStateAwareDrawerToggle : ActionBarDrawerToggle
    {
        public bool IsDrawerClosed { get; private set; }
        public bool IsDrawerOpen { get; private set; }

        public View VisibleDrawer { get; private set; }

        public event EventHandler DrawerStateChanged;
        //public event EventHandler DrawerSlide;

        public ActionBarStateAwareDrawerToggle(Activity activity, DrawerLayout drawerLayout,
                                              int openDrawerContentDescRes, int closeDrawerContentDescRes)
            : base(activity, drawerLayout, openDrawerContentDescRes, closeDrawerContentDescRes)
        {
            IsDrawerClosed = true;
        }

        public override void OnDrawerClosed(View view)
        {
            IsDrawerClosed = true;
            VisibleDrawer = null;
            base.OnDrawerClosed(view);
            FireDrawerStateChanged();
        }

        public override void OnDrawerOpened(View view)
        {
            IsDrawerOpen = true;
            VisibleDrawer = view;

            base.OnDrawerClosed(view);
            FireDrawerStateChanged();
        }

        public override void OnDrawerStateChanged(int newState)
        {
            // If it's not idle, it isn't closed or open
            if (newState != DrawerLayout.StateIdle)
            {
                IsDrawerClosed = false;
                IsDrawerOpen = false;
            }

            base.OnDrawerStateChanged(newState);
            FireDrawerStateChanged();
        }

        private void FireDrawerStateChanged()
        {
            var handler = DrawerStateChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        //public override void OnDrawerSlide(View view, float single)
        //{
        //    OpenDrawer = view;
        //    base.OnDrawerSlide(view, single);
        //    FireDrawerSlide();
        //}

        //private void FireDrawerSlide()
        //{
        //    var handler = DrawerSlide;
        //    if (handler != null) handler(this, EventArgs.Empty);
        //}
    }
}