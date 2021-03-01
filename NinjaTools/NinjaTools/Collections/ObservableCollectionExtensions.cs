using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinjaTools.Collections
{
    /// <summary>
    /// externs to allow replacing items in a list while preserving thoses items
    /// already in the list, resulting in a minimum change update.
    /// </summary>
    public static class ObservableCollectionExtensions
    {
        public static int ReplaceWith<T, E>(this ObservableCollection<T> target, IList<T> source,
                                   Func<T, E> equalitySelector,
                                   T selectedItem = default(T),
                                   Action<T, T> replace = null,
                                   Action<T>    removed = null)
        {
            return ReplaceWith(target, source,
                               (a, b) => Equals(equalitySelector(a), equalitySelector(b)),
                               (a, b) => Equals(equalitySelector(a), equalitySelector(b)),
                               //(a, b) => Equals(equalitySelector(a), equalitySelector(b)),
                               a => equalitySelector(a).GetHashCode(),
                               a => equalitySelector(a).GetHashCode(),
                               a => a,
                               selectedItem,
                               (i, newItem) => replace?.Invoke(target[i], newItem),
                               removed);
            ;        }

        public static int ReplaceWith<T>(this ObservableCollection<T> target, IList<T> source,
                                   T selectedItem = default(T),
                                   IEqualityComparer<T> equals = null,
                                   Action<T, T> replace = null,
                                   Action<T> removed = null)
        {
            equals = equals ?? EqualityComparer<T>.Default;
            return ReplaceWith(target, source, 
                               (a, b) => equals.Equals(a, b),
                               (a, b) => equals.Equals(a, b),
                               //(a, b) => equals.Equals(a, b),
                               a => equals.GetHashCode(a),
                               a => equals.GetHashCode(a),
                               a => a,
                               selectedItem,
                               (i, newItem) => replace?.Invoke(target[i], newItem),
                               removed);
        }

        ///// <summary>
        ///// Note: O(n^2)
        ///// </summary>
        //public static int ReplaceWith<T>(this ObservableCollection<T> target, IList<T> source,
        //                           T selectedItem = default(T),
        //                           IEqualityComparer<T> equals = null,
        //                           Action<int, T> replace = null,
        //                           Action<T> removed = null)
        //{
        //    equals = equals ?? EqualityComparer<T>.Default;
        //    return ReplaceWith(target, source, (a,b)=>equals.Equals(a,b), 
        //                       a => a, selectedItem, replace, removed);
        //}

        public static int ReplaceWith<T, S>(this ObservableCollection<T> target, IList<S> source,
                                      Func<T, S, bool> equals,
                                      //Func<T, T, bool> targetEquals,
                                      Func<S, S, bool> sourceEquals,
                                      Func<T, int> getTargetHashCode,
                                      Func<S, int> getSourceHashCode,
                                      Func<S, T> toViewModel,
                                      T selectedItem = default(T),
                                      Action<int, S> replace = null,
                                      Action<T> removed = null)
        {
            if (source == null)
            {
                target.Clear();
                return -1;
            }

            List<int> removeIdxes = new List<int>();
            int removedSelectionIdx = -1;

            var crossEqualityComparer = new CrossEqualityComparer<T, S>(@equals, getTargetHashCode, getSourceHashCode, null, sourceEquals); 
            var sourceHashSet = new HashSet<object>(source.Cast<object>(), crossEqualityComparer);

            for (int idx = 0; idx < target.Count; idx++)
            {
                var oldItem = target[idx];
                if (!sourceHashSet.Contains(oldItem))
                {
                    if (Equals(selectedItem, oldItem))
                        removedSelectionIdx = idx - removeIdxes.Count;
                    removeIdxes.Add(idx);
                }
            }

            // remove in reverse order, hopefully gaining some performance.
            foreach (var idx in removeIdxes.FastReverse())
            {
                var oldItem = target[idx];
                target.RemoveAt(idx);
                removed?.Invoke(oldItem);
            }

            int nextIdx = 0;
            foreach (var newItem in source)
            {
                int oldIdx;
                if(target.Count > nextIdx && equals(target[nextIdx], newItem))
                    oldIdx = nextIdx;
                else 
                    oldIdx = target.IndexOf(old => equals(old, newItem));

                if (oldIdx >= 0)
                {
                    if (oldIdx != nextIdx)
                        target.Move(oldIdx, nextIdx);

                    replace?.Invoke(nextIdx, newItem);
                }
                else
                {
                    var newVm = toViewModel(newItem);
                    target.Insert(nextIdx, newVm);
                }
                ++nextIdx;
            }

            return removedSelectionIdx;
        }

        private class CrossEqualityComparer<T, S> : IEqualityComparer<object>
        {
            private readonly Func<T, S, bool> _equals;
            private readonly Func<T, int> _getTargetHashCode;
            private readonly Func<S, int> _getSourceHashCode;
            private readonly Func<T, T, bool> _targetEquals;
            private readonly Func<S, S, bool> _sourceEquals;

            public CrossEqualityComparer(Func<T, S, bool> equals,
                                         Func<T, int> getTargetHashCode,
                                         Func<S, int> getSourceHashCode,
                                         Func<T, T, bool> targetEquals = null,
                                         Func<S, S, bool> sourceEquals = null)
            {
                _equals = equals;
                _getTargetHashCode = getTargetHashCode;
                _getSourceHashCode = getSourceHashCode;
                _targetEquals = targetEquals;
                _sourceEquals = sourceEquals;
            }

            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                if (x == null && y == null)
                    return true;
                if (x == null || y == null)
                    return false;

                if (x is T && y is S)
                    return _equals((T) x, (S) y);
                if (x is S && y is T)
                    return _equals((T)y, (S)x);
                if (x is T && y is T)
                    return _targetEquals((T)x, (T)y);
                if (x is S && y is S)
                    return _sourceEquals((S)x, (S)y);

                throw new InvalidOperationException();
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                if (obj == null) return 0;

                if (obj is T)
                    return _getTargetHashCode((T) obj);

                if (obj is S)
                    return _getSourceHashCode((S)obj);

                throw new InvalidOperationException();
            }
        }
    }
}
