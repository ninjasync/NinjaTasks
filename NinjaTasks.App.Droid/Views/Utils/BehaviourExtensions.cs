using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace NinjaTasks.App.Droid.Views.Utils
{
    static class BehaviourExtensions
    {
        /// <summary>
        /// This will open the softkeyboard when the EditText receives focus 
        /// and when no hardware keyboard is present.
        /// </summary>
        /// <param name="edit"></param>
        public static void OpenSoftKeyboardOnReceiveFocus(this EditText edit, Dialog dlg)
        {
            // http://stackoverflow.com/questions/2403632/android-show-soft-keyboard-automatically-when-focus-is-on-an-edittext
            // http://stackoverflow.com/questions/9102074/android-edittext-in-dialog-doesnt-pull-up-soft-keyboard
            // http://stackoverflow.com/questions/6654177/is-there-a-way-in-android-to-tell-if-a-users-device-has-an-actual-keyboard-or-no/6654219#6654219
            if (!edit.Context.HasHwKeyboard())
            {
                edit.FocusChange += (s, e) =>
                {
                    if (e.HasFocus)
                    {
                        dlg.Window.SetSoftInputMode(SoftInput.StateAlwaysVisible);
                        //InputMethodManager imm = (InputMethodManager)edit.Context.GetSystemService(Context.INPUT_METHOD_SERVICE);
                        //imm.ShowSoftInput(edit, InputMethodManager.SHOW_IMPLICIT);
                    }
                        
                };
            }
        }

        public static bool HasHwKeyboard(this Context ctx)
        {
            bool hasHwKeyboard = ctx.Resources.Configuration.Keyboard != Android.Content.Res.KeyboardType.Nokeys;
            return hasHwKeyboard;
        }
        
        /// <summary>
        /// Opens the soft keyboard if no hw keybaord is attached.
        /// </summary>
        public static void OpenSoftKeyboard(this View view)
        {
            if (view.Context.HasHwKeyboard())
                return;
            InputMethodManager imm = (InputMethodManager)view.Context.GetSystemService(Context.InputMethodService);
            imm.ShowSoftInput(view, ShowFlags.Implicit);

        }

        /// <summary>
        /// Opens the soft keyboard if no hw keybaord is attached.
        /// </summary>
        public static void CloseSoftKeyboard(this View view)
        {
            InputMethodManager imm = (InputMethodManager)view.Context.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.ImplicitOnly);

        }

    }
}
