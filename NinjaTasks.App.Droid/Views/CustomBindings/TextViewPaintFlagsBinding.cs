using System;
using Android.Graphics;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Platforms.Android.Binding.Target;

namespace NinjaTasks.App.Droid.Views.CustomBindings
{
    public class TextViewPaintFlagsBinding : MvxAndroidTargetBinding
    {
        private readonly PaintFlags _paintFlags;

        public TextViewPaintFlagsBinding(TextView target, PaintFlags paintFlags)
            : base(target)
        {
            _paintFlags = paintFlags;
        }

        protected override void SetValueImpl(object target, object value)
        {
            var view = (TextView)target;
            bool val = Convert.ToBoolean(value);

            if (val) view.PaintFlags |= _paintFlags;
            else     view.PaintFlags &=~_paintFlags;
        }

        public override Type TargetType
        {
            get { return typeof(bool); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<TextView>("IsStrikeThrough", target => new TextViewPaintFlagsBinding(target, PaintFlags.StrikeThruText));
            registry.RegisterCustomBindingFactory<TextView>("IsUnderline", target => new TextViewPaintFlagsBinding(target, PaintFlags.UnderlineText));
            registry.RegisterCustomBindingFactory<TextView>("IsFakeBold", target => new TextViewPaintFlagsBinding(target, PaintFlags.FakeBoldText));
        }
    }
}