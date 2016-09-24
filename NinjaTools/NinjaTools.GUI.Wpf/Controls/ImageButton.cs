using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NinjaTools.GUI.Wpf.Controls
{
    [DefaultEvent("Click")]
    [DefaultProperty("Text")]
    public class ImageButton : Button
    {

        public ImageButton()
        {
            ImagedContent content = new ImagedContent();

            BindingOperations.SetBinding(content, ImagedContent.ImageProperty, new Binding(ImageProperty.Name) {Source = this});
            BindingOperations.SetBinding(content, ImagedContent.ContentProperty, new Binding(TextProperty.Name) { Source = this });
            BindingOperations.SetBinding(content, ImagedContent.ImageHeightProperty, new Binding(ImageHeightProperty.Name) { Source = this });
            BindingOperations.SetBinding(content, ImagedContent.ImageWidthProperty, new Binding(ImageWidthProperty.Name) { Source = this });
            BindingOperations.SetBinding(content, ImagedContent.GapProperty, new Binding(GapProperty.Name) { Source = this });

            Content = content;
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageButton), new PropertyMetadata(default(ImageSource)));
        public ImageSource Image { get { return (ImageSource)GetValue(ImageProperty); } set { SetValue(ImageProperty, value); } }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ImageButton), new PropertyMetadata(null));
        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }

        // Using a DependencyProperty as the backing store for ImageHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register("ImageHeight", typeof(double?), typeof(ImageButton), new PropertyMetadata(null));
        public double? ImageHeight { get { return (double?)GetValue(ImageHeightProperty); } set { SetValue(ImageHeightProperty, value); } }

        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register("ImageWidth", typeof(double?), typeof(ImageButton), new PropertyMetadata(null));
        public double? ImageWidth { get { return (double?)GetValue(ImageWidthProperty); } set { SetValue(ImageWidthProperty, value); } }

        public static readonly DependencyProperty GapProperty =
            DependencyProperty.Register("Gap", typeof (double), typeof (ImageButton), new PropertyMetadata(ImagedContent.DefaultGap));
        public double Gap { get { return (double) GetValue(GapProperty); } set { SetValue(GapProperty, value); }}
    }
}
