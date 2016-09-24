using System.Threading.Tasks;
using NinjaTools.MVVM;

namespace NinjaTasks.Core.ViewModels
{
    public class EditListViewModel : BaseViewModel
    {
        private readonly TaskListViewModel _list;
        private readonly TodoListsViewModel _lists;

        public bool IsNewList { get { return _list == null; } }

        public string Description { get; set; }

        public EditListViewModel(TodoListsViewModel lists)
        {
            _lists = lists;
            Description = "";
        }

        public EditListViewModel(TodoListsViewModel lists, TaskListViewModel list)
        {
            _lists = lists;
            _list = list;
            Description = list.Description;
        }

        public async Task Delete()
        {
            if (IsNewList)
                return;

            if(await _lists.DeleteList(_list))
                Close(this);
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(Description))
                return;

            if (IsNewList)
                _lists.AddList(Description.Trim());
            else
                _list.Description = Description.Trim();
        }
    }
}
