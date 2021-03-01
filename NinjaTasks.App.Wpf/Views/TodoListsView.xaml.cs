using System.Collections.Generic;
using System.Windows;
using MvvmCross.Plugin.Messenger;
using GongSolutions.Wpf.DragDrop;
using NinjaTasks.Core.ViewModels;
using NinjaTasks.Model.Storage.Mocks;
using NinjaTools.Npc;

namespace NinjaTasks.App.Wpf.Views
{
    public partial class TodoListsView : IDropTarget, IDragSource
    {
        private readonly DefaultDropHandler _defaultDrop = new DefaultDropHandler();
        private readonly DefaultDragHandler _defaultDrag = new DefaultDragHandler();

        public TodoListsView()
        {
            this.InitializeComponent();

        }

        public new void DragOver(IDropInfo dropInfo)
        {
            bool isTask = dropInfo.Data is TodoTaskViewModel || dropInfo.Data is IList<TodoTaskViewModel>;
            if (isTask && dropInfo.TargetItem is TasksViewModelBase)
            {
                dropInfo.Effects = DragDropEffects.Move;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            }
            else
            {
                var vm = dropInfo.TargetItem as TasksViewModelBase;
                if (vm == null || !vm.AllowReorder)
                {
                    //dropInfo.NotHandled = true;
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }

                try { _defaultDrop.DragOver(dropInfo); }
                catch { dropInfo.NotHandled = true; }
                
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public new void Drop(IDropInfo dropInfo)
        {
            bool isTask = dropInfo.Data is TodoTaskViewModel || dropInfo.Data is IList<TodoTaskViewModel>;
            if (isTask && dropInfo.TargetItem is TasksViewModelBase)
            {
                var targetTasks = ((TasksViewModelBase)dropInfo.TargetItem);
                var currentTasks = ((TodoListsViewModel) DataContext).SelectedList;

                bool isSimple = dropInfo.Data is TodoTaskViewModel;
                IList<TodoTaskViewModel> tasks = !isSimple
                    ? (IList<TodoTaskViewModel>) dropInfo.Data
                    : new[] {(TodoTaskViewModel) dropInfo.Data};
                targetTasks.MoveInto(tasks, currentTasks);
            }
            else
            {
                try { _defaultDrop.Drop(dropInfo); }
                catch { dropInfo.NotHandled = true; }
            }
        }

        public void StartDrag(IDragInfo dragInfo)
        {
            _defaultDrag.StartDrag(dragInfo);
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            var tasksvm = dragInfo.SourceItem as TasksViewModelBase;
            if (tasksvm == null || !tasksvm.AllowReorder) return false;

            return _defaultDrag.CanStartDrag(dragInfo);
        }

        public void Dropped(IDropInfo dropInfo)
        {
            _defaultDrag.Dropped(dropInfo);
        }

        public void DragCancelled()
        {
            _defaultDrag.DragCancelled();
        }
    }  
    
    public class MockTodoListsViewModel : TodoListsViewModel
    {
        public MockTodoListsViewModel()
            : base(new MockTodoStorage(), null, new MvxMessengerHub(), null)
        {
            OnActivate();
        }
    }

}
