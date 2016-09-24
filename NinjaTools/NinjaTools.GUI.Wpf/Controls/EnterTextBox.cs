using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NinjaTools.GUI.Wpf.Controls
{
    [DefaultEvent("EnterPressed")]
    public class EnterTextBox : TextBox
    {
        public event EventHandler<EventArgs> EnterPressed = delegate { };

        public static readonly DependencyProperty ClearOnEscapeProperty = DependencyProperty.Register("ClearOnEscape", typeof(bool), typeof(EnterTextBox), new PropertyMetadata(true));
        public bool ClearOnEscape { get { return (bool) GetValue(ClearOnEscapeProperty); } set { SetValue(ClearOnEscapeProperty, value); } }

        public EnterTextBox()
        {
            PreviewKeyUp += DefaultTextBoxControl_PreviewKeyUp;
        }

        void DefaultTextBoxControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // update source!
                var expression = GetBindingExpression(TextProperty);
                if (expression != null) expression.UpdateSource();

                EnterPressed(this, EventArgs.Empty);
                return;
            }
            if (e.Key == Key.Escape && ClearOnEscape)
            {
                Clear();
                return;
            }
        }
    }

}