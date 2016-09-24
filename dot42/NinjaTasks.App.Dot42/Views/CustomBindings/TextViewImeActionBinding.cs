using System;
using System.Windows.Input;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace NinjaTasks.App.Droid.Views.CustomBindings
{
    public class TextViewImeActionBinding : MvxAndroidTargetBinding
    {
        private readonly bool _subscribed;
        private ICommand _command;

        protected TextView TextView
        {
            get { return Target as TextView; }
        }

        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<TextView>("ImeAction", view => new TextViewImeActionBinding(view));
        }

        public TextViewImeActionBinding(TextView view)
            : base(view)
        {
            if (view == null)
                Mvx.Trace(MvxTraceLevel.Error, "TextViewImeActionBinding : view is null");

            if (view != null)
            {
                TextView.EditorAction += ViewOnEditorAction;
                _subscribed = true;
            }
        }

        private void ViewOnEditorAction(object sender, EditorActionEventArgs args)
        {
            FireValueChanged(args.ActionId);

            //args.ActionId
            args.IsHandled = true;
            if (_command == null)
                return;

            _command.Execute(((TextView)sender).Text);
        }

        public override Type TargetType
        {
            get { return typeof(ICommand); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        protected override void SetValueImpl(object target, object value)
        {
            _command = (ICommand)value;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var view = TextView;
                if (view != null && _subscribed)
                {
                    view.EditorAction -= ViewOnEditorAction;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}