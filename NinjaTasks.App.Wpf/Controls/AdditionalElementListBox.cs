using System.Windows;
using System.Windows.Controls;

namespace NinjaTasks.App.Wpf.Controls
{
    /// <summary>
    /// this class allows to place an 'Add'-Button below all Elements.
    /// </summary>
    public class AdditionalElementListBox : ListBox
    {
        public static readonly DependencyProperty AdditionalElementProperty = DependencyProperty.Register("AdditionalElement", typeof (FrameworkElement), typeof (AdditionalElementListBox), new PropertyMetadata(default(FrameworkElement)));
        public FrameworkElement AdditionalElement { get { return (FrameworkElement) GetValue(AdditionalElementProperty); } set { SetValue(AdditionalElementProperty, value); } }

        static AdditionalElementListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AdditionalElementListBox),
                new FrameworkPropertyMetadata(typeof(AdditionalElementListBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

        }
    }
}
