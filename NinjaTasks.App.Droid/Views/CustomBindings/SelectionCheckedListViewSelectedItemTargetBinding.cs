// MvxListViewSelectedItemTargetBinding.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com

using System;
using System.Collections.Specialized;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Bindings.Target.Construction;
using MvvmCross.Platforms.Android.Binding.Target;
using NinjaTasks.App.Droid.Views.Controls;

namespace NinjaTasks.App.Droid.Views.CustomBindings
{
    public class SelectionCheckedListViewSelectedItemTargetBinding : MvxAndroidTargetBinding
    {
        protected SelectionCheckedListView ListView
        {
            get { return (SelectionCheckedListView)Target; }
        }

        private object _currentValue;
        private int _currentPosition = -1;
        private bool _subscribed;
        

        public SelectionCheckedListViewSelectedItemTargetBinding(AbsListView view)
            : base(view)
        {
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            var listView = ListView;
            if (listView == null)
                return;

            var adapter = listView.Adapter;

            if (adapter == null)
                return;

            var position = itemClickEventArgs.Position;

            var newValue = adapter.GetRawItem(position);
            UpdateCheckedState(position, listView);

            if (!ReferenceEquals(newValue, _currentValue))
            {
                _currentValue = newValue;
                FireValueChanged(newValue);
            }
        }

        private void OnDataSetChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_currentPosition == -1 || _currentValue == null)
                return;

            if (e != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Add
                    && _currentPosition < e.NewStartingIndex)
                    return;
                if (e.Action == NotifyCollectionChangedAction.Remove
                    && _currentPosition < e.OldStartingIndex)
                    return;
                if (e.Action == NotifyCollectionChangedAction.Move
                    && _currentPosition < e.OldStartingIndex && _currentPosition < e.NewStartingIndex)
                    return;
                if (e.Action == NotifyCollectionChangedAction.Replace
                    &&
                    (_currentPosition < e.NewStartingIndex || _currentPosition > e.NewStartingIndex + e.NewItems.Count))
                    return;
            }

            var listView = ListView;
            if (listView == null)
                return;

            var position = listView.Adapter.GetPosition(_currentValue);
            if (position == -1)
                _currentValue = null;

            UpdateCheckedState(position, listView);
        }
        
        protected override void SetValueImpl(object target, object value)
        {
            if (ReferenceEquals(value, _currentValue))
                return;

            var listView = (SelectionCheckedListView)target;
            if (listView == null)
                return;

            int index = value == null ? -1 : listView.Adapter.GetPosition(value);

            if (value != null && index < 0)
            {
                //MvxBindingTrace.Trace(MvxTraceLevel.Warning, "Value not found for ListView {0}", value.ToString());
                return;
            }

            _currentValue = value;

            if (index >= 0)
                listView.SetSelection(index);

            UpdateCheckedState(index, ListView);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        public override void SubscribeToEvents()
        {
            var listView = ListView;
            if (listView == null)
                return;

            ((ListView)listView).ItemClick += OnItemClick;
            listView.DataSetChanged += OnDataSetChanged;
            
            _subscribed = true;
        }

        public override Type TargetType
        {
            get { return typeof(object); }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var listView = ListView;
                if (listView != null && _subscribed)
                {
                    ((ListView)listView).ItemClick -= OnItemClick;
                    listView.DataSetChanged -= OnDataSetChanged;

                    _subscribed = false;
                }
                _currentPosition = -1;
                _currentValue = null;
            }
            base.Dispose(isDisposing);
        }

        private void UpdateCheckedState(int position, SelectionCheckedListView listView)
        {
            if (position != _currentPosition)
            {
                if(_currentPosition >= 0)
                    listView.SetItemChecked(_currentPosition, false);
                if (position >= 0)
                    listView.SetItemChecked(position, true);
                _currentPosition = position;
            }
        }

        //private View GetItemViewAtPosition(int position)
        //{
        //    var listView = ListView;
        //    if (listView == null)
        //        return null;
            
        //    int firstPosition = listView.FirstVisiblePosition - listView.HeaderViewsCount; // This is the same as child #0
        //    int wantedChild = position - firstPosition;
        //    if (wantedChild < 0 || wantedChild >= listView.ChildCount - listView.FooterViewsCount)
        //    {
        //        return null;
        //    }
        //    // Could also check if wantedPosition is between listView.getFirstVisiblePosition() and listView.getLastVisiblePosition() instead.
        //    return listView.GetChildAt(wantedChild);            
        //}

        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterCustomBindingFactory<SelectionCheckedListView>("SelectedItem", target => new SelectionCheckedListViewSelectedItemTargetBinding(target));
        }
    }
}