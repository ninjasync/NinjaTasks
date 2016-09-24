using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using NinjaTasks.App.Droid.Views.Utils;
using NinjaTasks.Core.ViewModels;
using NinjaTasks.Model;
using NinjaTools.Npc;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public class CtrlTaskList : BaseControlView<ITasksViewModel>
    {
        public SelectionCheckedListView List { get; private set; }
        private View NewTask { get; set; } 

        public event EventHandler<AdapterView.ItemClickEventArgs> ListItemClick;
        public event EventHandler NewTaskGotFocus;

        private IDisposable _cfgToken;
        private NinjaTasksConfiguration _cfg;
        private TaskListFilteringAdapter _adapter;

        public CtrlTaskList(Context context, IAttributeSet attrs)
            : base(R.Layout.TaskList, context, attrs)
        {
        }

        protected override void OnFinishDelayInflate()
        {
            List = FindViewById<SelectionCheckedListView>(R.Id.theTaskList);
            NewTask = FindViewById<View>(R.Id.editTextHost);
            
            //this.DumpHierachy();
            ((ListView)List).ItemClick += OnItemClick;
            NewTask.FocusChange += OnNewTaskFocusChange;

            _adapter = new TaskListFilteringAdapter(Context, (IMvxAndroidBindingContext)BindingContext);

            _cfg = Mvx.Resolve<INinjaTasksConfigurationService>().Cfg;
            _cfgToken = _cfg.SubscribeWeak(null, OnConfigChanged);
            _adapter.ShowCompleted = _cfg.ShowCompletedTasks;

            List.Adapter = _adapter;
            _adapter.AccumulateChanges = true;
         
            base.OnFinishDelayInflate();
        }

        private void OnConfigChanged()
        {
            if (_cfg.ShowCompletedTasks != _adapter.ShowCompleted)
            {
                _adapter.ShowCompleted = _cfg.ShowCompletedTasks;
                _adapter.NotifyDataSetChanged();
            }
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            FireListItemClicked(e);
        }

        private void FireListItemClicked(AdapterView.ItemClickEventArgs e)
        {
            var handler = ListItemClick;
            if (handler != null) handler(this, e);
        }

        private void OnNewTaskFocusChange(object sender, FocusChangeEventArgs e)
        {
            if (NewTask.HasFocus)
                OnNewTaskGotFocus();
        }

        protected virtual void OnNewTaskGotFocus()
        {
            var handler = NewTaskGotFocus;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }

}
