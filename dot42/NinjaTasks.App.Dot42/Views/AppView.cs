using System;
using System.Reflection;
using System.Text;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Views.Inputmethod;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Plugins.Messenger;
using Dot42;
using Dot42.Manifest;
using NinjaTasks.App.Droid.Views.Controls;
using NinjaTasks.App.Droid.Views.Utils;
using NinjaTasks.Core.Messages;
using NinjaTasks.Core.ViewModels;
using NinjaTasks.Model;
using Uk.Co.Senab.Actionbarpulltorefresh.Library;

namespace NinjaTasks.App.Droid.Views
{
    [Activity(Label = "@string/app_name" 
              ,Icon = "@drawable/ic_launcher"
              ,LaunchMode = LaunchModes.SingleTask
              ,WindowSoftInputMode=WindowSoftInputModes.AdjustPan
              //,ConfigChanges = ConfigChanges.Orientation|ConfigChanges.ScreenSize|ConfigChanges.Keyboard
              )]
    [IntentFilter(Label = "@string/resolve_edit",
                  Actions = new[]
                  {
                      //Intent.ACTION_EDIT, Intent.ACTION_VIEW,
                      Intent.ACTION_SEND,
                      "com.google.android.gm.action.AUTO_SEND" // Voice command "note to self" in google search
                  },
                  Categories = new[] { Intent.CATEGORY_DEFAULT },
                  DataMimeType = "text/*")]
    public class AppView : BaseFragmentView
    {
        private const int LayoutRessource = R.Layout.AppView;

        private CtrlTaskListsList TaskListsListDirect { get { return FindViewById<CtrlTaskListsList>(R.Id.taskLists); } }
        private CtrlTaskListsList TaskListsListDrawer { get { return FindViewById<CtrlTaskListsList>(R.Id.taskListsDrawer); } }
        private CtrlTaskListsList TaskListsList { get { return TaskListsListDirect ?? TaskListsListDrawer; } }

        private CtrlTaskDetails TaskDetails { get { return FindViewById<CtrlTaskDetails>(R.Id.taskDetails); } }

        private ClickThroughDrawerLayout DrawerLayout { get { return FindViewById<ClickThroughDrawerLayout>(R.Id.drawerLayout); } }

        private CtrlTaskList TaskListCtrl { get { return FindViewById<CtrlTaskList>(R.Id.taskList); } }

        private LayoutInflater _layoutInflater;
        private InputMethodManager _inputManager;

        private PullToRefreshAttacher pullToRefreshAttacher;

        private bool _animateExit = false;
        // Changes depending on what we're showing since the started activity can receive new intents
        private bool _showingEditor = false;

        private ActionBarStateAwareDrawerToggle _drawerToggle;

        // Only not if opening note directly
        private Bundle _state;
        private bool _reverseAnimation;
        private ITasksViewModel _selectedList;
        private TodoTaskViewModel _selectedTask;
        private MvxSubscriptionToken _syncMessageToken;

        //PullToRefreshAttacher pullToRefreshAttacher;

        private new AppViewModel ViewModel { get { return (AppViewModel) base.ViewModel; } }

        private bool IsTodoListsListVisible { get { return TaskListsListDirect != null || (_drawerToggle != null && _drawerToggle.IsDrawerOpen); } }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //SetContentView(R.Layout.AppView_Drawer);
            SetContentView(LayoutRessource);
            _state = bundle;

            InitializeDrawer();

            InitlializePullToRefresh();

            var set = this.CreateBindingSet<AppView, AppViewModel>();
            set.Bind(this).For("SelectedList").To("SelectedList").OneWay();
            set.Bind(this).For("SelectedTask").To("SelectedList.SelectedPrimaryTask").OneWay();
            set.Apply();
        }

        private void HandleSyncRequest()
        {
            ViewModel.Sync();
        }

        private void OnSyncStatusMessage(SyncFinishedMessage m)
        {
            if (m.TotalSyncActive <= 0)
                pullToRefreshAttacher.SetRefreshComplete();
        }

        protected override void OnResume()
        {
            base.OnResume();

            var msg = Mvx.Resolve<IMvxMessenger>();
            _syncMessageToken = msg.SubscribeOnMainThread<SyncFinishedMessage>(OnSyncStatusMessage);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _syncMessageToken.Dispose();
            _syncMessageToken = null;

            if (pullToRefreshAttacher != null)
                pullToRefreshAttacher.SetRefreshComplete();
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            base.OnPostCreate(savedInstanceState);

            // Sync the toggle state after onRestoreInstanceState has occurred.
            if (_drawerToggle != null)
                _drawerToggle.SyncState();
        }

