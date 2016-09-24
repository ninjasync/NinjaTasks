using System;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace NinjaTasks.App.Droid.Views.CustomBindings
{
    public class ImageViewImageResourceTargetBinding : MvxAndroidTargetBinding
    {
        public ImageViewImageResourceTargetBinding(ImageView target)
            : base(target)
        {
        }

        protected override void SetValueImpl(object target, object value)
        {
            var binaryEdit = (ImageView)target;
            binaryEdit.SetImageResource((int)value);
        }

        public override Type TargetType
        {
            get { return typeof(int); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<ImageView>("ImageResource", target => new ImageViewImageResourceTargetBinding(target));
        }
    }
}