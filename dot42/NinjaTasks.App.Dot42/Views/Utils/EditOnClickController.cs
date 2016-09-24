using System;
using Android.Views;
using Android.Widget;

namespace NinjaTasks.App.Droid.Views.Utils
{
    public class EditOnClickController
    {
        private readonly int _viewResId;
        private readonly int _editResId;
        private View _activeEdit;

        public EditOnClickController(int viewResId, int editResId)
        {
            _viewResId = viewResId;
            _editResId = editResId;
        }

        public void StartEdit(View containingView)
        {
            var edit = containingView.FindViewById<EditText>(_editResId);

            bool isSameCtrl = _activeEdit != null && ReferenceEquals(_activeEdit, containingView);
            if (isSameCtrl && edit.Visibility == View.VISIBLE)
                return;

            if (!isSameCtrl && _activeEdit != null)
            {
                StopEdit();
            }

            if (edit != null && edit.Visibility != View.VISIBLE)
            {
                StartEditing(containingView);
                _activeEdit = containingView;
            }
        }

        public void StopEdit()
        {
            if (_activeEdit == null)
                return;

            StopEditing(_activeEdit);
            _activeEdit = null;
        }

        private void StartEditing(View containingView)
        {
            var edit = containingView.FindViewById<EditText>(_editResId);
            var view = containingView.FindViewById<TextView>(_viewResId);
            view.Visibility = View.INVISIBLE;
            edit.Visibility = View.VISIBLE;
            edit.FocusChange += FinishEdit;
            edit.EditorAction += FinishEdit;
            
            edit.RequestFocus();
            int textlen = edit.Text.Length();
            edit.SetSelection(textlen,textlen);
            edit.OpenSoftKeyboard();

        }

        private void StopEditing(View containingView)
        {
            var edit = containingView.FindViewById<EditText>(_editResId);
            var view = containingView.FindViewById<TextView>(_viewResId);
            view.Visibility = View.VISIBLE;
            edit.Visibility = View.INVISIBLE;
            edit.FocusChange -= FinishEdit;
            edit.EditorAction -= FinishEdit;

            edit.CloseSoftKeyboard();
        }

        private void FinishEdit(object sender, EventArgs e)
        {
            StopEdit();
        }
    }
}
