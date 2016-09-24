using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using NinjaTools.Collections;
using NinjaTools.GUI.Wpf.Utils;

namespace NinjaTools.GUI.Wpf.Behaviors
{
    // based upon but extended from  http://stackoverflow.com/questions/6503972/excel-like-drag-selection-in-wpf-itemscontrol
    // exhibits (minor) visual problems when freezed columns are used in datagrid.
    public sealed partial class ItemsControlBehavior : DependencyObject
    {
        private static readonly Dictionary<Selector, AdornderBase> Dictionary = new Dictionary<Selector, AdornderBase>();
        public static readonly DependencyProperty ExcelSelectionAdornerProperty = DependencyProperty.RegisterAttached("ExcelSelectionAdorner", typeof(bool), typeof(ItemsControlBehavior),
            new PropertyMetadata(OnUseAdornerChanged));

        public static bool GetExcelSelectionAdorner(Selector control) { return (bool)control.GetValue(ExcelSelectionAdornerProperty); }
        public static void SetExcelSelectionAdorner(Selector control, bool useAdorner) { control.SetValue(ExcelSelectionAdornerProperty, useAdorner); }

        private static void AttachToScrollViewer(Selector selector)
        {
            ScrollViewer viewer = GetScrollViewer(selector);
            if (viewer != null)
            {
                viewer.Tag = selector;
                viewer.ScrollChanged += Viewer_ScrollChanged;
            }
        }

        private static void DetachAdorner(Selector selector)
        {
            AdornderBase adorner;
            if (Dictionary.TryGetValue(selector, out adorner))
            {
                adorner.Clear();
                Dictionary.Remove(selector);
            }
        }
        private static AdornderBase GetAdorner(Selector selector)
        {
            AdornderBase adorner;
            if (!Dictionary.TryGetValue(selector, out adorner))
            {
                Dictionary.Add(selector, adorner = CreateAdorner(selector));
            }
            return adorner;
        }

        private static AdornderBase CreateAdorner(Selector selector)
        {
            if(selector is DataGrid)
                return new DataGridHandleAdorner(selector);
            else
                return new DefaultAdorder(selector);
        }

