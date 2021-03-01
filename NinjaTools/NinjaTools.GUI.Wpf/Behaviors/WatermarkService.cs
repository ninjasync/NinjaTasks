// source: http://stackoverflow.com/questions/833943/watermark-hint-text-placeholder-textbox-in-wpf

//
// <TextBox x:Name="SearchTextBox" controls:WatermarkService.Watermark="Hallo!">
//
// OR LONG VERSION:
//
//<AdornerDecorator>
//   <TextBox x:Name="SearchTextBox">
//      <controls:WatermarkService.Watermark>
//         <TextBlock>Type here to search text</TextBlock>
//      </controls:WatermarkService.Watermark>
//   </TextBox>
//</AdornerDecorator>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using NinjaTools.GUI.Wpf.Utils;
using System.Windows.Threading;

namespace NinjaTools.GUI.Wpf.Behaviors
{
    /// <summary>
    /// Class that provides the Watermark attached property
    /// </summary>
    public static class WatermarkService
    {
        /// <summary>
        /// Watermark Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.RegisterAttached(
            "Watermark",
            typeof(object),
            typeof(WatermarkService),
            new FrameworkPropertyMetadata(null, OnWatermarkChanged));

        public static readonly DependencyProperty MarginProperty = DependencyProperty.RegisterAttached(
            "Margin", 
            typeof(Thickness), 
            typeof(WatermarkService), 
            new PropertyMetadata(default(Thickness)));

        public static Thickness GetMargin(UIElement element) { return (Thickness) element.GetValue(MarginProperty); }
        public static void SetMargin(FrameworkElement element, Thickness value) { element.SetValue(MarginProperty, value); } 

        /// <summary>
        /// Dictionary of ItemsControls
        /// </summary>
        private static readonly Dictionary<object, ItemsControl> ItemsControls = new Dictionary<object, ItemsControl>();

        /// <summary>
        /// Gets the Watermark property.  This dependency property indicates the watermark for the control.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> to get the property from</param>
        /// <returns>The value of the Watermark property</returns>
        public static object GetWatermark(DependencyObject d){ return (object)d.GetValue(WatermarkProperty); }

        /// <summary>
        /// Sets the Watermark property.  This dependency property indicates the watermark for the control.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetWatermark(DependencyObject d, object value) {d.SetValue(WatermarkProperty, value);}

        /// <summary>
        /// Handles changes to the Watermark property.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> that fired the event</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Control control = GetWatermarkedControl(d);

