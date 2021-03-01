using System.Collections.Generic;
using Android.Views;

namespace NinjaTools.Droid.Extensions
{
    public static class ViewExtensions
    {
        public static IEnumerable<View> SelfAndDescendants(this View eView)
        {
            yield return eView;
            if (!(eView is ViewGroup g)) yield break;
            // recursive foreach not nessesary very performant
            foreach (var descendant in Descendants(eView))
                yield return descendant;
        }

        public static IEnumerable<View> Descendants(this View eView)
        {
            if (!(eView is ViewGroup g)) yield break;
            int cc = g.ChildCount;
            for (int i = 0; i < cc; ++i)
            {
                var child = g.GetChildAt(i);
                yield return child;
                // recursive foreach not nessesary very performant
                foreach (var descendant in Descendants(child))
                    yield return descendant;
            }
        }
    }
}