using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;

namespace NinjaTools.Collections
{
    public static class CollectionExtensions
    {
        [Pure]
        public static IEnumerable<T> TakeRandom<T>(this ICollection<T> elements, int maxCount, int seed=-1)
        {
            if (maxCount < 1) yield break;
            if (maxCount >= elements.Count)
            {
                foreach (var e in elements)
                    yield return e;
                yield break;
            }

            Random random = seed == -1?new Random():new Random(seed);
            int itemsLeft = elements.Count;

            foreach (var e in elements)
            {
                double propability = (double)maxCount / itemsLeft;
                if (random.NextDouble() < propability)
                {
                    yield return e;
                    if (--maxCount <= 0)
                        yield break;
                }
                --itemsLeft;
            }
        }

        [Pure]
        public static int FindIndex<T>(this ICollection<T> c, [InstantHandle] Predicate<T> predicate)
        {
            var n = c.Select((item, index) => new {Item = item, Index = index}).FirstOrDefault(i => predicate(i.Item));
            
            if (n == null) return -1;
            return n.Index;
        }

        /// <summary>
        /// Adds the elements of the specified collection to the specified generic IList.
        /// </summary>
        /// <param name="initial">The list to add to.</param>
        /// <param name="collection">The collection of elements to add.</param>
        public static void AddRange<T>(this IList<T> initial, IEnumerable<T> collection)
        {
            if (initial == null)
                throw new ArgumentNullException(nameof(initial));

            if (collection == null)
                return;

            foreach (T value in collection)
            {
                initial.Add(value);
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (T value in collection)
            {
                if (predicate(value))
                    return index;

                index++;
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of the first occurrence in a sequence by using a specified IEqualityComparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="list">A sequence in which to locate a value.</param>
        /// <param name="value">The object to locate in the sequence</param>
        /// <param name="comparer">An equality comparer to compare values.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire sequence, if found; otherwise, –1.</returns>
        public static int IndexOf<TSource>(this IEnumerable<TSource> list, TSource value, IEqualityComparer<TSource> comparer)
        {
            int index = 0;
            foreach (TSource item in list)
            {
                if (comparer.Equals(item, value))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static void Move<T>(this IList<T> list, int oldIdx, int newIdx)
        {
            if (list is ObservableCollection<T>)
                ((ObservableCollection<T>) list).Move(oldIdx, newIdx);
            else
            {
                var item = list[oldIdx];
                list.RemoveAt(oldIdx);
                if (newIdx > oldIdx)
                    newIdx -= 1;
                list.Insert(newIdx, item);
            }
        }

    }
}
