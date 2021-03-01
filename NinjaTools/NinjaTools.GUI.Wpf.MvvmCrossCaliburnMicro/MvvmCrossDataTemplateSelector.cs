using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MvvmCross;
using MvvmCross.Views;

namespace NinjaTools.GUI.Wpf.MvvmCrossCaliburnMicro
{
    public class MvvmCrossDataTemplateSelector : DataTemplateSelector
    {
        private readonly Dictionary<Type, DataTemplate> _templates = new Dictionary<Type, DataTemplate>();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return base.SelectTemplate(null, container);

            DataTemplate template = null;

            var viewModelType = item.GetType();
            if (_templates.TryGetValue(viewModelType, out template))
                return template ?? base.SelectTemplate(item, container);

            var viewType = GetViewType(viewModelType);

            if (viewType != null)
            {
                template = new DataTemplate();
                template.DataType = viewModelType;
                FrameworkElementFactory fact = new FrameworkElementFactory(viewType);
                template.VisualTree = fact;
            }

            _templates[viewModelType] = template;

            return template ?? base.SelectTemplate(item, container);
        }

        private class InvalidViewFinder : IMvxViewFinder
        {
            public Type GetViewType(Type viewModelType)
            {
                return GetType();
            }
        }

        private static Type GetViewType(Type viewModelType)
        {
            // Not sure why they need to use exceptions...

            // we want to prevent the KeyNotFoundException, as they are more than 
            // annoying during debugging in VS2015

            var vf = Mvx.IoCProvider.GetSingleton<IMvxViewFinder>();

            (vf as IMvxViewsContainer)?.SetLastResort(new InvalidViewFinder());
            var ret = vf.GetViewType(viewModelType);
            (vf as IMvxViewsContainer)?.SetLastResort(null);

            return ret == typeof(InvalidViewFinder) ? null : ret;
        }
    }
}
