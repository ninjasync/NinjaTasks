using System;
using System.Collections.Specialized;
using Android.Content;
using Android.Util;
using Android.Views;
using MvvmCross.Base;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public class SelectionCheckedListView : MvxListView
    {
        /// <summary>
        /// the argument can be null for a non-NotifyCollectionChanged change.
        /// </summary>
        public event EventHandler<NotifyCollectionChangedEventArgs> DataSetChanged;

        public SelectionCheckedListView(Context context, IAttributeSet attrs) 
               : base(context, attrs, new SelectionCheckedAdapter(context))
        {
            Adapter.ListView = this;
        }

        public SelectionCheckedListView(Context context, IAttributeSet attrs, SelectionCheckedAdapter adapter)
               : base(context, attrs, adapter)
        {
            if(adapter != null)
                adapter.ListView = this;
        }

        public new SelectionCheckedAdapter Adapter
        {
            get { return (SelectionCheckedAdapter)base.Adapter; }
            set { base.Adapter = value; value.ListView = this; }
        }

        protected virtual void OnDataSetChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = DataSetChanged;
            if (handler != null) handler(this, e);
        }

        public class SelectionCheckedAdapter : MvxAdapter
        {
            private SelectionCheckedListView listView;
            public SelectionCheckedListView ListView { get { return listView; } set { listView = value; } }

            public SelectionCheckedAdapter(Context context) : base(context)
            {
            }

            public SelectionCheckedAdapter(Context context, IMvxAndroidBindingContext bindingContext)
                : base(context, bindingContext)
            {
            }

            protected override void BindBindableView(object source, IMvxListItemView viewToUse)
            {
                base.BindBindableView(source, viewToUse);
                if (viewToUse.Content is IMvxDataConsumer dataConsumer)
                    dataConsumer.DataContext = source;
            }
            public override void NotifyDataSetChanged(NotifyCollectionChangedEventArgs e)
            {
                base.NotifyDataSetChanged(e);
                if (listView != null)
                    listView.OnDataSetChanged(e);
            }

            public override void NotifyDataSetChanged()
            {
                base.NotifyDataSetChanged();
                if (listView != null)
                    listView.OnDataSetChanged(null);
            }
        }

    }


   

}
