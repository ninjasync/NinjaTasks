﻿using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using NinjaTasks.Core.ViewModels;

namespace NinjaTasks.App.Droid.Views.Converters
{
    public class ListIconResourceIdConverter : MvxValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskListInboxViewModel)
                return R.Drawable.filled_box_30;
            if (value is TaskListPriorityViewModel)
                return R.Drawable.star_30;
            return R.Drawable.list_30;
        }
    }
}
