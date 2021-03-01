using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NinjaTools.GUI.Wpf.Controls
{
    /// <summary>
    /// Displays an image besides some content.
    /// </summary>
    [DefaultProperty("Content")]
    public class ImagedContent : StackPanel
    {
        public const double DefaultGap = 4d;
        private readonly AutoGreyableImage _image = null;
        private readonly ContentPresenter _content = null;

        public ImagedContent()
        {
            Orientation = Orientation.Horizontal;
            
            _image = new AutoGreyableImage
            {
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.Uniform,
            };

            _content = new ContentPresenter
            {
                Margin = new Thickness(Gap, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                RecognizesAccessKey = true,
                Visibility = Visibility.Collapsed,
            };

            BindImageHeight();

            Children.Add(_image);
            Children.Add(_content);
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImagedContent), new PropertyMetadata(default(ImageSource), OnImageSourceChanged));
        public ImageSource Image { get { return (ImageSource)GetValue(ImageProperty); } set { SetValue(ImageProperty, value); } }
        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = ((ImagedContent)d);
            s._image.Source = e.NewValue as ImageSource;
            s._image.Visibility = Visibility.Visible;
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(ImagedContent), new PropertyMetadata(default(object), OnContentChanged));
        public object Content { get { return (string)GetValue(ContentProperty); } set { SetValue(ContentProperty, value); } }
        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = ((ImagedContent)d);
            s._content.Content = e.NewValue;
            s._content.Visibility = e.NewValue == null ? Visibility.Collapsed : Visibility.Visible;
            s.BindImageHeight();
        }

        // Using a DependencyProperty as the backing store for ImageHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(double?), typeof(ImagedContent), new PropertyMetadata(null, OnImageHeightChanged));
        public double? ImageHeight { get { return (double?)GetValue(ImageHeightProperty); } set { SetValue(ImageHeightProperty, value); } }
        private static void OnImageHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            double? val = (double?)e.NewValue;
            var btn = ((ImagedContent)d);
            if (val != null) btn._image.Height = val.Value;
            else             btn.BindImageHeight();
        }

        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register("ImageWidth", typeof(double?), typeof(ImagedContent), new PropertyMetadata(null, OnImageWidthChanged));
        public double? ImageWidth { get { return (double?)GetValue(ImageWidthProperty); } set { SetValue(ImageWidthProperty, value); } }
        private static void OnImageWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            double? val = (double?)e.NewValue;
            var btn = ((ImagedContent)d);
            if (val != null) btn._image.Width = val.Value;
            else btn.BindImageHeight();
        }

        public static readonly DependencyProperty GapProperty =
            DependencyProperty.Register("Gap", typeof (double), typeof (ImagedContent), new PropertyMetadata(DefaultGap, OnGapChanged));
        public double Gap { get { return (double) GetValue(GapProperty); } set { SetValue(GapProperty, value); }}

        private static void OnGapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = ((ImagedContent)d);
            if(sender._content != null)
                sender._content.Margin = new Thickness(sender.Gap, 0, 0, 0);
        }

        private void BindImageHeight()
        {
            if (ImageHeight == null && ImageWidth == null && _content.Visibility != Visibility.Collapsed)
            {
                var binding = new Binding("ActualHeight");
                binding.Source = _content;
                BindingOperations.SetBinding(_image, FrameworkElement.HeightProperty, binding);
            }
            else
                BindingOperations.ClearBinding(_image, FrameworkElement.HeightProperty);
        }
    }
}
