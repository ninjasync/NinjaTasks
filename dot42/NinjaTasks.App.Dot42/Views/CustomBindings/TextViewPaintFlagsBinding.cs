using System;
using Android.Graphics;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace NinjaTasks.App.Droid.Views.CustomBindings
{
    public class TextViewPaintFlagsBinding : MvxAndroidTargetBinding
    {
        private readonly int _paintFlags;

        public TextViewPaintFlagsBinding(TextView target, int paintFlags)
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
            registry.RegisterCustomBindingFactory<TextView>("IsStrikeThrough", target => new TextViewPaintFlagsBinding(target, Paint.STRIKE_THRU_TEXT_FLAG));
            registry.RegisterCustomBindingFactory<TextView>("IsUnderline", target => new TextViewPaintFlagsBinding(target, Paint.UNDERLINE_TEXT_FLAG));
            registry.RegisterCustomBindingFactory<TextView>("IsFakeBold", target => new TextViewPaintFlagsBinding(target, Paint.FAKE_BOLD_TEXT_FLAG));
        }
    }
}