using NinjaTools.GUI.Wpf.Utils;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace NinjaTools.GUI.Wpf.Behaviors
{
    /// <summary>
    /// Allows an <see cref="ItemsControl"/> to scroll horizontally by listening to the
    /// <see cref="PreviewMouseWheel"/> event of its internal <see cref="ScrollViewer"/>.
    /// </summary>
    public class HorizontalScrollBehavior : Behavior<ItemsControl>
    {
        /// <summary>
        /// A reference to the internal ScrollViewer.
        /// </summary>
        private ScrollViewer ScrollViewer { get; set; }

        /// <summary>
        /// By default, scrolling down on the wheel translates to right, and up to left.
        /// Set this to true to invert that translation.
        /// </summary>
        public bool IsInverted { get; set; }

        /// <summary>
        /// Enable horizontal scrolling only when shift is pressed.
        /// </summary>
        public bool OnlyOnShift { get; set; } = true;

        /// <summary>
        /// The ScrollViewer is not available in the visual tree until the control is loaded.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= OnLoaded;

            ScrollViewer = AssociatedObject.GetVisualDescendent<ScrollViewer>();

            if (ScrollViewer != null)
            {
                ScrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (ScrollViewer != null)
            {
                ScrollViewer.PreviewMouseWheel -= OnPreviewMouseWheel;
            }
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(OnlyOnShift && (Keyboard.Modifiers & ModifierKeys.Shift) == 0
               && ScrollViewer.ScrollableHeight > 0)
                return;
            if (ScrollViewer.ScrollableWidth <= 0)
                return;

            var newOffset = IsInverted ?
                                ScrollViewer.HorizontalOffset + e.Delta :
                                ScrollViewer.HorizontalOffset - e.Delta;

            ScrollViewer.ScrollToHorizontalOffset(newOffset);
            e.Handled = true;
        }
    }
}