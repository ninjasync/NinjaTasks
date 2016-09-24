using System;
using Android.Content;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using NinjaTasks.Core.ViewModels;
using NinjaTools.Logging;
using NinjaTools.Threading;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public class TaskListFilteringAdapter : SelectionCheckedListView.SelectionCheckedAdapter
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly DelayedCommandQueue _delayed;

        public bool ShowCompleted { get; set; }
        public bool AccumulateChanges { get; set; }

        public TaskListFilteringAdapter(Context context, IMvxAndroidBindingContext bindingContext) : base(context, bindingContext)
        {
            _delayed =  new DelayedCommandQueue(TimeSpan.FromMilliseconds(20), true);
        }

        public override int Count
        {
            get
            {
                if (ShowCompleted)
                    return base.Count;

                // we know that the completed items are all at the end of the list.
                // all we have to to is to adjust the count...
                var items = ItemsSource;
                if (items == null) return 0;
                int count = 0;
                foreach (TodoTaskViewModel vm in items)
                {
                    if (vm.IsCompleted)
                        break;
                    count += 1;
                }
                return count;
            }
        }

        protected override void RealNotifyDataSetChanged()
        {
            if(AccumulateChanges)
                _delayed.Add(DoNotifyReal, "notify");
            else
                DoNotifyReal();
        }

        private void DoNotifyReal()
        {
            base.RealNotifyDataSetChanged();
        }
    }
}
