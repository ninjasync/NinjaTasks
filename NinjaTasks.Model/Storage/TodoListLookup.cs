using System.Collections.Generic;
using System.Linq;
using NinjaSync.Model.Journal;
using NinjaSync.Storage;

namespace NinjaTasks.Model.Storage
{
    public class TodoListLookup
    {
        private bool _initialized;
        private readonly ITrackableStorage _storage;
        
        readonly Dictionary<string, TodoList> _ids = new Dictionary<string, TodoList>();
        readonly Dictionary<string, TodoList> _names = new Dictionary<string, TodoList>();

        public IList<TodoList> Lists { get { return _ids.Values.ToList(); } }

        public TodoListLookup()
        {
            _initialized = true;
        }

        public TodoListLookup(ITrackableStorage storage)
        {
            _storage = storage;
        }

        public TodoListLookup(IEnumerable<TodoList> storage)
        {
            foreach(var list in storage)
                Add(list);
            _initialized = true;
        }

        public TodoList GetById(string id)
        {
            EnsureInitialized();
            TodoList list;
            if (!_ids.TryGetValue(id, out list))
                return null;
            return list;
        }

        public TodoList GetByName(string description)
        {
            EnsureInitialized();
            TodoList list;
            if (!_names.TryGetValue(description??"", out list))
                return null;
            return list;
        }

        private void EnsureInitialized()
        {
            if (_initialized || _storage == null) return;

            foreach (var list in _storage.GetTrackable(TrackableType.List).Cast<TodoList>())
                Add(list);

            _initialized = true;
        }

        /// <summary>
        /// add a list (not to the backing storage). if a list with
        /// the same name or id already exists, nothing is done.
        /// </summary>
        /// <param name="list"></param>
        public void Add(TodoList list)
        {
            if (!_ids.ContainsKey(list.Id)) 
                _ids.Add(list.Id, list);
            if(!_names.ContainsKey(list.Description??""))
                _names.Add(list.Description??"", list);
        }
    }
}