using System.Windows;
using Microsoft.Win32;

namespace NinjaTasks.App.Wpf.Views
{
    public partial class ImportExportDlg
    {
        const string Filter = "All Supported Files (*.ics,*.txt)|*.ics;*.txt|All Files (*.*)|*.*";

        public ImportExportDlg()
        {
            this.InitializeComponent();
        }

        private void SelectFilename(object sender, RoutedEventArgs e)
        {
            FileDialog dlg;

            if (IsExport.IsChecked == true)
                dlg = new SaveFileDialog();
            else
                dlg = new OpenFileDialog();

            dlg.Filter = Filter;
            dlg.FileName = FileName.Text;
            
            if (dlg.ShowDialog(Window.GetWindow(this)) != true)
                return;
            FileName.Text = dlg.FileName;    
            
        }


        
    }
}
