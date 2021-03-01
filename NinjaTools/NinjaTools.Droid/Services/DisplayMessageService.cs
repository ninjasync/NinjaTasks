using System;
using System.Threading.Tasks;
using Android.App;
using MvvmCross;
using MvvmCross.Platforms.Android;
using NinjaTools.GUI.MVVM.Services;
using NinjaTools.GUI.MVVM.ViewModels;

namespace NinjaTools.Droid.Services
{
    public class DisplayMessageService : IDisplayMessageService
    {
        public static int ErrorIconResourceId;

        public Task<bool> ShowDelete(MessageViewModel model)
        {
            return Show(model); // this is the same on this platform.
        }

        public Task<bool> Show(MessageViewModel model)
        {
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();

            var activity = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>().Activity;

            var bld = new AlertDialog.Builder(activity);
            bld.SetMessage(model.Message);
            if (!string.IsNullOrEmpty(model.Caption))
                bld.SetTitle(model.Caption);

            if (model.IsError && ErrorIconResourceId != 0)
                bld.SetIcon(ErrorIconResourceId);

            if (model.AllowCancel)
            {
                bld.SetNegativeButton("Cancel", (s, e) => { model.WasCancelled = true; task.TrySetResult(false); });               
            }
            if (model.YesNo)
            {
                bld.SetPositiveButton("Yes", (s, e) => { task.TrySetResult(true); });

                if(model.AllowCancel)
                    bld.SetNeutralButton("No", (s, e) => { task.TrySetResult(false); });
                else
                    bld.SetNegativeButton("No", (s, e) => { task.TrySetResult(false); });
            }
            else
            {
                bld.SetPositiveButton("Ok", (s, e) => { task.TrySetResult(true); });
            }
            
            bld.Create().Show();
            return task.Task;
        }

        public Task<bool> Show(InputViewModel vm)
        {
            throw new NotImplementedException();
        }
    }
}