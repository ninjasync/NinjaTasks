using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NinjaTools.GUI.Wpf.Controls
{
    /// <summary>
    /// A Stack Panel that makes all its children have the same width/height and a specified padding between them.
    /// </summary>
    public class UniformStackPanel : Panel
    {
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(UniformStackPanel), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public Orientation Orientation { get { return (Orientation)GetValue(OrientationProperty); } set { SetValue(OrientationProperty, value); } }

        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(double), typeof(UniformStackPanel), new FrameworkPropertyMetadata(10d, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double Padding { get { return (double)GetValue(PaddingProperty); } set { SetValue(PaddingProperty, value); } }

        protected override Size MeasureOverride(Size constraint)
        {
            int measuredChildren = InternalChildren.Cast<UIElement>().Count(x => x.Visibility != Visibility.Collapsed);

            if (measuredChildren == 0)
                return new Size(0, 0);

            double totalPadding = Padding * (measuredChildren - 1);

            Size availableSize = Orientation == Orientation.Horizontal ?
                                     new Size(Math.Max(0, (constraint.Width - totalPadding)/measuredChildren), constraint.Height)
                                   : new Size(constraint.Width, Math.Max(0, (constraint.Height - totalPadding)/measuredChildren));

            if (availableSize.Height < 0) availableSize.Height = 0;
            if (availableSize.Width < 0) availableSize.Width = 0;
            //if (availableSize.Height <= 0 || availableSize.Width <= 0)
            //    return constraint;

            double maxWidth = 0.0;
            double maxHeight = 0.0;
            int count = this.InternalChildren.Count;

            for (int index = 0; index < count; ++index)
            {
                UIElement uiElement = this.InternalChildren[index];
                uiElement.Measure(availableSize);
                Size desiredSize = uiElement.DesiredSize;
                if (maxWidth < desiredSize.Width)
                    maxWidth = desiredSize.Width;
                if (maxHeight < desiredSize.Height)
                    maxHeight = desiredSize.Height;
            }

            return Orientation == Orientation.Horizontal ?
                  new Size(maxWidth * measuredChildren + totalPadding, maxHeight)
                : new Size(maxWidth, maxHeight * measuredChildren + totalPadding);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            int measuredChildren = InternalChildren.Cast<UIElement>().Count(x => x.Visibility != Visibility.Collapsed);
            double padding = Padding;
            double totalPadding = padding * (measuredChildren - 1);

            var orientation = Orientation;

            Rect position = Orientation == Orientation.Horizontal
                        ? new Rect(0, 0, (arrangeSize.Width - totalPadding) / measuredChildren, arrangeSize.Height)
                        : new Rect(0, 0, arrangeSize.Width, (arrangeSize.Height - totalPadding) / measuredChildren);

            double width = position.Width;
            double height = position.Height;

            foreach (UIElement uiElement in this.InternalChildren)
            {
                uiElement.Arrange(position);
                if (uiElement.Visibility == Visibility.Collapsed)
                    continue;
                if (orientation == Orientation.Horizontal)
                    position.X += width + padding;
                if (orientation == Orientation.Vertical)
                    position.Y += height + padding;
            }
            return arrangeSize;
        }
    }
}