        private static Rect GetBounds(Selector selector, UIElement containerElement)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(containerElement);
            return new Rect(containerElement.TranslatePoint(bounds.TopLeft, selector), bounds.Size);
        }
        private static ScrollViewer GetScrollViewer(DependencyObject d)
        {
            List<DependencyObject> list = new List<DependencyObject>();
            foreach (DependencyObject child in Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(d)).Select(index => VisualTreeHelper.GetChild(d, index)))
            {
                ScrollViewer viewer = child as ScrollViewer;
                if (viewer != null)
                {
                    return viewer;
                }
                list.Add(child);
            }
            return list.Select(GetScrollViewer).FirstOrDefault(viewer => viewer != null);
        }

        private static void OnUseAdornerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Selector selector = d as Selector;
            if (selector != null)
            {
                if (((bool)e.NewValue))
                {
                    selector.SelectionChanged += Selector_SelectionChanged;

                    if (selector is DataGrid)
                    {
                        ((DataGrid) selector).SelectedCellsChanged += DataGrid_SelectedCellsChanged;
                    }

                    if (!selector.IsLoaded)
                    {
                        selector.Loaded += Selector_Loaded;
                    }
                    else
                    {
                        AttachToScrollViewer(selector);
                    }

                    ProcessSelection(selector);
                }
                else
                {
                    selector.SelectionChanged -= Selector_SelectionChanged;

                    if (selector is DataGrid)
                    {
                        ((DataGrid)selector).SelectedCellsChanged -= DataGrid_SelectedCellsChanged;
                    }

                    if (!selector.IsLoaded)
                    {
                        selector.Loaded -= Selector_Loaded;
                    }
                    else
                    {
                        ScrollViewer viewer = GetScrollViewer(selector);
                        if (viewer != null)
                        {
                            viewer.ScrollChanged -= Viewer_ScrollChanged;
                        }
                    }
                    DetachAdorner(selector);
                }
            }
        }

        private static void ProcessSelection(Selector selector)
        {
            DataGrid dataGrid = selector as DataGrid;
            ListBox listBox = selector as ListBox;

            if ((dataGrid != null && dataGrid.SelectionMode == DataGridSelectionMode.Single && dataGrid.SelectionUnit == DataGridSelectionUnit.FullRow)
              ||(listBox != null && listBox.SelectionMode == SelectionMode.Single)
              ||(listBox == null && dataGrid == null))
            {
                if (selector.SelectedItem != null)
                {
                    ProcessRowSelection(selector, new[] {selector.SelectedItem});
                    return;
                }
            }
            else if (listBox != null)
            {
                if (listBox.SelectedItems.Count != 0)
                {
                    object[] selectedItems = new object[listBox.SelectedItems.Count];
                    listBox.SelectedItems.CopyTo(selectedItems, 0);
                    ProcessRowSelection(selector, selectedItems);
                    return;
                }
            }
            else // (dataGrid != null)
            {
                if (dataGrid.SelectedCells.Count != 0)
                {
                    if (ProcessDataGridSelection(selector, dataGrid))
                        return;
                }
            }
            DetachAdorner(selector);
        }

        private static bool ProcessDataGridSelection(Selector selector, DataGrid dataGrid)
        {
            // get row and columns; for virtualized rows/columns we will get no visual info.
            var rowByItem = dataGrid.SelectedCells
                               .Select(c => c.Item)
                               .Distinct()
                               .Select(item => new {Item = item, Container = selector.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow})
                               .Where(sel => sel.Container != null)
                               .Select(c => new {c.Item, c.Container, RowIndex = selector.ItemContainerGenerator.IndexFromContainer(c.Container)})
                               .OrderBy(c => c.RowIndex)
                               .ToLookup(k => k.Item);
            var cells = dataGrid.SelectedCells
                                .Select(c => new {ColumnIndex = c.Column.DisplayIndex, Row = rowByItem[c.Item].FirstOrDefault() })
                                .Where(c=>c.Row != null)
                                .OrderBy(c => c.Row.RowIndex)
                                .ThenBy(c => c.ColumnIndex)
                                .ToList();
            var columns = cells.Select(c => c.ColumnIndex).Distinct().OrderBy(c => c).ToList();

            if (cells.Count == 0 || columns.Count* rowByItem.Count != cells.Count)
                return false;
            if (rowByItem.Last().First().RowIndex - rowByItem.First().First().RowIndex + 1 != rowByItem.Count)
                return false;
            if (columns.Last() - columns.First() + 1 != columns.Count)
                return false;

            // TODO: take frozen columns into account.

            // we do have a contingous selection.
            // get first/last cell container that has been actually generated.
            var firstElt = cells.Select(c=> dataGrid.ColumnFromDisplayIndex(c.ColumnIndex).GetCellContent(c.Row.Container)).FirstOrDefault(c=> (c?.Parent as DataGridCell) != null);
            var lastElt  = cells.FastReverse().Select(c => dataGrid.ColumnFromDisplayIndex(c.ColumnIndex).GetCellContent(c.Row.Container)).FirstOrDefault(c => (c?.Parent as DataGridCell) != null);
            if (firstElt != null && lastElt != null)
            {
                var firstcell = (DataGridCell)firstElt.Parent;
                var lastCell = (DataGridCell)lastElt.Parent;
                Point topLeft, bottomRight;

                try
                {
                    topLeft = dataGrid.PointFromScreen(firstcell.PointToScreen(new Point(0, 0)));
                    bottomRight = dataGrid.PointFromScreen(lastCell.PointToScreen(new Point(lastCell.ActualWidth, lastCell.ActualHeight)));
                }
                catch
                {
                    /*TODO: find out why an exception can occur at all.*/
                    return false;
                }

                if (topLeft.X < bottomRight.X && topLeft.Y < bottomRight.Y)
                {
                    var bounds = new Rect(topLeft, bottomRight);
                    GetAdorner(selector).Update(bounds);
                    return true;
                }
            }

            return false;
        }

        private static void ProcessRowSelection(Selector selector, IEnumerable<object> selectedItems)
        {
            List<DependencyObject> containers = new List<DependencyObject>();
            List<int> indices = new List<int>();
            foreach (DependencyObject container in selectedItems
                                        .Select(selectedItem => selector.ItemContainerGenerator.ContainerFromItem(selectedItem))
                                        .Where(container => container != null))
            {
                int containerIndex = selector.ItemContainerGenerator.IndexFromContainer(container);
                int index = indices.BinarySearch(containerIndex);
                containers.Insert(~index, container);
                indices.Insert(~index, containerIndex);
            }
            for (int i = 1; i < indices.Count; i++)
            {
                if (indices[i] != (indices[i - 1] + 1))
                {
                    // Not contiguous
                    DetachAdorner(selector);
                    return;
                }
            }
            Rect bounds = GetBounds(selector, (UIElement)containers[0]);
            for (int i = 1; i < containers.Count; i++)
            {
                bounds.Union(GetBounds(selector, (UIElement)containers[i]));
            }
            GetAdorner(selector).Update(bounds);
        }

        private static IEnumerable<DependencyObject> GetContainers(Selector selector, IEnumerable<object> selectedItems)
        {
            return selectedItems
                .Select(selectedItem => selector.ItemContainerGenerator.ContainerFromItem(selectedItem))
                .Where(container => container != null);
        }

        static void Selector_Loaded(object sender, RoutedEventArgs e)
        {
            var selector = (Selector)sender;
            AttachToScrollViewer(selector);
            selector.Loaded -= Selector_Loaded;
        }

        private static void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessSelection((Selector)sender);
        }

        private static void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            ProcessSelection((Selector)sender);
        }

        static void Viewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ProcessSelection((Selector)((FrameworkElement)sender).Tag);
        }

    }

    internal class AdornderBase : Adorner
    {
        private readonly UIElement _originalElement;
        private AdornerLayer _adornerLayer;
        protected Rect Bounds { get; private set; }

        public AdornderBase(UIElement adornedElement) : base(GetRealAdornedElement(adornedElement))
        {
            _originalElement = adornedElement;
        }

        private static UIElement GetRealAdornedElement(UIElement adornedElement)
        {
            if (adornedElement is DataGrid)
            {
                var cp = adornedElement.GetVisualDescendent<ScrollContentPresenter>()
                                       .GetVisualDescendent<UIElement>();
                return cp;
            }
            return adornedElement;
        }

        public virtual void Update(Rect bounds, bool invalidateVisual = true)
        {
            if (_adornerLayer == null)
            {
                if ((_adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement)) != null)
                {
                    _adornerLayer.Add(this);
                }
            }

            if (_originalElement != AdornedElement)
            {
                var tl = _originalElement.TranslatePoint(bounds.TopLeft, AdornedElement);
                var br = _originalElement.TranslatePoint(bounds.BottomRight, AdornedElement);
                bounds = new Rect(tl, br);
            }

            this.Bounds = bounds;

            if (invalidateVisual)
                this.InvalidateVisual();
        }

        public void Clear()
        {
            _adornerLayer?.Remove(this);
            
        }
    }

    internal class DefaultAdorder : AdornderBase
    {
        private static readonly Pen Pen = CreatePen();

        private static Pen CreatePen()
        {
            Pen pen = new Pen(Brushes.Black, 2.0);
            pen.Freeze();
            return pen;
        }

        public DefaultAdorder(UIElement adornedElement) : base(adornedElement)
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(null, Pen, this.Bounds);
        }
    }

    internal class DataGridHandleAdorner : AdornderBase
    {
        const int Size = 5;

        readonly Thumb _thumb;

        readonly VisualCollection _visualChildren;

        Point _mousePosition;

        readonly DataGrid _dataGrid;
        private readonly Pen _pen;

        public override void Update(Rect bounds, bool invalidateVisual = true)
        {
            base.Update(bounds, false);

            UpdateThumbPos(Bounds);
            _mousePosition = Bounds.BottomRight;
            InvalidateVisual();
        }

        public DataGridHandleAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            _dataGrid = adornedElement as DataGrid;

            var blackSolidBrush = new SolidColorBrush(Colors.Black);
            _pen = new Pen(blackSolidBrush, 3);
            _pen.Freeze();
            
            _visualChildren = new VisualCollection(this);
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, Brushes.Black);
            _thumb = new Thumb
            {
                //Cursor = Cursors.Cross,
                Background = new SolidColorBrush(Colors.Black),
                Height = Size,
                Template = new ControlTemplate(typeof(Thumb)) { VisualTree = border },
                Width = Size
            };
            _visualChildren.Add(_thumb);

            //// if(_dataGrid != null)
            //    //_thumb.DragDelta += thumb_DragDelta;
        }

        void UpdateThumbPos(Rect rect)
        {
            if (_thumb == null) return;
            _mousePosition = rect.BottomRight;
            _mousePosition.Offset(-Size / 2 - 1, -Size / 2 - 1);
            _thumb.Arrange(new Rect(_mousePosition, new Size(Size, Size)));
        }

        // doesn't work as expected; should probably just add the selected cells based on a starting cell.
        //void thumb_DragDelta(object sender, DragDeltaEventArgs e)
        //{
        //    _mousePosition.Offset(e.HorizontalChange, e.VerticalChange);
        //    IInputElement inputElt = _dataGrid.InputHitTest(_mousePosition);
        //    var tb = inputElt as TextBlock;
        //    if (tb == null)
        //        return;
        //    Point bottomRight = _dataGrid.PointFromScreen(tb.PointToScreen(new Point(tb.ActualWidth + 1, tb.ActualHeight + 1)));

        //    Update(new Rect(Bounds.TopLeft, bottomRight));
        //}

        protected override int VisualChildrenCount => _visualChildren?.Count ?? 0;

        protected override Visual GetVisualChild(int index)
        {
            return _visualChildren[index];
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            
            double halfPenWidth = _pen.Thickness / 2;

            Rect rangeBorderRect = Bounds;
            rangeBorderRect.Offset(-1, -1);

            GuidelineSet guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(rangeBorderRect.Left + halfPenWidth);
            guidelines.GuidelinesX.Add(rangeBorderRect.Right + halfPenWidth);
            guidelines.GuidelinesY.Add(rangeBorderRect.Top + halfPenWidth);
            guidelines.GuidelinesY.Add(rangeBorderRect.Bottom + halfPenWidth);

            Point p1 = rangeBorderRect.BottomRight;
            p1.Offset(0, -4);
            guidelines.GuidelinesY.Add(p1.Y + halfPenWidth);

            Point p2 = rangeBorderRect.BottomRight;
            p2.Offset(-4, 0);
            guidelines.GuidelinesX.Add(p2.X + halfPenWidth);

            drawingContext.PushGuidelineSet(guidelines);

            var geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(p1, true, false);
                ctx.LineTo(rangeBorderRect.TopRight, true, false);
                ctx.LineTo(rangeBorderRect.TopLeft, true, false);
                ctx.LineTo(rangeBorderRect.BottomLeft, true, false);
                ctx.LineTo(p2, true, false);
            }
            geometry.Freeze();
            drawingContext.DrawGeometry(null, _pen, geometry);

            drawingContext.Pop();
        }
    }
}
