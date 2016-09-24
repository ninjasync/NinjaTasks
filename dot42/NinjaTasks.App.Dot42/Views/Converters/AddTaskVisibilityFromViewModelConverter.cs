using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using NinjaTasks.Core.ViewModels;

namespace NinjaTasks.App.Droid.Views.Converters
{
    public class AddTaskVisibilityFromViewModelConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vm = value as ITasksViewModel;
            return vm != null && vm.AllowAddItem;
        }
    }
}
