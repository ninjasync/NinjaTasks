using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;
using NinjaTasks.App.Droid.Views.Utils;
using NinjaTasks.Core.ViewModels;

namespace NinjaTasks.App.Droid.Views
{
    public class EditListDialog  : MvxDialogFragment<EditListViewModel>
    {
        public override Dialog OnCreateDialog(Bundle savedState)
        {
            RetainInstance = true;

            base.EnsureBindingContextSet(savedState);

            var view = this.BindingInflate(R.Layout.EditList, null);

            var dialog = new AlertDialog.Builder(Activity);
            dialog.SetTitle(ViewModel.IsNewList ? "Add list" : "Edit List");
            dialog.SetView(view);
            dialog.SetNegativeButton("Cancel", (s, a) => { });
            dialog.SetPositiveButton("Save", (s, a) => ViewModel.Save());

            var dlg = dialog.Create();

            var edit = view.FindViewById<EditText>(R.Id.editText);
            edit.OpenSoftKeyboardOnReceiveFocus(dlg);

            view.FindViewById<View>(R.Id.delete).Click += (sender, e) => { dlg.Dismiss(); };
            return dlg;
        }
        
        public override void OnDestroyView()
        {
            // http://stackoverflow.com/questions/16723078/mvvmcross-does-showviewmodel-always-construct-new-instances/16723459#16723459
            // You may have to add this code to stop your dialog from being dismissed on rotation, due to a bug with the compatibility library:
 
            if (Dialog != null && RetainInstance)
                Dialog.SetDismissMessage(null);
            base.OnDestroyView();
        }
       
    }
}