        public override void Finish()
        {
            base.Finish();
            if (_animateExit)
            {
                OverridePendingTransition(R.Anim.activity_slide_in_right,
                                          R.Anim.activity_slide_out_right_full);
            }
        }

        public ITasksViewModel SelectedList
        {
            get { return _selectedList; }
            [Include]
            set
            {
                _selectedList = value;
                UpdateActionBarTitle();
            }
        }

        public TodoTaskViewModel SelectedTask
        {
            get { return _selectedTask; }
            [Include]
            set
            {
                _selectedTask = value;
                if(value == null && DrawerLayout != null)
                    DrawerLayout.CloseDrawer(TaskDetails);
            }
        }

        public override bool DispatchKeyEvent(KeyEvent @event)
        {
            if (@event.KeyCode == KeyEvent.KEYCODE_BACK 
                && SupportFragmentManager.BackStackEntryCount == 0)
            {
                // Android seems to be full of bugs. This one works around OnBackPressed
                // never called when we inherit from FragmentActivity
                if(@event.Action == KeyEvent.ACTION_UP)
                    OnBackPressed();
                return true;
            }
            return base.DispatchKeyEvent(@event);
        }

        public override void OnBackPressed()
        {
            // Workaround a problem with newer versions of the support library.
            // is there an other way? 
            // http://stackoverflow.com/questions/26216088/drawer-layout-not-closing-on-back-pressed-depending-on-support-v4-lib
            // http://stackoverflow.com/questions/25166702/navigation-drawerlayout-button-never-changes-to-back-button-and-wont-navigate
            if(_drawerToggle != null && !_drawerToggle.IsDrawerClosed)
                DrawerLayout.CloseDrawers();
            else if(DrawerLayout != null && DrawerLayout.IsDrawerOpen(TaskDetails))
                DrawerLayout.CloseDrawers();
            else
                base.OnBackPressed();
        }

        #region PullToRefresh

        private void InitlializePullToRefresh() 
        {
            // TODO: Only if some sync is enabled

            pullToRefreshAttacher = PullToRefreshAttacher.Get(this);
            var listener = new PullToRefreshListenerAction(HandleSyncRequest);
            TaskListsList.FinishDelayInflate += (s, e) =>
            {
                pullToRefreshAttacher.AddRefreshableView(TaskListsList.List, listener);
            };
            TaskListCtrl.FinishDelayInflate += (s, e) =>
            {
                pullToRefreshAttacher.AddRefreshableView(TaskListCtrl.List, listener);
            };
        }

        private class PullToRefreshListenerAction : PullToRefreshAttacher.IOnRefreshListener
        {
            private readonly Action _action;

            public PullToRefreshListenerAction(Action action)
            {
                _action = action;
            }

            public void OnRefreshStarted(View view)
            {
                System.Diagnostics.Debug.WriteLine("pull-to-refresh: refresh.");
                _action();
            }
        }

        #endregion

        #region Drawer

        private void InitializeDrawer()
        {
            if (DrawerLayout == null)
                return;

            _drawerToggle = new ActionBarStateAwareDrawerToggle(this, DrawerLayout, R.Drawable.ic_drawer_dark, R.String.ok, R.String.about);
            _drawerToggle.DrawerStateChanged += OnDrawerStateChanged;
            //_drawerToggle.DrawerSlide += OnDrawerSlide;
            DrawerLayout.SetDrawerListener(_drawerToggle);

            if (TaskDetails != null)
            {
                DrawerLayout.SetDrawerShadow(R.Drawable.shadow_vertical, GravityCompat.END);
                DrawerLayout.SetDrawerLockMode(Android.Support.V4.Widget.DrawerLayout.LOCK_MODE_LOCKED_CLOSED,
                                               TaskDetails);

                TaskListCtrl.ListItemClick += (sender, e) =>
                {
                    DrawerLayout.OpenDrawer(TaskDetails);
                };

                TaskListCtrl.NewTaskGotFocus += (sender, e) =>
                {
                    DrawerLayout.CloseDrawer(TaskDetails);
                };
            }

            if (TaskListsListDrawer == null)
            {
                ActionBar.SetDisplayHomeAsUpEnabled(false);
                ActionBar.SetHomeButtonEnabled(false);
            }
            else
            {
                DrawerLayout.IsClickThrough1 = false;

                ActionBar.SetDisplayHomeAsUpEnabled(true);
                ActionBar.SetHomeButtonEnabled(true);

                UpdateActionBarTitle();

                TaskListsListDrawer.ListItemClick += (s, e) =>
                {
                    DrawerLayout.Post(() => // close the drawer AFTER databinding has done its magic
                                            // to provide a smooth scrolling experience.
                    {
                        DrawerLayout.CloseDrawer(TaskListsList);
                    });

                };
            }
        }

