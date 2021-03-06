﻿using System;
using Android.Content;
using Android.Util;
using Android.Widget;
using MvvmCross.Platforms.Android.Binding.Views;
using NinjaTasks.App.Droid.Views.Utils;
using NinjaTasks.Core.ViewModels;
using NinjaTools;
using NinjaTools.Logging;
using R = NinjaTasks.App.Droid.Resource;

namespace NinjaTasks.App.Droid.Views.Controls
{
    public class CtrlTaskListsList : BaseControlView<TodoListsViewModel>
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public ListView List { get; set; }
        public event EventHandler<AdapterView.ItemClickEventArgs> ListItemClick;
        private readonly Guard _clickGuard = new Guard(); 

        private EditOnClickController _edit;

        public CtrlTaskListsList(Context context, IAttributeSet attrs)
            : base(R.Layout.TaskListsList, context, attrs)
        {
            _edit = new EditOnClickController(R.Id.tllrTextView, R.Id.tllrEditText);
        }

        protected override void OnFinishDelayInflate()
        {
            //this.DumpHierachy();

            List = FindViewById<MvxListView>(R.Id.theTaskListsList);
            List.ItemClick += OnItemClick;
            List.ItemLongClick += OnItemLongClick;

            //var ctrlAdd = new CtrlTaskListsListAdd(Context, null);
            //List.AddFooterView(ctrlAdd);

            base.OnFinishDelayInflate();
        }

        private void OnItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            //List.SetSelection(e.Position);
            //List.SetItemChecked(e.Position, true);

            // manually select the list, as this is not done by the MvxListView / Binder.
            if (e.Position < 2) return; 
            var list = ViewModel.TodoLists[e.Position-1]; // hack..
            
            ViewModel.SelectedList = list;
            if(list == null)
                return;

            if (!list.AllowRename || !(list is TaskListViewModel))
                return;
            
            //if(ViewModel != null && ViewModel.SelectedList != null && ViewModel.SelectedList.AllowRename)
            //    _edit.StartEdit(e.View);
            var vm = new EditListViewModel(ViewModel, (TaskListViewModel) list);
            var dlg = new EditListDialog {ViewModel = vm};
            dlg.Show(FragmentActivity.ActiveFragmentManager, dlg.GetType().Name);
        }


        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if(!_clickGuard.InUse)
                FireListItemClicked(e);
        }

        private void FireListItemClicked(AdapterView.ItemClickEventArgs e)
        {
            var handler = ListItemClick;
            if (handler != null) handler(this, e);
        }
    }
}