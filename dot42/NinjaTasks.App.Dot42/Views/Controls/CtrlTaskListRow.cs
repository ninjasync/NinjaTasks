using System;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Widget;
using NinjaTools.Logging;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public class CtrlTaskListRow : FrameLayout/*, ICheckable*/
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private bool _isCompleted;

        public CtrlTaskListRow(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            
            // not sure why this is not the default...
            var textView = FindViewById<TextView>(R.Id.textView);
            textView.PaintFlags |= (Paint.SUBPIXEL_TEXT_FLAG | Paint.ANTI_ALIAS_FLAG);
            textView.SetLayerType(LAYER_TYPE_SOFTWARE, null); // without this we get blocky text for unknown reasons.

        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                _isCompleted = value;

                if (_isCompleted)
                {
                    // https://plus.google.com/+RomanNurik/posts/NSgQvbfXGQN
                    // http://developer.android.com/guide/topics/graphics/hardware-accel.html
                    // http://stackoverflow.com/questions/17470665/android-setlayertype-crashing-on-lower-version
                    if (Android.OS.Build.VERSION.SDK_INT >= Android.OS.Build.VERSION_CODES.HONEYCOMB)
                    {
                        SetLayerType(LAYER_TYPE_HARDWARE, null);
                        Alpha = 0.5f;
                    }
                }
                else
                {
                    if (Android.OS.Build.VERSION.SDK_INT >= Android.OS.Build.VERSION_CODES.HONEYCOMB)
                    {
                        SetLayerType(LAYER_TYPE_NONE, null);
                        Alpha = 1f;
                    }
                }
            }
        }

        //public void Toggle()
        //{
        //    IsChecked = !IsChecked;
        //}

        //public bool IsChecked { get; set; }
    }

}
