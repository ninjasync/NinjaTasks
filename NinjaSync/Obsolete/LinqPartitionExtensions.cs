using System;
using System.Collections.Generic;

namespace NinjaSync.Obsolete
{
    public static class LinqPartitionExtensions
    {
        public static IEnumerable<IEnumerable<TElement>> Partition<TElement>
                    (this IEnumerable<TElement> source, Func<TElement, TElement, bool> shouldPartitionInBetween)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    yield break;


                TElement currentKey = iterator.Current;
                List<TElement> currentList = new List<TElement> {iterator.Current};

                while (iterator.MoveNext())
                {
                    TElement element = iterator.Current;

                    bool shouldPartition = shouldPartitionInBetween(currentKey, element);

                    if (shouldPartition)  // yield current list and start a new one
                    {
                        yield return currentList;
                        currentList = new List<TElement> {element};
                    }
                    else
                        currentList.Add(element);

                    currentKey = element;
                }
                yield return currentList;
            }
        }
    }
}