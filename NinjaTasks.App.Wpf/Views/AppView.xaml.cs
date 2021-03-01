using System.Windows;
using System.Windows.Controls;

namespace NinjaTasks.App.Wpf.Views
{
    public partial class AppView
    {
        public AppView()
        {
            this.InitializeComponent();
        }
        
        private void OnContentGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var g = (Grid)sender;

            double maxW = e.NewSize.Width - g.ColumnDefinitions[2].MinWidth - g.ColumnDefinitions[1].ActualWidth;
            g.ColumnDefinitions[0].MaxWidth = maxW;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null) window.Close();
        }
    }
}
