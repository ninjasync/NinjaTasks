using System;
using System.Globalization;
using NinjaTasks.Core.ViewModels;
using MvvmCross.Converters;

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
