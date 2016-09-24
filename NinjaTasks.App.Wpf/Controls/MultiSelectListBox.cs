using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NinjaTasks.App.Wpf.Controls
{
    public class MultiSelectListBox : ListBox
    {
        public static readonly DependencyProperty ScrollSelectionIntoViewProperty = DependencyProperty.Register("ScrollSelectionIntoView", typeof (bool), typeof (MultiSelectListBox), new PropertyMetadata(true));
        public bool ScrollSelectionIntoView { get { return (bool) GetValue(ScrollSelectionIntoViewProperty); } set { SetValue(ScrollSelectionIntoViewProperty, value); } }

        public static readonly DependencyProperty SelectedItemsListProperty =
                                        DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(MultiSelectListBox),
                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemsListChanged));

        static MultiSelectListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectListBox), new FrameworkPropertyMetadata(typeof(MultiSelectListBox)));
   
        }


        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public MultiSelectListBox()
        {
            this.SelectionChanged += OnSelectionChanged;
        }

        public override void OnApplyTemplate()
        {
            base.SelectionMode = SelectionMode.Extended;

            base.OnApplyTemplate();
        }

        private static void OnSelectedItemsListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // make sure the list is changed...
            this.SelectedItemsList = new ArrayList(SelectedItems);

            if (ScrollSelectionIntoView)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(delegate
                {
                    ScrollIntoView(SelectedItem);
                }));
            }
        }



    }
}