using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NinjaTasks.Core.Reusable
{
    /// <summary>
    /// This class updates SortablePosition after a move. 
    /// We try to modify as few elements as possible. 
    /// <para>
    /// We also try to avoid negative values by starting with a 
    /// large initial sort position.
    /// </para>
    /// </summary>
    [SuppressMessage("dot42", "StaticFieldInGenericType")]
    public class SortableElementIdCalculator<TElementType> where TElementType : class, ISortableElement
    {
        public readonly int DefaultSpacing = 1000;

        public void UpdateAfterMove(IList<TElementType> list, IList<TElementType> moved, int newStartIndex)
        {
            if (list.Count == 1 || moved.Count == 0)
                return; // nothing to do...

            bool isEndOfList = newStartIndex + moved.Count >= list.Count;
            bool isBeginningOfList = newStartIndex == 0;

            int previousPos, nextPos;

            if (isBeginningOfList && isEndOfList)
                previousPos = 0;
            else if (!isBeginningOfList)
                previousPos = list[newStartIndex - 1].SortPosition;
            else // don't try too hard to avoid negative values, since sorting typically
                 // goes to the top.
                previousPos = list.Min(p => p.SortPosition) - DefaultSpacing * (moved.Count +1);

            if (isBeginningOfList && isEndOfList)
                nextPos = (moved.Count + 1) * DefaultSpacing;
            else if (!isEndOfList)
                nextPos = list[newStartIndex + moved.Count].SortPosition;
            else // we are the new last element.
                nextPos = list.Max(p => p.SortPosition) + DefaultSpacing * (moved.Count + 1);

            if (previousPos >= nextPos - moved.Count)
            {
                // unable to fit the new items. try again with the adjourning elements as well.
                moved = moved.ToList(); // make copy
                
                if (!isBeginningOfList)
                {
                    moved.Insert(0, list[newStartIndex - 1]);
                    --newStartIndex;
                }
                if (!isEndOfList)
                    moved.Add(list[newStartIndex + moved.Count]);

                // call recursivly [endrekursiv...]
                UpdateAfterMove(list, moved, newStartIndex);
                return;
            }
            
            // check if already in order.
            if (IsInOrder(previousPos, moved, nextPos))
                return;

            // spread out all values.
            float spread = (float)(nextPos - previousPos) / (moved.Count + 1);
            float sortPos = previousPos + spread;
            Debug.Assert(spread > 0);

            for (int i = 0; i < moved.Count; i++)
            {
                moved[i].SortPosition = (int)sortPos;
                sortPos += spread;
            }
        }

        private bool IsInOrder(int previousPos, IList<TElementType> moved, int nextPos)
        {
            if (previousPos >= moved[0].SortPosition) 
                return false;

            for(int i = 0; i < moved.Count -1; ++i)
                if (moved[i].SortPosition >= moved[i + 1].SortPosition) 
                    return false;

            if (moved[moved.Count - 1].SortPosition >= nextPos) 
                return false;

            return true;
        }
    }
}