        private void OnDrawerStateChanged(object sender, EventArgs e)
        {
            UpdateActionBarTitle();
            InvalidateOptionsMenu(); // creates call to onPrepareOptionsMenu()
            bool isTaskDetailsVisible = _drawerToggle.VisibleDrawer == TaskDetails;
            SetHomeAsDrawer(!isTaskDetailsVisible);
            if(_drawerToggle.IsDrawerOpen && isTaskDetailsVisible)
                DrawerLayout.SetDrawerLockMode(Android.Support.V4.Widget.DrawerLayout.LOCK_MODE_UNLOCKED, TaskDetails);
            else if(_drawerToggle.IsDrawerClosed)
                DrawerLayout.SetDrawerLockMode(Android.Support.V4.Widget.DrawerLayout.LOCK_MODE_LOCKED_CLOSED, TaskDetails);
        }

        private void UpdateActionBarTitle()
        {
            if (_drawerToggle != null && _drawerToggle.VisibleDrawer == TaskListsListDrawer)
            {
                ActionBar.SetTitle(R.String.show_from_all_lists);
            }
            else
            {
                if (ViewModel.SelectedList == null)
                    ActionBar.SetTitle(R.String.app_name);
                else
                    ActionBar.Title = ViewModel.SelectedList.Description;
            }
        }

        private void SetHomeAsDrawer(bool value) 
        {
            if(_drawerToggle != null)
                _drawerToggle.IsDrawerIndicatorEnabled = value;
        }

        #endregion

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            Intent = intent;
            //CreateTaskListFragment();

