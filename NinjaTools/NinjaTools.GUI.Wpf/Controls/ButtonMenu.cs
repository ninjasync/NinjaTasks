using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace NinjaTools.GUI.Wpf.Controls
{
    /// <summary>
    /// a button, that open its associated context menu on click.
    /// </summary>
    [ContentProperty("ContextMenu")]
    public class ButtonMenu : Button
    {
        static ButtonMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonMenu),
                new FrameworkPropertyMetadata(typeof(ButtonMenu)));
        }

        public override void OnApplyTemplate()
        {
            if (ContextMenu != null)
                ContextMenu.Placement = PlacementMode.Bottom;

            base.OnApplyTemplate();
        }

        protected override void OnClick()
        {
            base.OnClick();
            ContextMenu.PlacementTarget = this;
            ContextMenu.IsOpen = true;
        }
    }
}
