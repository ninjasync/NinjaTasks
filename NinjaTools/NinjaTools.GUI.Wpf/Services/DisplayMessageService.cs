﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NinjaTools.GUI.MVVM.Services;
using NinjaTools.GUI.MVVM.ViewModels;
using NinjaTools.GUI.Wpf.Views;

namespace NinjaTools.GUI.Wpf.Services
{
    public class DisplayMessageService : IDisplayMessageService
    {
        public Task<bool> ShowDelete(MessageViewModel model)
        {
            return Show(model); // this is the same on this platform.
        }

        public Task<bool> Show(MessageViewModel model)
        {
            MessageBoxButton btn =
                    (model.YesNo && model.AllowCancel) ? MessageBoxButton.YesNoCancel
                    : model.YesNo ? MessageBoxButton.YesNo
                    : model.AllowCancel ? MessageBoxButton.OKCancel
                    : MessageBoxButton.OK;

            var icon = model.IsError ? MessageBoxImage.Error : MessageBoxImage.None;

            var activeWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            if (activeWindow == null) activeWindow = Application.Current.MainWindow;

            var res = MessageBox.Show(activeWindow, model.Message, model.Caption, btn, icon);
            
            model.WasCancelled = (res == MessageBoxResult.Cancel) || (res == MessageBoxResult.None);
            
            bool ret = res == MessageBoxResult.OK || res == MessageBoxResult.Yes;
            return Task.FromResult(ret);
        }

        public Task<bool> Show(InputViewModel vm)
        {
            var dlg = new InputDlg { DataContext = vm, Owner = Application.Current.MainWindow };
            bool ret = dlg.ShowDialog() == true;
            return Task.FromResult(ret);
        }
    }
}
