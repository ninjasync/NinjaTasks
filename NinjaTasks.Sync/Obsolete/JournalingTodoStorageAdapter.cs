using System;
using System.Collections.Generic;
using System.Diagnostics;
using NinjaTasks.Model;
using NinjaTasks.Model.Storage;

namespace NinjaTasks.Sync.Obsolete
{
    /// <summary>
    /// this adds a journaling layer on top of the ITodoStorage service.
    /// <para/>
    /// Note: this class is not used, since it's functionality was implemented
    ///       as sqlite triggers. Keep it here as a reference, if it 
    ///       is ever needed with other storage implementations.
    /// </summary>
    public class JournalingTodoStorageAdapter : ITodoStorage
    {
        private readonly ITodoStorage _storage;
        private readonly IJournalStorageService _journalStorage;
        private readonly ModificationTracker _tracker = new ModificationTracker();

        public ITodoStorage Storage { get { return _storage; } }

        public JournalingTodoStorageAdapter(ITodoStorage storage, IJournalStorageService journalStorage)
        {
            _storage = storage;
            _journalStorage = journalStorage;
        }

        public void SaveList(TodoList obj)
        {
            bool isCreated = obj.Id == 0;

            _storage.RunInTransaction(() =>
            {
                _storage.SaveList(obj);
                HandleSaveTracking(obj, isCreated);
            });

        }

        public void SaveTask(TodoTask obj)
        {
            bool isCreated = obj.Id == 0;

            _storage.RunInTransaction(() =>
            {
                _storage.SaveTask(obj);
                HandleSaveTracking(obj, isCreated);
            });
        }

        public void DeleteList(TodoList list, List<int> returnDeletedTaskIds)
        {
            DeleteLists(SelectionMode.SelectSpecified, new [] { list.Id}, returnDeletedTaskIds,null);
        }

        public void DeleteLists(SelectionMode selectionMode, IList<int> ids, List<int> returnDeletedTaskIds = null, List<int> returnDeletedListIds = null)
        {
            _storage.RunInTransaction(() =>
            {
                if (returnDeletedTaskIds == null)
                    returnDeletedTaskIds = new List<int>();
                if (returnDeletedListIds == null)
                    returnDeletedListIds = new List<int>();

                _storage.DeleteLists(selectionMode, ids, returnDeletedTaskIds, returnDeletedListIds);

                var now = DateTime.UtcNow;

                foreach (int listId in returnDeletedTaskIds)
                {
                    JournalEntry record = new JournalEntry();
                    record.Change = ChangeType.Deleted;
                    record.Timestamp = now;
                    record.Type = TodoList.Type;
                    record.ObjectId = listId;
                    _journalStorage.AddEntry(record);
                }

                foreach (int taskId in returnDeletedTaskIds)
                {
                    JournalEntry record = new JournalEntry();
                    record.Change = ChangeType.Deleted;
                    record.Timestamp = now;
                    record.Type = TodoTask.Type;
                    record.ObjectId = taskId;
                    _journalStorage.AddEntry(record);
                }
            });
        }

        public void DeleteTask(TodoTask task)
        {
            _storage.RunInTransaction(() =>
            {
                _tracker.StopTracking(task);
                ObjectDeleted(task);
                _storage.DeleteTask(task);
            });
        }

        public void DeleteTasks(SelectionMode selectionMode, IList<int> ids, List<int> returnDeletedTaskIds = null)
        {
            _storage.RunInTransaction(() =>
            {
                if (returnDeletedTaskIds == null)
                    returnDeletedTaskIds = new List<int>();
                
                _storage.DeleteTasks(selectionMode, ids, returnDeletedTaskIds);

                foreach (int taskId in returnDeletedTaskIds)
                {
                    JournalEntry record = new JournalEntry();
                    record.Change = ChangeType.Deleted;
                    record.Timestamp = DateTime.UtcNow;
                    record.Type = TodoTask.Type;
                    record.ObjectId = taskId;
                    _journalStorage.AddEntry(record);
                }
            });
        }


        private void HandleSaveTracking(ITrackable obj, bool wasCreated)
        {
            if (wasCreated)
            {
                ObjectCreated(obj);
                _tracker.Track(obj);
            }
            else
            {
                foreach (var member in _tracker.GetModifications(obj, true))
                    ObjectModified(obj, member);
            }
        }

        public IEnumerable<TodoList> GetLists(params int[] id)
        {
            foreach (var list in _storage.GetLists(id))
            {
                _tracker.Track(list);
                yield return list;
            }
        }

        public IEnumerable<TodoTask> GetTasks(TodoList list = null, bool includeComplete = false, bool onlyHighPriority = false, params int[] ids)
        {
            foreach (var track in _storage.GetTasks(list, includeComplete, onlyHighPriority, ids))
            {
                _tracker.Track(track);
                yield return track;
            }
        }

        public IEnumerable<TodoTask> FindTasks(string searchText)
        {
            foreach (var track in _storage.FindTasks(searchText))
            {
                _tracker.Track(track);
                yield return track;
            }
        }

        public void ObjectCreated(ITrackable obj)
        {
            JournalEntry je = new JournalEntry();
            je.Timestamp = DateTime.UtcNow;
            je.Change = ChangeType.Created;
            je.Type = obj.JournalingType;
            je.ObjectId = obj.Id;
            _journalStorage.AddEntry(je);
        }

        public void ObjectModified(ITrackable obj, string member)
        {
            Debug.Assert(obj.Id != 0);

            JournalEntry je = new JournalEntry();
            je.Timestamp = DateTime.UtcNow;
            je.Change = ChangeType.Modified;
            je.Type = obj.JournalingType;
            je.ObjectId = obj.Id;
            je.Member = member;

            _journalStorage.AddOrReplaceEntry(je);
        }

        public void ObjectDeleted(ITrackable obj)
        {
            JournalEntry je = new JournalEntry();
            je.Timestamp = DateTime.UtcNow;
            je.Change = ChangeType.Deleted;
            je.Type = obj.JournalingType;
            je.ObjectId = obj.Id;

            _journalStorage.AddEntry(je);
        }


        public void RunInTransaction(Action a)
        {
            _storage.RunInTransaction(a);
            
        }

        public void RunInExclusiveTransaction(Action a)
        {
            _storage.RunInExclusiveTransaction(a);
        }
    }
}
