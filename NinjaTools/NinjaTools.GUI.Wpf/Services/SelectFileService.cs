using System.Windows;
using Microsoft.Win32;
using NinjaTools.GUI.MVVM.Services;
using NinjaTools.GUI.MVVM.ViewModels;

namespace NinjaTools.GUI.Wpf.Services
{
    public class SelectFileService : ISelectFileService 
    {
        public bool Select(SelectFileViewModel model)
        {
            Window wnd = Application.Current.MainWindow;

            var filter = GetFilter(model);

            bool? res = null;
            if (model != null)
            {
                if (model.Mode == FileMode.Save)
                {
                    var dlg = new SaveFileDialog
                    {
                        Filter = filter,
                        FileName = model.FileName,
                        Title = model.Caption,
                        RestoreDirectory = true,
                        AddExtension = true,
                        OverwritePrompt = true
                    };
                    res = dlg.ShowDialog(wnd);
                    if (res == true)
                        model.FileName = dlg.FileName;
                }
                else
                {
                    var dlg = new OpenFileDialog
                    {
                        Filter = filter,
                        FileName = model.FileName,
                        Title = model.Caption,
                        RestoreDirectory = true,
                        Multiselect = model.Mode == FileMode.OpenMultiple
                    };
                    res = dlg.ShowDialog(wnd);
                    if (res == true)
                    {
                        model.FileName = dlg.FileName;
                        //if (model.Mode == FileMode.OpenMultiple)
                        model.FileNames = dlg.FileNames;
                    }
                }

            }

            return res == true;
        }

        private static string GetFilter(SelectFileViewModel model)
        {
            string filter = model.Filter;
            if (model.FilterAutoAddAllFiles && !filter.EndsWith("|*.*", System.StringComparison.Ordinal))
            {
                if (string.IsNullOrEmpty(filter))
                    filter = "";
                else if(!filter.EndsWith("|", System.StringComparison.Ordinal))
                    filter += "|";

                filter += "All Files (*.*)|*.*";
            }
            return filter;
        }
    }
}
