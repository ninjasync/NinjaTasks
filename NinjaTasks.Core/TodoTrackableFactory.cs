using System;
using NinjaSync;
using NinjaSync.Model.Journal;
using NinjaTasks.Model;

namespace NinjaTasks.Core
{
    public class TodoTrackableFactory: ITrackableFactory
    {
        public TrackableType[] Types { get { return new[] {TrackableType.List, TrackableType.Task}; } }
        public string GetName(TrackableType type)
        {
            return type.ToString();
        }

        public Type GetType(TrackableType type)
        {
            if (type == TrackableType.List)
                return typeof(TodoList);
            if (type == TrackableType.Task)
                return typeof(TodoTask);
            throw new Exception("unsupported type: " + type);
        }

        public ITrackable Create(TrackableType type)
        {
            if (type == TrackableType.List)
                return new TodoList();
            if (type == TrackableType.Task)
                return new TodoTask();
            throw new Exception("unsupported type: " + type);
        }
    }
}
