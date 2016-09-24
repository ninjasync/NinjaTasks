using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace NinjaTools.GUI.Wpf.Utils
{
    public static class VisualTreeExtensions
    {
        internal static DependencyObject FindVisualTreeRoot(this DependencyObject d)
        {
            DependencyObject current = d;
            DependencyObject dependencyObject = d;
            for (; current != null; current = LogicalTreeHelper.GetParent(current))
            {
                dependencyObject = current;
                if (current is Visual || current is Visual3D)
                    break;
            }
            return dependencyObject;
        }

        public static T GetVisualAncestor<T>(this DependencyObject d) where T : class
        {
            for (DependencyObject parent = VisualTreeHelper.GetParent(d.FindVisualTreeRoot());
                parent != null;
                parent = VisualTreeHelper.GetParent(parent))
            {
                T obj = parent as T;
                if (obj != null)
                    return obj;
            }
            return default(T);
        }

        public static DependencyObject GetVisualAncestor(this DependencyObject d, Type type)
        {
            for (DependencyObject parent = VisualTreeHelper.GetParent(d.FindVisualTreeRoot());
                parent != null && type != (Type) null;
                parent = VisualTreeHelper.GetParent(parent))
            {
                if (parent.GetType() == type || parent.GetType().IsSubclassOf(type))
                    return parent;
            }
            return null;
        }

        public static DependencyObject GetVisualAncestor(this DependencyObject d, Type type, ItemsControl itemsControl)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(FindVisualTreeRoot(d));
            DependencyObject dependencyObject = null;
            for (;
                parent != null && type != (Type) null && parent != itemsControl;
                parent = VisualTreeHelper.GetParent(parent))
            {
                if (parent.GetType() == type || parent.GetType().IsSubclassOf(type))
                    dependencyObject = parent;
            }
            return dependencyObject;
        }

        public static T GetVisualDescendent<T>(this DependencyObject d) where T : DependencyObject
        {
            return GetVisualDescendents<T>(d).FirstOrDefault();
        }

        public static IEnumerable<T> GetVisualDescendents<T>(this DependencyObject d) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(d);

            for (int n = 0; n < childCount; ++n)
            {
                DependencyObject child = VisualTreeHelper.GetChild(d, n);
                if (child is T)
                    yield return (T) child;
            }

            for (int n = 0; n < childCount; ++n)
            {
                DependencyObject child = VisualTreeHelper.GetChild(d, n);
                foreach (T obj in GetVisualDescendents<T>(child))
                    yield return obj;
            }
        }
    }
}