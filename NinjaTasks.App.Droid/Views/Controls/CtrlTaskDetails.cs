using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCross;
using MvvmCross.Platforms.Android;
using NinjaTasks.App.Droid.Views.Utils;
using NinjaTasks.Core.ViewModels;
using R = NinjaTasks.App.Droid.Resource;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public class CtrlTaskDetails : BaseControlView
    {
        private readonly EditOnClickController _edit;

        public CtrlTaskDetails(Context context, IAttributeSet attrs)
            : base(Resource.Layout.TaskDetails, context, attrs)
        {
            _edit = new EditOnClickController(Resource.Id.description_view, Resource.Id.description_edit);
        }

        protected override void OnFinishDelayInflate()
        {
            base.OnFinishDelayInflate();

            FindViewById<View>(R.Id.description_view).Click += OnDescriptionViewClick;

            var attachmentButton = FindViewById<Button>(R.Id.attachmentButton);
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

        private void OnDescriptionViewClick(object sender, EventArgs e)
        {
            _edit.StartEdit(this);
        }
    }

}
