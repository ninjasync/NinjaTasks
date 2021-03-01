using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using NinjaTasks.Core.ViewModels;
using NinjaTasks.Core.ViewModels.Sync;

namespace NinjaTasks.App.Wpf.Views
{
    public partial class TaskWarriorAccountDlg
    {
        public TaskWarriorAccountDlg()
        {
            this.InitializeComponent();
        }

        private void SelectServerCertificateFilename(object sender, RoutedEventArgs e)
        {
            string fileName = GetPfxFilename(ServerCertificateFileName.Text);
            if (fileName == null) return;
            ServerCertificateFileName.Text = fileName;
        }

        private void SelectClientCertificateFilename(object sender, RoutedEventArgs e)
        {
            string fileName = GetPfxFilename(ClientCertificateFileName.Text);
            if (fileName == null) return;
            ClientCertificateFileName.Text = fileName;
        }

        private string GetPfxFilename(string fileName)
        {
            var dlg = new OpenFileDialog()
            {
                FileName = fileName,
                Filter = "Certificates/Keys (*.pfx, *.crt)|*.pfx;*.crt|All Files (*.*)|*.*",
            };

            if (dlg.ShowDialog(Window.GetWindow(this)) != true)
                return null;
            return dlg.FileName;
        }


        private void SelectImportTaskdConfig(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as TaskWarriorAccountViewModel;
            if (vm == null) return;

            var dlg = new OpenFileDialog()
            {
                Filter = ".taskdconfig (*.taskdconfig)|*.taskdconfig|All Files (*.*)|*.*",
            };

            if (dlg.ShowDialog(Window.GetWindow(this)) != true)
                return;

            vm.ImportTaskdConfig(File.ReadAllText(dlg.FileName, Encoding.UTF8), false);
        }
    }
}
