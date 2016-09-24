using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;
using NinjaSync.Model.Journal;
using NinjaTools;
using System.Linq.Expressions;

namespace NinjaSync.Storage
{
    public enum SelectionMode
    {
        SelectSpecified,
        SelectNotSpecified,
    }

    public interface ITrackableStorage
    {
        /// <summary>
        /// this is a unique id 
        /// </summary>
        string StorageId { get; }

        ITrackable GetById(TrackableId id);
        IEnumerable<ITrackable> GetById(params TrackableId[] ids);
        IEnumerable<TrackableId> GetIds(SelectionMode mode, TrackableType type, params string[] ids);

        /// <summary>
        /// when no parameters are given, will return all trackables.
        /// </summary>
        IEnumerable<ITrackable> GetTrackable(params TrackableType[] limitToSpecifiedTypes);

        void Save(ITrackable obj, ICollection<string> properties);
        
        void Delete(SelectionMode mode, params TrackableId[] id);

        /// <summary>
        /// this deleted everyting.
        /// </summary>
        void Clear();

        [DebuggerHidden]
        void RunInTransaction([InstantHandle] Action a);
        [DebuggerHidden]
        void RunInImmediateTransaction([InstantHandle] Action a);

        ITransaction BeginTransaction();
    }

    public static class TrackableStorageExtensions
    {
        public static bool Exists(this ITrackableStorage storage, TrackableId obj)
        {
            return storage.GetIds(SelectionMode.SelectSpecified, obj.Type, obj.ObjectId).Any();
        }

        public static void Save(this ITrackableStorage storage, ITrackable obj)
        {
            storage.Save(obj, null);
        }

        public static void Save(this ITrackableStorage storage, ITrackable obj, params string[] properties)
        {
            storage.Save(obj, properties);
        }

#if !DOT42
        public static void Save<T>(this ITrackableStorage storage, ITrackable obj, params Expression<Func<T, object>>[] properties)
        {
            storage.Save(obj, properties.Select(ExpressionHelper.GetMemberName).ToList());
        }
#endif

    }
}