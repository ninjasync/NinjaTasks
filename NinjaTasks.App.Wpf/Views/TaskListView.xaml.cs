using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MvvmCross.Plugin.Messenger;
using GongSolutions.Wpf.DragDrop;
using NinjaTasks.Core.ViewModels;
using NinjaTasks.Model.Storage.Mocks;
using NinjaTools;
using NinjaTools.GUI.Wpf;
using Microsoft.Win32;
using System.IO;

namespace NinjaTasks.App.Wpf.Views
{

    public partial class TaskListView :  IDropTarget
    {
        private readonly DefaultDropHandler _defaultDrop = new DefaultDropHandler();
        private readonly ViewModelAccess<ITasksViewModel> _vm;

        public TaskListView()
        {
            this.InitializeComponent();

            _vm = new ViewModelAccess<ITasksViewModel>(this);
            _vm.ModelChanged += OnModelChanged;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateGroupingAndSorting();
        }

        private void OnModelChanged(object sender, ValueChangedEventArgs<ITasksViewModel> e)
        {
            if (_vm.Model == null) return;

            UpdateGroupingAndSorting();

            // scroll to top...
            ScrollViewer scrollViewer = GetVisualChild<ScrollViewer>(List);
            if (scrollViewer != null)
                scrollViewer.ScrollToHome();
        }

        private void UpdateGroupingAndSorting()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            CollectionViewSource viewSource = (CollectionViewSource)Resources["TasksCollectionViewSource"];

            if (viewSource == null || viewSource.View == null) return;

            using (viewSource.DeferRefresh())
            {
                viewSource.GroupDescriptions.Clear();

                if (!_vm.Model.HasMultipleLists)
                {
                    viewSource.GroupDescriptions.Add(new PropertyGroupDescription("IsCompleted"));
                    //always show "true" group, so that the sort order stays in place
                    viewSource.GroupDescriptions.First().GroupNames.Add(false);
                }
                else
                {
                    viewSource.GroupDescriptions.Add(new PropertyGroupDescription("ListDescription"));
                }
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var isControl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            if (e.Key == Key.A && isControl)
            {
                // don't select non-shown items. [atm: dont select completed tasks...]
                var vm = DataContext as TasksViewModelBase;
                if (vm != null /*&& !vm.ShowCompletedTasks*/)
                {
                    e.Handled = true;

                    foreach (TodoTaskViewModel m in List.Items)
                    {
                        if (!m.IsCompleted) 
                            List.SelectedItems.Add(m);
                    }
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var isControl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            if (e.Key == Key.C && isControl)
            {
                var vm = DataContext as TasksViewModelBase;
                if (vm != null)
                {
                    var clip = vm.GetCopyText();
                    Clipboard.SetText(clip);
                }
            }
        }

        public new void DragOver(IDropInfo dropInfo)
        {
            var viewModel = DataContext as TaskListViewModel;
            if (viewModel == null)
            {
                dropInfo.NotHandled = true;
                return;
            }

            // check if we are dropping on the completed group...
            bool isUpperDrop = dropInfo.TargetGroup != null && dropInfo.TargetGroup.Name is bool &&
                               !((bool) dropInfo.TargetGroup.Name);
            if (!isUpperDrop)
            {
                dropInfo.Effects = DragDropEffects.Move;
                //dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                return;
            }

            _defaultDrop.DragOver(dropInfo);

        }

        public new void Drop(IDropInfo dropInfo)
        {
            var viewModel = DataContext as TaskListViewModel;
            if (viewModel == null) return;

            bool isUpperDrop = dropInfo.TargetGroup != null && dropInfo.TargetGroup.Name is bool &&
                               !((bool)dropInfo.TargetGroup.Name);

            // is dropping on lower part: complete the tasks.
            bool isTask = dropInfo.Data is TodoTaskViewModel || dropInfo.Data is IList<TodoTaskViewModel>;
            bool isSimple = dropInfo.Data is TodoTaskViewModel;

            if (!isTask) return;

            IList<TodoTaskViewModel> tasks = !isSimple
                    ? (IList<TodoTaskViewModel>) dropInfo.Data
                    : new[] {(TodoTaskViewModel) dropInfo.Data};

            viewModel.MoveToPosition(tasks, dropInfo.InsertIndex, !isUpperDrop);
        }

        private void OpenAttachment_Click(object sender, RoutedEventArgs e)
        {
            TodoTaskViewModel task = (sender as FrameworkElement).DataContext as TodoTaskViewModel;
            if (task == null) return;

            (string filename, byte[] data) = task.GetAttachment();

            if (data == null)
                return;

            FileDialog dlg = new SaveFileDialog()
            {
                Title = "Save Attachment",
                OverwritePrompt = true,
                FileName = filename
            };

            if (dlg.ShowDialog() != true)
                return;

            File.WriteAllBytes(dlg.FileName, data);
        }

        private void SetAttachment_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                Title = "Select Attachment",
                CheckFileExists = true,
                Multiselect = false,
            };

            if (dlg.ShowDialog() != true)
                return;

            var data = File.ReadAllBytes(dlg.FileName);
            _vm.Model.SelectedPrimaryTask.SetAttachment(Path.GetFileName(dlg.FileName), data);
        }

        private T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }


    }

    //internal class TodoTaskViewModelSorter : IComparer
    //{
    //    public int Compare(object x1, object y1)
    //    {
    //        var x = x1 as TodoTaskViewModel;
    //        var y = y1 as TodoTaskViewModel;
    //        if (x == null && y == null) return 0;
    //        if (x == null) return -1;
    //        if (y == null) return 1;
    //        if (x.List.SortPosition > y.List.SortPosition) return 1;
    //        if (x.List.SortPosition < y.List.SortPosition) return -1;
    //        if (x.List.List.CreatedAt > y.List.List.CreatedAt) return 1;
    //        if (x.List.List.CreatedAt < y.List.List.CreatedAt) return -1;

    //        if (x.IsCompleted && !y.IsCompleted) return 1;
    //        if (!x.IsCompleted && y.IsCompleted) return -1;

    //        if (x.SortPosition > y.SortPosition) return 1;
    //        if (x.SortPosition < y.SortPosition) return -1;

    //        if (x.IsPriority && !y.IsPriority) return -1;
    //        if (!x.IsPriority && y.IsPriority) return 1;

    //        return 0;
    //    }
    //}

    public class MockTaskListViewModel : TaskListViewModel
    {
        public MockTaskListViewModel()
            : base(MockTodoStorage.Instance.GetLists().First(), MockTodoStorage.Instance, new MvxMessengerHub(), null)
        {
            base.OnActivate();
        }

    }
}
