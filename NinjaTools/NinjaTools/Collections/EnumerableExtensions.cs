using System;
using System.Collections.Generic;

namespace NinjaTools.Collections
{
    public static class EnumerableExtensions
    {
        public static bool SequenceEqual<T>(this IEnumerable<T> c1, IEnumerable<T> c2, Func<T, T, bool> predicate)
        {
            using (var it1 = c1.GetEnumerator())
            using (var it2 = c2.GetEnumerator())
            {
                while (true)
                {
                    bool has1 = it1.MoveNext();
                    bool has2 = it2.MoveNext();

                    if (!has1 || !has2)
                        return has1 == has2;

                    if (!predicate(it1.Current, it2.Current))
                        return false;
                }
            }
        }

        public static bool SequenceEqual<T>(this IList<T> c1, IList<T> c2, Func<T, T, bool> predicate)
        {
            if (c1.Count != c2.Count) return false;
            return SequenceEqual((IEnumerable<T>)c1, c2, predicate);
        }

    }
}
