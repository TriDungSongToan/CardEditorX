using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CardEditor.Behaviors
{
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        #region Dependency Properties

        public static readonly DependencyProperty OrientationProperty =
            OrientationProperty = DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(
                nameof(ItemWidth),
                typeof(double),
                typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(
                nameof(ItemHeight),
                typeof(double),
                typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        #endregion

        #region Fields

        private IItemContainerGenerator _generator;
        private Size _extent = new Size(0, 0);
        private Size _viewport = new Size(0, 0);
        private Point _offset;
        private ScrollViewer _owner;

        #endregion

        public VirtualizingWrapPanel()
        {
            this.CanHorizontallyScroll = false;
            this.CanVerticallyScroll = false;
            this._generator = this.ItemContainerGenerator;
        }

        #region Measure/Arrange

        protected override Size MeasureOverride(Size availableSize)
        {
            // Don't rely on a cached _generator field; get the current one each measure
            EnsureGenerator(); // optional, keeps _generator in sync if you still use it elsewhere
            var generator = this.ItemContainerGenerator;
            if (generator == null)
                return availableSize;

            UpdateScrollInfo(availableSize);

            var itemsControl = ItemsControl.GetItemsOwner(this);
            if (itemsControl == null) return availableSize;

            var itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;
            if (itemCount == 0)
            {
                ResetScrollInfo();
                return availableSize;
            }

            double itemW = double.IsNaN(ItemWidth) ? 0 : ItemWidth;
            double itemH = double.IsNaN(ItemHeight) ? 0 : ItemHeight;

            bool fixedWidth = !double.IsNaN(itemW);
            bool fixedHeight = !double.IsNaN(itemH);

            if (!fixedWidth || !fixedHeight)
            {
                if (this.InternalChildren.Count > 0)
                {
                    UIElement first = this.InternalChildren[0];
                    first.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    if (!fixedWidth) itemW = Math.Max(0, first.DesiredSize.Width);
                    if (!fixedHeight) itemH = Math.Max(0, first.DesiredSize.Height);
                }
                else
                {
                    if (!fixedWidth) itemW = 100;
                    if (!fixedHeight) itemH = 100;
                }
            }

            if (itemW <= 0) itemW = 1;
            if (itemH <= 0) itemH = 1;

            int itemsPerLine;
            double horizontalAvailable = availableSize.Width;
            double verticalAvailable = availableSize.Height;

            if (Orientation == Orientation.Horizontal)
            {
                itemsPerLine = Math.Max(1, (int)Math.Floor(horizontalAvailable / itemW));
            }
            else
            {
                itemsPerLine = Math.Max(1, (int)Math.Floor(verticalAvailable / itemH));
            }

            int lineCount = (int)Math.Ceiling((double)itemCount / itemsPerLine);

            Size extent;
            if (Orientation == Orientation.Horizontal)
            {
                extent = new Size(itemsPerLine * itemW, lineCount * itemH);
            }
            else
            {
                extent = new Size(lineCount * itemW, itemsPerLine * itemH);
            }

            _extent = extent;
            _viewport = availableSize;
            CoerceOffsets();

            // Calculate visible range of items based on offset and viewport
            int firstVisibleIndex, lastVisibleIndex;
            GetVisibleRange(itemsPerLine, itemW, itemH, out firstVisibleIndex, out lastVisibleIndex);

            // defensive guards
            if (firstVisibleIndex < 0) firstVisibleIndex = 0;
            if (lastVisibleIndex >= itemCount) lastVisibleIndex = itemCount - 1;
            if (firstVisibleIndex > lastVisibleIndex) return availableSize;

            // Realize children in visible range using the current generator
            var startPos = generator.GeneratorPositionFromIndex(firstVisibleIndex);
            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;

            using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                for (int itemIndex = firstVisibleIndex; itemIndex <= lastVisibleIndex; itemIndex++, childIndex++)
                {
                    bool newlyRealized = false;
                    var child = (UIElement)generator.GenerateNext(out newlyRealized);
                    if (child == null)
                        continue;

                    if (newlyRealized)
                    {
                        if (childIndex >= this.Children.Count)
                            base.AddInternalChild(child);
                        else
                            base.InsertInternalChild(childIndex, child);

                        generator.PrepareItemContainer(child);
                    }
                    else
                    {
                        // child already realized - ensure it's in right spot
                        if (child != null && childIndex < this.Children.Count && child != this.Children[childIndex])
                        {
                            // attempt to move it into place
                            try
                            {
                                this.RemoveInternalChildRange(childIndex, 1);
                                base.InsertInternalChild(childIndex, child);
                            }
                            catch
                            {
                                // swallow unexpected errors during reordering - it's safer than crashing
                            }
                        }
                    }

                    // Measure the child with the computed item size
                    child.Measure(new Size(itemW, itemH));
                }
            }

            // Clean up children that are out of range
            CleanUpItems(firstVisibleIndex, lastVisibleIndex, generator);

            return availableSize;
        }
        private void CleanUpItems(int firstVisibleIndex, int lastVisibleIndex, IItemContainerGenerator generator)
        {
            // Use the provided generator (do not rely on cached field)
            if (generator == null) generator = this.ItemContainerGenerator;
            if (generator == null) return;

            for (int i = this.Children.Count - 1; i >= 0; i--)
            {
                var genPos = new GeneratorPosition(i, 0);
                int itemIndex;
                try
                {
                    itemIndex = generator.IndexFromGeneratorPosition(genPos);
                }
                catch
                {
                    // If generator can't map position, remove child defensively
                    _generator?.Remove(genPos, 1);
                    RemoveInternalChildRange(i, 1);
                    continue;
                }

                if (itemIndex < firstVisibleIndex || itemIndex > lastVisibleIndex)
                {
                    generator.Remove(genPos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            var itemsControl = ItemsControl.GetItemsOwner(this);
            if (itemsControl == null) return finalSize;

            var itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;
            if (itemCount == 0) return finalSize;

            double itemW = double.IsNaN(ItemWidth) ? 0 : ItemWidth;
            double itemH = double.IsNaN(ItemHeight) ? 0 : ItemHeight;

            if (itemW <= 0 || itemH <= 0)
            {
                // fall back if not set
                if (this.InternalChildren.Count > 0)
                {
                    var d = this.InternalChildren[0].DesiredSize;
                    if (itemW <= 0) itemW = d.Width;
                    if (itemH <= 0) itemH = d.Height;
                }
                else
                {
                    itemW = 100; itemH = 100;
                }
            }

            int itemsPerLine;
            if (Orientation == Orientation.Horizontal)
                itemsPerLine = Math.Max(1, (int)Math.Floor(finalSize.Width / itemW));
            else
                itemsPerLine = Math.Max(1, (int)Math.Floor(finalSize.Height / itemH));

            if (itemsPerLine < 1) itemsPerLine = 1;

            var generator = this._generator;
            int firstVisibleIndex, lastVisibleIndex;
            GetVisibleRange(itemsPerLine, itemW, itemH, out firstVisibleIndex, out lastVisibleIndex);

            int childIndex = 0;
            for (int itemIndex = firstVisibleIndex; itemIndex <= lastVisibleIndex; itemIndex++, childIndex++)
            {
                UIElement child = this.Children[childIndex];
                int row, col;
                if (Orientation == Orientation.Horizontal)
                {
                    int indexInLine = itemIndex % itemsPerLine;
                    row = (itemIndex / itemsPerLine);
                    col = indexInLine;
                }
                else
                {
                    int indexInLine = itemIndex % itemsPerLine;
                    row = indexInLine;
                    col = (itemIndex / itemsPerLine);
                }

                double x = col * itemW - _offset.X;
                double y = row * itemH - _offset.Y;
                Rect rect = new Rect(new Point(x, y), new Size(itemW, itemH));
                child.Arrange(rect);
            }

            return finalSize;
        }

        private void GetVisibleRange(int itemsPerLine, double itemW, double itemH, out int firstIndex, out int lastIndex)
        {
            var itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;

            int firstLine = Orientation == Orientation.Horizontal
                ? Math.Max(0, (int)Math.Floor(_offset.Y / itemH))
                : Math.Max(0, (int)Math.Floor(_offset.X / itemW));

            int visibleLines = Orientation == Orientation.Horizontal
                ? (int)Math.Ceiling(_viewport.Height / itemH) + 1
                : (int)Math.Ceiling(_viewport.Width / itemW) + 1;

            int first = firstLine * itemsPerLine;
            int last = Math.Min(itemCount - 1, (firstLine + visibleLines) * itemsPerLine + (itemsPerLine - 1));

            firstIndex = Math.Max(0, first);
            lastIndex = Math.Max(0, last);
        }

        private void CleanUpItems(int firstVisibleIndex, int lastVisibleIndex)
        {
            for (int i = this.Children.Count - 1; i >= 0; i--)
            {
                GeneratorPosition pos = new GeneratorPosition(i, 0);
                int itemIndex = this._generator.IndexFromGeneratorPosition(pos);
                if (itemIndex < firstVisibleIndex || itemIndex > lastVisibleIndex)
                {
                    _generator.Remove(pos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        private void EnsureGenerator()
        {
            if (this._generator == null)
                this._generator = this.ItemContainerGenerator;
        }

        private void ResetScrollInfo()
        {
            _extent = new Size(0, 0);
            _viewport = new Size(0, 0);
            _offset = new Point(0, 0);
            if (_owner != null) _owner.InvalidateScrollInfo();
        }

        private void UpdateScrollInfo(Size availableSize)
        {
            if (_viewport != availableSize)
            {
                _viewport = availableSize;
                if (_owner != null) _owner.InvalidateScrollInfo();
            }
        }

        private void CoerceOffsets()
        {
            if (_offset.X + _viewport.Width > _extent.Width)
                _offset.X = Math.Max(0, _extent.Width - _viewport.Width);
            if (_offset.Y + _viewport.Height > _extent.Height)
                _offset.Y = Math.Max(0, _extent.Height - _viewport.Height);

            if (_offset.X < 0) _offset.X = 0;
            if (_offset.Y < 0) _offset.Y = 0;
        }

        #endregion

        #region IScrollInfo Implementation

        public bool CanHorizontallyScroll { get; set; }
        public bool CanVerticallyScroll { get; set; }

        public double ExtentHeight => _extent.Height;
        public double ExtentWidth => _extent.Width;
        public double HorizontalOffset => _offset.X;
        public double VerticalOffset => _offset.Y;
        public double ViewportHeight => _viewport.Height;
        public double ViewportWidth => _viewport.Width;
        public ScrollViewer ScrollOwner
        {
            get => _owner;
            set => _owner = value;
        }

        public void LineDown() => SetVerticalOffset(VerticalOffset + 16);
        public void LineUp() => SetVerticalOffset(VerticalOffset - 16);
        public void LineLeft() => SetHorizontalOffset(HorizontalOffset - 16);
        public void LineRight() => SetHorizontalOffset(HorizontalOffset + 16);
        public void MouseWheelDown() => SetVerticalOffset(VerticalOffset + SystemParameters.WheelScrollLines * 16);
        public void MouseWheelUp() => SetVerticalOffset(VerticalOffset - SystemParameters.WheelScrollLines * 16);
        public void MouseWheelLeft() => SetHorizontalOffset(HorizontalOffset - SystemParameters.WheelScrollLines * 16);
        public void MouseWheelRight() => SetHorizontalOffset(HorizontalOffset + SystemParameters.WheelScrollLines * 16);
        public void PageDown() => SetVerticalOffset(VerticalOffset + ViewportHeight);
        public void PageUp() => SetVerticalOffset(VerticalOffset - ViewportHeight);
        public void PageLeft() => SetHorizontalOffset(HorizontalOffset - ViewportWidth);
        public void PageRight() => SetHorizontalOffset(HorizontalOffset + ViewportWidth);

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var gen = this.ItemContainerGenerator as IItemContainerGenerator;
            int index = gen.IndexFromGeneratorPosition(new GeneratorPosition(this.Children.IndexOf((UIElement)visual), 0));
            if (index < 0) return rectangle;

            double itemW = double.IsNaN(ItemWidth) ? rectangle.Width : ItemWidth;
            double itemH = double.IsNaN(ItemHeight) ? rectangle.Height : ItemHeight;

            int itemsPerLine = Math.Max(1, (int)Math.Floor(ViewportWidth / itemW));

            int row = index / itemsPerLine;
            int col = index % itemsPerLine;

            double x = col * itemW;
            double y = row * itemH;

            if (x < HorizontalOffset) SetHorizontalOffset(x);
            else if (x + itemW > HorizontalOffset + ViewportWidth) SetHorizontalOffset(x + itemW - ViewportWidth);

            if (y < VerticalOffset) SetVerticalOffset(y);
            else if (y + itemH > VerticalOffset + ViewportHeight) SetVerticalOffset(y + itemH - ViewportHeight);

            rectangle.Offset(-HorizontalOffset, -VerticalOffset);
            return rectangle;
        }

        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || ViewportWidth >= ExtentWidth) offset = 0;
            else if (offset + ViewportWidth >= ExtentWidth) offset = ExtentWidth - ViewportWidth;

            _offset.X = offset;
            InvalidateMeasure();
            _owner?.InvalidateScrollInfo();
        }

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || ViewportHeight >= ExtentHeight) offset = 0;
            else if (offset + ViewportHeight >= ExtentHeight) offset = ExtentHeight - ViewportHeight;

            _offset.Y = offset;
            InvalidateMeasure();
            _owner?.InvalidateScrollInfo();
        }

        #endregion
    }
}
