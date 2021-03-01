using System.Windows;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;

namespace NinjaTools.GUI.Wpf.Views
{
    public partial class InputDlg : Window, IMvxWpfView
    {
        public IMvxBindingContext BindingContext { get; set; }

        public InputDlg()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Text.SelectAll();
            Text.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public IMvxViewModel ViewModel
        {
            get { return (IMvxViewModel)DataContext; }
            set
            {
                DataContext = value;
            }
        }

    }
}
