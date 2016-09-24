using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NinjaTools.Collections
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// Note: O(n^2)
        /// </summary>
        public static int ReplaceWith<T>(this ObservableCollection<T> target, IList<T> source,
                                   T selectedItem = default(T),
                                   IEqualityComparer<T> equals = null,
                                   Action<T, T> replace = null,
                                   Action<T> removed = null)
        {
            equals = equals ?? EqualityComparer<T>.Default;
            return ReplaceWith(target, source, (a, b) => equals.Equals(a, b),
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

        /// <summary>
        /// Note: O(n^2)
        /// </summary>
        public static int ReplaceWith<T, V>(this ObservableCollection<T> target, IList<V> source,
                                      Func<T, V, bool> equals,
                                      Func<V, T> toViewModel,
                                      T selectedItem = default(T),
                                      Action<int, V> replace = null,
                                      Action<T> removed = null)
        {
            if (source == null)
            {
                target.Clear();
                return -1;
            }

            int removedSelectionIdx = -1;

            for (int idx = 0; idx < target.Count; idx++)
            {
                var oldItem = target[idx];
                if (source.All(newItem => !equals(oldItem, newItem)))
                {
                    if (Equals(selectedItem, oldItem))
                        removedSelectionIdx = idx;

                    target.RemoveAt(idx--);
                    removed?.Invoke(oldItem);
                }
            }
            int nextIdx = 0;
            foreach (var newItem in source)
            {
                int oldIdx = target.IndexOf(old => equals(old, newItem));

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
    }
}