            //// Just to be sure it gets done
            //// Clear notification if present
            //clearNotification(intent);
        }

        public override void OnConfigurationChanged(Configuration configuration)
        {
            base.OnConfigurationChanged(configuration);
            if (_drawerToggle != null) 
                _drawerToggle.OnConfigurationChanged(configuration);
            //SetContentView(LayoutRessource);
        }

        ///// <summary>
        ///// Opens the specified list and closes the left drawer
        ///// </summary>
        //public void OpenList(long id) 
        //{
        //    // Open list
        //    Intent i = new Intent(ActivityMain.this, ActivityMain_.class);
        //    i.setAction(Intent.ACTION_VIEW).setData(TaskList.getUri(id))
        //            .addFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP);

        //    // If editor is on screen, we need to reload fragments
        //    if (listOpener == null) {
        //        ClearFragmentBackStack();
        //        reverseAnimation = true;
        //        startActivity(i);
        //    } else {
        //        // If not popped, then send the call to the fragment
        //        // directly
        //        Log.d("nononsenseapps list", "calling listOpener");
        //        listOpener.openList(id);
        //    }

        //    // And then close drawer
        //    if (drawerLayout != null && leftDrawer != null) {
        //        drawerLayout.closeDrawer(leftDrawer);
        //    }
        //}

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            MenuInflater.Inflate(R.Menu.activity_main, menu);
            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            menu.SetGroupVisible(R.Id.activity_menu_group, TaskListsListDrawer == null || !IsTodoListsListVisible);
            menu.SetGroupVisible(R.Id.activity_listslist_menu_group, IsTodoListsListVisible);

            return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Pass the event to ActionBarDrawerToggle, if it returns
            // true, then it has handled the app icon touch event
            if (_drawerToggle != null && _drawerToggle.OnOptionsItemSelected(item))
                return true;

            // Handle your other action bar items...
            int itemId = item.ItemId;
            if (itemId == Android.R.Id.Home)
            {
                if (_drawerToggle != null && _drawerToggle.IsDrawerOpen)
                {
                    DrawerLayout.CloseDrawers();
                    return true;
                }
                //if (_showingEditor)
                //{
                //    // Only true in portrait mode
                //    View focusView = CurrentFocus;
                //    if (_inputManager != null && focusView != null)
                //    {
                //        _inputManager.HideSoftInputFromWindow(focusView.WindowToken,
                //            InputMethodManager.HIDE_NOT_ALWAYS);
                //    }

                //    // Should load the same list again
                //    // Try getting the list from the original intent
                //    long listId = GetListId(this.Intent);

                //    Intent intent = new Intent().SetAction(Intent.ACTION_VIEW)
                //                                .SetClass(this, typeof (AppView));
                    
                //    if (listId > 0)
                //    {
                //        //intent.Set(TodoList.ColId, listId);
                //    }

                //    // Set the intent before, so we set the correct action bar
                //    Intent = intent;

                //    _reverseAnimation = true;
                //    intent.SetFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP | Intent.FLAG_ACTIVITY_NEW_TASK);
                //    StartActivity(intent);
                //}
                // else
                // Handled by drawer
                return false;
            }
            if (itemId == R.Id.drawer_menu_createlist)
            {
                // Show fragment
                var dlg = new EditListDialog();
                dlg.ViewModel = new EditListViewModel(ViewModel.Lists);
                dlg.Show(SupportFragmentManager, dlg.GetType().Name);
                return true;
            }
            if (itemId == R.Id.menu_preferences)
            {
                ViewModel.SetupSync();
                return true;
            }
            if (itemId == R.Id.menu_toggle_completed)
            {
                var cfg = Mvx.Resolve<INinjaTasksConfigurationService>();
                cfg.Cfg.ShowCompletedTasks = !cfg.Cfg.ShowCompletedTasks;
                cfg.Save();
                return true;
            }
            //else if (itemId == R.Id.menu_sync)
            //{
            //    handleSyncRequest();
            //    return true;
            //}
            //else if (itemId == R.id.menu_delete)
            //{
            //    return false;
            //}
             return false;
        }

        #region Intent Evaluation
       /// <summary>
        /// Returns a list id from an intent if it contains one, either as part of
        /// its URI or as an extra.
        /// <para>
        /// Returns -1 if no id was contained, this includes insert actions
        /// </para>
        /// </summary>
        private long GetListId(Intent intent) 
        {
            long retval = -1;
            if (intent != null && intent.Data != null 
                && (Intent.ACTION_EDIT.Equals(intent.Action) 
                  ||Intent.ACTION_VIEW.Equals(intent.Action) 
                  ||Intent.ACTION_INSERT.Equals(intent.Action))) 
            {
                //if ((intent.Data.Path.StartsWith(NotePad.Lists.PATH_VISIBLE_LISTS) 
                //   ||intent.Data.Path.StartsWith(NotePad.Lists.PATH_LISTS) 
                //   ||intent.Data.Path.StartsWith(TaskList.URI.getPath()))) 
                //{
                //    try { retval = long.Parse(intent.Data.LastPathSegment); } 
                //    catch (Exception ex) { retval = -1; }
                //} 
                if(retval == -1)
                    retval = intent.GetLongExtra(TodoTask.ColListFk, -1);
                if(retval == -1)
                    retval = intent.GetLongExtra(TodoList.ColId, -1);
                //if(retval == -1)
                //    retval = intent.GetLongExtra(TaskDetailFragment.ARG_ITEM_LIST_ID, -1);
            }
            return retval;
        }

        private string GetNoteShareText(Intent intent) 
        {
            if (intent == null || intent.Extras == null)
                return "";

            StringBuilder retval = new StringBuilder();
            
            // possible title
            if (intent.Extras.ContainsKey(Intent.EXTRA_SUBJECT) 
              &&intent.Action != "com.google.android.gm.action.AUTO_SEND")
            {
                retval.Append(intent.Extras.Get(Intent.EXTRA_SUBJECT));
            }
            
            // possible note
            if (intent.Extras.ContainsKey(Intent.EXTRA_TEXT)) 
            {
                if (retval.Length > 0)
                    retval.Append(": ");
                retval.Append(intent.Extras.Get(Intent.EXTRA_TEXT));
            }
            return retval.ToString();
        }
     
        private bool IsNoteIntent(Intent intent) 
        {
            if (intent == null) 
                return false;
            
            if (intent.Action == Intent.ACTION_SEND 
             || intent.Action == "com.google.android.gm.action.AUTO_SEND") 
                return true;

            if (intent.Data == null)
                return false;

            bool isEditViewOrInsert = intent.Action == Intent.ACTION_EDIT
                                   || intent.Action == Intent.ACTION_VIEW
                                   || intent.Action == Intent.ACTION_INSERT
                                   || intent.Action == Intent.ACTION_INSERT_OR_EDIT;

            //if(isEditViewOrInsert &&
            //    //intent.Data.Path.StartsWith(LegacyDBHelper.NotePad.Notes.PATH_VISIBLE_NOTES) ||
            //    //intent.Data.Path.StartsWith(LegacyDBHelper.NotePad.Notes.PATH_NOTES) ||
            //     intent.Data.Path.StartsWith(Task.URI.Path)) 
            //    && !intent.Data().getPath().startsWith(TaskList.URI.Path)) {
            //    return true;
            //}
            return false;
        }
        #endregion
    }
}
