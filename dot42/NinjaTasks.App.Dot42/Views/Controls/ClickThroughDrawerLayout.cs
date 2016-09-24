using System;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public class DrawerOpenButNotHitTouchEvent : EventArgs
    {
        public View Drawer { get; private set; }
        public MotionEvent Event { get; private set; }

        public bool PreventClickThrough { get; set; }

        public DrawerOpenButNotHitTouchEvent(MotionEvent @event, View drawer)
        {
            Event = @event;
            Drawer = drawer;
        }
    }

    public class ClickThroughDrawerLayout : DrawerLayout
    {
        private bool _isClickThrough2;
        private bool _isClickThrough1;

        private int _scrimColor = Color.Argb(0x80, 0, 0, 0);

        public event EventHandler<DrawerOpenButNotHitTouchEvent> DrawerOpenButNotHitInterception;

        public bool IsClickThrough
        {
            get { return _isClickThrough1 && _isClickThrough2; } 
            set { _isClickThrough1 = _isClickThrough2 = value; UpdateScrimColor();}
        }
       
        public bool IsClickThrough1
        {
            get { return _isClickThrough1; }
            set { _isClickThrough1 = value; UpdateScrimColor(); }
        }

        public bool IsClickThrough2
        {
            get { return _isClickThrough2; }
            set { _isClickThrough2 = value; UpdateScrimColor();
            }
        }

        public ClickThroughDrawerLayout(Context context) 
            : base(context)
        {
            IsClickThrough = true;
        }

        public ClickThroughDrawerLayout(Context context, IAttributeSet attrs) 
            : base(context, attrs)
        {
            IsClickThrough = true;
        }

        public ClickThroughDrawerLayout(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            IsClickThrough = true;
        }

        public override void SetScrimColor(int scrimColor)
        {
            _scrimColor = scrimColor;
            UpdateScrimColor();
        }


        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            DrawerOpenButNotHitTouchEvent args = null;

            if (IsClickThrough1 && ChildCount > 1)
            {
                View drawer = GetChildAt(1);
                args = GetDrawerOpenButNotHit(ev, drawer);
            }
            if (args == null && IsClickThrough2 && ChildCount > 2)
            {
                View drawer = GetChildAt(2);
                args = GetDrawerOpenButNotHit(ev, drawer);
            }

            if (args != null)
            {
                OnDrawerOpenButNotHit(args);
                if (!args.PreventClickThrough)
                    return false;
            }

            return base.OnInterceptTouchEvent(ev);
        }

        private DrawerOpenButNotHitTouchEvent GetDrawerOpenButNotHit(MotionEvent ev, View drawer)
        {
            if (IsDrawerOpen(drawer))
            {
                Rect rect = new Rect();
                drawer.GetHitRect(rect);
                if (!rect.Contains((int) ev.X, (int) ev.Y))
                    return new DrawerOpenButNotHitTouchEvent(ev, drawer);
            }
            return null;
        }

        public override void OnDrawerSlide(View view, float single)
        {
            if (_isClickThrough2 == _isClickThrough1)
                return;

            bool isTransparent =
                   IsClickThrough1 && ChildCount > 1 && GetChildAt(1) == view
                || IsClickThrough2 && ChildCount > 2 && GetChildAt(2) == view;
            base.SetScrimColor(isTransparent?Android.R.Color.Transparent : _scrimColor);
            
            base.OnDrawerSlide(view, single);
        }

        private void UpdateScrimColor()
        {
            if (_isClickThrough2 == _isClickThrough1)
            {
                if(_isClickThrough1)
                    base.SetScrimColor(Android.R.Color.Transparent);
                else
                    base.SetScrimColor(_scrimColor);
                return;
            }

            if (IsClickThrough1 && ChildCount > 1 && IsDrawerVisible(GetChildAt(1))
             || IsClickThrough2 && ChildCount > 2 && IsDrawerVisible(GetChildAt(2)))
                base.SetScrimColor(Android.R.Color.Transparent);
            else
                base.SetScrimColor(_scrimColor);
        }

        protected virtual void OnDrawerOpenButNotHit(DrawerOpenButNotHitTouchEvent e)
        {
            var handler = DrawerOpenButNotHitInterception;
            if (handler != null) handler(this, e);
        }
    }
}
