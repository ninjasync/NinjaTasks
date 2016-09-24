using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Views;

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

        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        private static Type GetViewType(Type viewModelType)
        {
            try
            {
                // Not sure why they need to use exceptions...
                return Mvx.GetSingleton<IMvxViewFinder>().GetViewType(viewModelType);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
    }
}
