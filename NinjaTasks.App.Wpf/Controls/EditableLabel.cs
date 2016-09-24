using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace NinjaTasks.App.Wpf.Controls
{
    [TemplatePart(Name="PART_Container", Type=typeof(Panel))]
    [TemplatePart(Name = "PART_Edit", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_Display", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_FocusElement", Type = typeof(Control))]
    public class EditableLabel : Control
    {
        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty IsEditingProperty;
        public static readonly DependencyProperty TextTrimmingProperty;
        public static readonly DependencyProperty TextWrappingProperty;
        public static readonly DependencyProperty MaxLengthProperty;
        public static readonly DependencyProperty IsReadOnlyProperty;
        public static readonly DependencyProperty StartEditOnFocusGainProperty;

        private Panel _container;
        private bool _contextMenuOpening;
        private TextBlock _displayBlock;
        private TextBox _editBox;
        private Control _hiddenFocus;
        private bool _stoppingEditing;

        static EditableLabel()
        {
            TextProperty = DependencyProperty.Register("Text", typeof (string), typeof (EditableLabel),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    null, null, true, UpdateSourceTrigger.PropertyChanged));
            IsEditingProperty = DependencyProperty.Register("IsEditing", typeof (bool), typeof (EditableLabel),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                              IsEditingPropertyChanged));
            TextTrimmingProperty = DependencyProperty.RegisterAttached("TextTrimming", typeof (TextTrimming),
                typeof (EditableLabel), new UIPropertyMetadata(TextTrimming.None));
            TextWrappingProperty = TextBlock.TextWrappingProperty.AddOwner(typeof (EditableLabel),
                new FrameworkPropertyMetadata(TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure));
            MaxLengthProperty = DependencyProperty.Register("MaxLength", typeof (int), typeof (EditableLabel),
                new FrameworkPropertyMetadata(0));
            IsReadOnlyProperty = TextBoxBase.IsReadOnlyProperty.AddOwner(typeof (EditableLabel),
                                                                    new FrameworkPropertyMetadata(false, OnIsReadonlyChanged));

            StartEditOnFocusGainProperty = DependencyProperty.Register("StartEditOnFocusGain", typeof(bool), typeof(EditableLabel), new PropertyMetadata(true));

            DefaultStyleKeyProperty.OverrideMetadata(typeof (EditableLabel),
                new FrameworkPropertyMetadata(typeof (EditableLabel)));
        }


        public bool StartEditOnFocusGain { get { return (bool) GetValue(StartEditOnFocusGainProperty); } set { SetValue(StartEditOnFocusGainProperty, value); } }

        public bool IsEditing
        {
            get { return (bool) base.GetValue(IsEditingProperty); }
            set { base.SetValue(IsEditingProperty, value); }
        }

        public string Text
        {
            get { return (string) base.GetValue(TextProperty); }
            set { base.SetValue(TextProperty, value); }
        }

        public TextTrimming TextTrimming
        {
            get { return (TextTrimming) base.GetValue(TextTrimmingProperty); }
            set { base.SetValue(TextTrimmingProperty, value); }
        }

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping) base.GetValue(TextWrappingProperty); }
            set { base.SetValue(TextWrappingProperty, value); }
        }

        [DefaultValue(0)]
        public int MaxLength
        {
            get { return (int) base.GetValue(MaxLengthProperty); }
            set { base.SetValue(MaxLengthProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool) base.GetValue(IsReadOnlyProperty); }
            set { base.SetValue(IsReadOnlyProperty, value); }
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _container = this.GetTemplateChild("PART_Container") as Panel;
            //_container.SetBinding(Control.BackgroundProperty, new Binding {Source = this, Path = new PropertyPath(BackgroundProperty)});
            _container.SetBinding(VerticalContentAlignmentProperty, new Binding {Source = this, Path = new PropertyPath(VerticalContentAlignmentProperty)});
            
            _hiddenFocus = this.GetTemplateChild("PART_FocusElement") as Control;

            _editBox = this.GetTemplateChild("PART_Edit") as TextBox;
            _editBox.SetBinding(TextBox.MaxLengthProperty, new Binding {Source = this, Path = new PropertyPath(MaxLengthProperty)});
            _editBox.SetBinding(TextBoxBase.IsReadOnlyProperty, new Binding { Source = this, Path = new PropertyPath(IsReadOnlyProperty), Mode = BindingMode.OneWay });
            _editBox.SetBinding(TextBox.TextWrappingProperty, new Binding {Source = this, Path = new PropertyPath(TextWrappingProperty)});
            
            _displayBlock = this.GetTemplateChild("PART_Display") as TextBlock;
            _displayBlock.SetBinding(TextBlock.TextProperty, new Binding {Source = this, Path = new PropertyPath(TextProperty)});
            _displayBlock.SetBinding(TextBlock.TextTrimmingProperty, new Binding {Source = this, Path = new PropertyPath(TextTrimmingProperty)});
            _displayBlock.SetBinding(TextBlock.TextWrappingProperty, new Binding {Source = this, Path = new PropertyPath(TextWrappingProperty)});
            SetupFocusHandling();

            if (IsEditing && !IsReadOnly)
            {
                OnEnterEditMode();
            }
        }

        private void SetupFocusHandling()
        {
            _container.Focusable = true;
            _container.FocusVisualStyle = null;
            _container.GotFocus += OnGotFocus;

            _displayBlock.MouseLeftButtonUp += OnMouseLeftButtonUp;

            _editBox.ContextMenuOpening += delegate { _contextMenuOpening = true; };
            _editBox.LostKeyboardFocus += OnLostKeyboardFocus;
            _editBox.LostFocus += OnLostFocus;
            
            Window window = Window.GetWindow(this);
            if (window != null)
                window.MouseLeftButtonDown += OnWindowMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _container.Focus();
            e.Handled = true;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (_stoppingEditing)
                return;

            if (!Equals(e.OriginalSource, _container))
                return;

            if (!StartEditOnFocusGain) return;

            IsEditing = true;
        }

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_contextMenuOpening)
            {
                _contextMenuOpening = false;
                return;
            }
            if (!string.IsNullOrWhiteSpace(_editBox.Text))
            {
                IsEditing = false;
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            IsEditing = false;
        }

        private void OnWindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEditing)
            {
                IsEditing = false;
                _hiddenFocus.Focus();
            }

        }
        private static void OnIsReadonlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editableLabel = d as EditableLabel;
            if (editableLabel == null) return;
            editableLabel.UpdateEditingStatus();
        }
        private static void IsEditingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editableLabel = d as EditableLabel;
            if (editableLabel == null) return;
            editableLabel.UpdateEditingStatus();
        }

        private void UpdateEditingStatus()
        {
            if (_editBox == null) return; // not yet fully constructed?

            bool shouldEdit = IsEditing && !IsReadOnly;
            
            bool isEdit = _editBox.Visibility == Visibility.Visible;

            if (shouldEdit == isEdit) return;

            if(shouldEdit)
                OnEnterEditMode();
            else
                OnLeaveEditMode();
        }


        private void OnEnterEditMode()
        {
            if (IsReadOnly) return;

            _editBox.Visibility = Visibility.Visible;
            _editBox.BeginChange();
            _editBox.Text = Text;
            _editBox.SelectAll();
            _editBox.ScrollToHome();
            _editBox.EndChange();
            _editBox.Focus();

            _displayBlock.Visibility = Visibility.Collapsed;
        }

        private void OnLeaveEditMode()
        {
            _stoppingEditing = true;

            if (!string.IsNullOrWhiteSpace(_editBox.Text))
                Text = _editBox.Text;
            
            _displayBlock.Visibility = Visibility.Visible;
            _editBox.Visibility = Visibility.Collapsed;

            _stoppingEditing = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Key key = e.Key;

            if (IsEditing && key == Key.Escape)
            {
                _editBox.Text = Text;
                _hiddenFocus.Focus();
                e.Handled = true;
            }

            if (key == Key.Return)
            {
                _hiddenFocus.Focus();
                e.Handled = true;
            }
        }
    }
}