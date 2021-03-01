using System;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Android;
using NinjaTasks.Core.ViewModels;
using NinjaTools.Logging;
using R = NinjaTasks.App.Droid.Resource;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public class CtrlTaskListRow : FrameLayout, IMvxDataConsumer
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private bool _isCompleted;

        public object DataContext { get; set; }

        public int TemplateId => R.Layout.TaskListRow;

        public CtrlTaskListRow(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            
            // not sure why this is not the default...
            var textView = FindViewById<TextView>(R.Id.textView);
            textView.PaintFlags |= (PaintFlags.SubpixelText| PaintFlags.AntiAlias);
            textView.SetLayerType(Android.Views.LayerType.Software, null); // without this we get blocky text for unknown reasons.

            var attachmentButton = FindViewById<CheckBox>(R.Id.attachmentButton);
            attachmentButton.Click += OnSaveAttachment;
        }

        private void OnSaveAttachment(object sender, EventArgs e)
        {
            var vm = DataContext as TodoTaskViewModel;
            if (vm?.AttachmentName == null || vm?.HasAttachments != true)
                return;

            var appView = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>().Activity as AppView;

            if (appView != null)
            {
                var attachment = vm.GetAttachment();
                appView.SaveFile(attachment.Item1, attachment.Item2);
            }
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
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Honeycomb)
                    {
                        SetLayerType(Android.Views.LayerType.Hardware, null);
                        Alpha = 0.5f;
                    }
                }
                else
                {
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Honeycomb)
                    {
                        SetLayerType(Android.Views.LayerType.None, null);
                        Alpha = 1f;
                    }
                }
            }
        }

        //public void Toggle()
        //{
        //    this.IsChecked = !IsChecked;
        //}

        //public bool IsChecked { get; set; }
    }

}
