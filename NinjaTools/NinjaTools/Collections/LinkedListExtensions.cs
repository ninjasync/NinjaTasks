using System;
using System.Collections.Generic;

namespace NinjaTools.Collections
{
    public static class LinkedListExtensions
    {
        public static int RemoveWhile<T>(this LinkedList<T> list, Func<T, bool> predicate)
        {
            LinkedListNode<T> p;
            int removed = 0;
            for (p = list.First; p != null && !predicate(p.Value); )
            {
                ++removed;
                var del = p; p = p.Next; list.Remove(del);
            }
            return removed;
        }
    }
}