            if (e.OldValue != null && e.NewValue == null)
            {
                // unload if requested.
                if(control.IsLoaded)
                    UninitializeControl(control, new RoutedEventArgs());
                else 
                    control.Loaded -= OnControlLoaded;
            }
            else if (e.OldValue == null && e.NewValue != null)
            {
                // load if requested.
                if (control.IsLoaded)
                    InitializeControl(control);
                else
                    control.Loaded += OnControlLoaded;

            }
            else if(e.NewValue != null && control.IsLoaded)
            {
                // TODO: this could be done with a binding or something as well....
                RemoveWatermark(control);
                OnCheckWatermarkStatus(control, null);
            }
        }

        private static void UninitializeControl(Control c, RoutedEventArgs e)
        {
            c.Loaded -= OnControlLoaded;
            c.GotKeyboardFocus -= OnCheckWatermarkStatus;
            c.LostKeyboardFocus -= OnLostKeyboardFocusStatus;

            if (c is TextBox)
                ((TextBox)c).TextChanged -= OnCheckWatermarkStatus;
            else if (c is ComboBox)
                ((ComboBox)c).SelectionChanged += OnCheckWatermarkStatus;
            else if (c is ItemsControl)
            {
                // not supported atm..
            }

            RemoveWatermark(c);
        }

        private static void OnControlLoaded(object d, RoutedEventArgs e)
        {
            InitializeControl(d, true);
        }

        private static void InitializeControl(object d, bool allowRetryAfterChildsAreLoaded = false)
        {
            Control c = GetWatermarkedControl(d);

            if (c is ComboBox || c is TextBox)
            {
                c.GotKeyboardFocus += OnCheckWatermarkStatus;
                c.LostKeyboardFocus += OnLostKeyboardFocusStatus;

                if (c is TextBox)
                    ((TextBox)c).TextChanged += OnCheckWatermarkStatus;
                else
                    ((ComboBox)c).SelectionChanged += OnCheckWatermarkStatus;

            }
            else if (c is ItemsControl)
            {
                ItemsControl i = (ItemsControl)c;

                // for Items property  
                i.ItemContainerGenerator.ItemsChanged += ItemsChanged;
                ItemsControls.Add(i.ItemContainerGenerator, i);

                // for ItemsSource property  
                DependencyPropertyDescriptor prop =
                    DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, i.GetType());
                prop.AddValueChanged(i, ItemsSourceChanged);
            }
            else if(allowRetryAfterChildsAreLoaded)
            {
                // try again after all children have completed loading
                // http://stackoverflow.com/questions/567216/is-there-a-all-children-loaded-event-in-wpf
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded,
                                     new Action(() => { InitializeControl(d, false); }));
                return;
            }

            OnCheckWatermarkStatus(c, new EventArgs());
        }


        private static Control GetWatermarkedControl(object o)
        {
            if (o is TextBox || o is ItemsControl)
                return (Control)o;

            var textBox = ((FrameworkElement)o).GetVisualDescendent<TextBox>();
            if (textBox != null) return textBox;

            var cb = ((FrameworkElement)o).GetVisualDescendent<ItemsControl>();
            if (cb != null) return cb;


            // fallback.
            return (Control) o;
        }

        private static void OnCheckWatermarkStatus(object sender, EventArgs e)
        {
            var control = GetWatermarkedControl(sender); 
            if (!control.IsFocused && ShouldShowWatermark(control))
                ShowWatermark(control);
            else
                RemoveWatermark(control);
        }

        private static void OnLostKeyboardFocusStatus(object sender, EventArgs e)
        {
            var control = GetWatermarkedControl(sender);
            if (ShouldShowWatermark(control))
                ShowWatermark(control);
        }

        /// <summary>
        /// Event handler for the items source changed event
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        private static void ItemsSourceChanged(object sender, EventArgs e)
        {
            ItemsControl c = (ItemsControl)sender;
            if (c.ItemsSource != null)
            {
                if (ShouldShowWatermark(c))
                {
                    ShowWatermark(c);
                }
                else
                {
                    RemoveWatermark(c);
                }
            }
            else
            {
                ShowWatermark(c);
            }
        }

        /// <summary>
        /// Event handler for the items changed event
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="ItemsChangedEventArgs"/> that contains the event data.</param>
        private static void ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            ItemsControl control;
            if (ItemsControls.TryGetValue(sender, out control))
            {
                if (ShouldShowWatermark(control))
                {
                    ShowWatermark(control);
                }
                else
                {
                    RemoveWatermark(control);
                }
            }
        }

        /// <summary>
        /// Remove the watermark from the specified element
        /// </summary>
        /// <param name="control">Element to remove the watermark from</param>
        private static void RemoveWatermark(UIElement control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer != null)
            {
                Adorner[] adorners = layer.GetAdorners(control);
                if (adorners == null)
                {
                    return;
                }

                foreach (Adorner adorner in adorners)
                {
                    if (adorner is WatermarkAdorner)
                    {
                        adorner.Visibility = Visibility.Hidden;
                        layer.Remove(adorner);
                    }
                }
            }
        }

        /// <summary>
        /// Show the watermark on the specified control
        /// </summary>
        /// <param name="control">Control to show the watermark on</param>
        private static void ShowWatermark(Control control)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(control);

            // layer could be null if control is no longer in the visual tree
            if (layer == null) return;

            Adorner[] adorners = layer.GetAdorners(control);

            if (adorners != null)
                foreach (Adorner adorner in adorners)
                {
                    if (adorner is WatermarkAdorner)
                    {
                        adorner.Visibility = Visibility.Visible;
                        return;
                    }
                }
            // add new...
            layer.Add(new WatermarkAdorner(control, GetWatermark(control)));
        }

        /// <summary>
        /// Indicates whether or not the watermark should be shown on the specified control
        /// </summary>
        /// <param name="c"><see cref="Control"/> to test</param>
        /// <returns>true if the watermark should be shown; false otherwise</returns>
        private static bool ShouldShowWatermark(Control c)
        {
            if (c is ComboBox)
            {
                return ((ComboBox) c).Text == string.Empty;
            }
            else if (c is TextBox)
            {
                return ((TextBox) c).Text == string.Empty;
            }
            else if (c is ItemsControl)
            {
                return ((ItemsControl) c).Items.Count == 0;
            }
            return false;
        }
    }

    /// <summary>
    /// Adorner for the watermark
    /// </summary>
    internal class WatermarkAdorner : Adorner
    {
        #region Private Fields

        /// <summary>
        /// <see cref="ContentPresenter"/> that holds the watermark
        /// </summary>
        private readonly ContentPresenter _contentPresenter;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WatermarkAdorner"/> class
        /// </summary>
        /// <param name="adornedElement"><see cref="UIElement"/> to be adorned</param>
        /// <param name="watermark">The watermark</param>
        public WatermarkAdorner(UIElement adornedElement, object watermark) :
            base(adornedElement)
        {
            this.IsHitTestVisible = false;

            this._contentPresenter = new ContentPresenter();
            
            this._contentPresenter.Opacity = 0.7;

            // this margin-setting is propably not yet perfect...
            var margin = WatermarkService.GetMargin(adornedElement);
            
            if (margin == new Thickness())
            {
                bool isTextBoxAndText = Control is TextBox && !(watermark is FrameworkElement);
                this._contentPresenter.Margin = new Thickness(Control.Padding.Left + (isTextBoxAndText ? 2 : 0),
                                                    Control.Padding.Top, 0, 0);
                //this._contentPresenter.Margin = new Thickness(
                //        Control.Margin.Left + Control.Padding.Left + (isTextBoxAndText ? 2 : 0),
                //        Control.Margin.Top + Control.Padding.Top, 0, 0);
            }
            else
            {
                _contentPresenter.Margin = margin;
            }

            if (this.Control is ItemsControl && !(this.Control is ComboBox))
            {
                this._contentPresenter.VerticalAlignment = VerticalAlignment.Center;
                this._contentPresenter.HorizontalAlignment = HorizontalAlignment.Center;
            }

            _contentPresenter.Content = watermark;
        
            //if (this.Control is TextBoxBase)
            //{
            //    this.contentPresenter.VerticalAlignment = VerticalAlignment.Center;
            //}

            // Hide the control adorner when the adorned element is hidden
            Binding binding = new Binding("IsVisible");
            binding.Source = adornedElement;
            binding.Converter = new BooleanToVisibilityConverter();
            this.SetBinding(VisibilityProperty, binding);
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the number of children for the <see cref="ContainerVisual"/>.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets the control that is being adorned
        /// </summary>
        private Control Control
        {
            get { return (Control)this.AdornedElement; }
        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Returns a specified child <see cref="Visual"/> for the parent <see cref="ContainerVisual"/>.
        /// </summary>
        /// <param name="index">A 32-bit signed integer that represents the index value of the child <see cref="Visual"/>. The value of index must be between 0 and <see cref="VisualChildrenCount"/> - 1.</param>
        /// <returns>The child <see cref="Visual"/>.</returns>
        protected override Visual GetVisualChild(int index)
        {
            return this._contentPresenter;
        }

        /// <summary>
        /// Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        /// <returns>A <see cref="Size"/> object representing the amount of layout space needed by the adorner.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            // Here's the secret to getting the adorner to cover the whole control
            this._contentPresenter.Measure(Control.RenderSize);
            return Control.RenderSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="FrameworkElement"/> derived class. 
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            this._contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        #endregion
    }
}