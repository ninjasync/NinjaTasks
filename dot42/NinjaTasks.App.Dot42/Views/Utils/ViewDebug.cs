using System.Collections.Generic;
using System.Diagnostics;
using Android.Views;

namespace NinjaTasks.App.Droid.Views.Utils
{
    public static class ViewDebug
    {
        public static IEnumerable<View> GetChildren(this ViewGroup view)
        {
            for (int i = 0; i < view.ChildCount; ++i)
            {
                var child = view.GetChildAt(i);
                yield return child;
            }
        }

        public static IEnumerable<View> GetDescendants(this ViewGroup view)
        {
            for (int i = 0; i < view.ChildCount; ++i)
            {
                var child = view.GetChildAt(i);
                yield return child;

                if(child is ViewGroup)
                    foreach (var c in ((ViewGroup) child).GetDescendants())
                        yield return c;
            }
        }

        public static void DumpHierachy(this ViewGroup view, string level = "")
        {
            DumpView(view, level);

            level += "  ";

            for (int i = 0; i < view.ChildCount; ++i)
            {
                var child = view.GetChildAt(i);

                if (child is ViewGroup)
                    ((ViewGroup) child).DumpHierachy(level);
                else
                    DumpView(child, level);
            }
        }

        private static void DumpView(View view, string level)
        {
            Debug.WriteLine("{0}id={1:X} {2}", level, view.Id, view.GetType().Name);
        }
    }
}
