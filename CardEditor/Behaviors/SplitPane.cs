using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace CardEditor.Behaviors
{
    public static class SplitPaneBehavior
    {
        // Attached Property: RowDefinition mà chúng ta muốn điều khiển
        public static readonly DependencyProperty TargetRowProperty =
            DependencyProperty.RegisterAttached("TargetRow", typeof(RowDefinition), typeof(SplitPaneBehavior), new PropertyMetadata(null, OnTargetRowChanged));

        public static void SetTargetRow(DependencyObject element, RowDefinition value) => element.SetValue(TargetRowProperty, value);
        public static RowDefinition GetTargetRow(DependencyObject element) => (RowDefinition)element.GetValue(TargetRowProperty);

        // Thumb kéo xuống
        public static readonly DependencyProperty PullThumbProperty =
            DependencyProperty.RegisterAttached("PullThumb", typeof(Thumb), typeof(SplitPaneBehavior), new PropertyMetadata(null));

        public static void SetPullThumb(DependencyObject element, Thumb value) => element.SetValue(PullThumbProperty, value);
        public static Thumb GetPullThumb(DependencyObject element) => (Thumb)element.GetValue(PullThumbProperty);

        private static void OnTargetRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is RowDefinition oldRow)
            {
                var desc = DependencyPropertyDescriptor.FromProperty(FrameworkElement.ActualHeightProperty, typeof(RowDefinition));
                desc?.RemoveValueChanged(oldRow, OnRowHeightChanged);
            }

            if (e.NewValue is RowDefinition newRow)
            {
                var desc = DependencyPropertyDescriptor.FromProperty(FrameworkElement.ActualHeightProperty, typeof(RowDefinition));
                desc?.AddValueChanged(newRow, OnRowHeightChanged);

                // Cập nhật ngay lập tức
                UpdateThumbVisibility(d, newRow);
            }
        }

        private static void OnRowHeightChanged(object sender, EventArgs e)
        {
            if (sender is RowDefinition row)
            {
                // Tìm control nào đang gắn vào row này (GridSplitter hoặc Thumb)
                var parent = LogicalTreeHelper.GetParent(row) as Grid;
                if (parent == null) return;

                foreach (var child in parent.Children)
                {
                    if (child is UIElement ui && GetTargetRow(ui) == row)
                    {
                        UpdateThumbVisibility(ui, row);
                    }
                }
            }
        }

        private static void UpdateThumbVisibility(DependencyObject control, RowDefinition topRow)
        {
            var thumb = GetPullThumb(control) ??
                        (control is Thumb t ? t : null);

            if (thumb == null) return;

            bool isCollapsed = topRow.ActualHeight < 10.0;

            thumb.Visibility = isCollapsed ? Visibility.Visible : Visibility.Collapsed;

            if (isCollapsed)
                Canvas.SetTop(thumb, 0);
        }

        // ==== GridSplitter Behavior ====
        public static readonly DependencyProperty EnableSplitProperty =
            DependencyProperty.RegisterAttached("EnableSplit", typeof(bool), typeof(SplitPaneBehavior), new PropertyMetadata(false, OnEnableSplitChanged));

        public static void SetEnableSplit(DependencyObject element, bool value) => element.SetValue(EnableSplitProperty, value);
        public static bool GetEnableSplit(DependencyObject element) => (bool)element.GetValue(EnableSplitProperty);

        private static void OnEnableSplitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not GridSplitter splitter || !(bool)e.NewValue) return;

            var row = GetTargetRow(splitter);
            if (row == null) return;

            splitter.DragStarted += (s, _) =>
            {
                var thumb = GetPullThumb(splitter);
                if (thumb != null) thumb.Visibility = Visibility.Collapsed;
            };

            splitter.DragDelta += (s, args) =>
            {
                double delta = args.VerticalChange;
                double newHeight = row.ActualHeight + delta;
                row.Height = new GridLength(Math.Max(0, newHeight));
            };

            splitter.DragCompleted += (s, _) =>
            {
                if (row.ActualHeight < 10.0)
                {
                    row.Height = new GridLength(0);
                }
                UpdateThumbVisibility(splitter, row);
            };
        }

        // ==== Thumb Behavior ====
        public static readonly DependencyProperty EnablePullProperty =
            DependencyProperty.RegisterAttached("EnablePull", typeof(bool), typeof(SplitPaneBehavior), new PropertyMetadata(false, OnEnablePullChanged));

        public static void SetEnablePull(DependencyObject element, bool value) => element.SetValue(EnablePullProperty, value);
        public static bool GetEnablePull(DependencyObject element) => (bool)element.GetValue(EnablePullProperty);

        private static void OnEnablePullChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Thumb thumb || !(bool)e.NewValue) return;

            var row = GetTargetRow(thumb);
            if (row == null) return;

            thumb.DragDelta += (s, args) =>
            {
                double newTop = Canvas.GetTop(thumb) + args.VerticalChange;
                newTop = Math.Max(0, newTop);
                Canvas.SetTop(thumb, newTop);

                // Đồng bộ chiều cao pane
                double desiredHeight = newTop + thumb.ActualHeight / 2;
                row.Height = new GridLength(desiredHeight);
            };

            thumb.DragCompleted += (s, _) =>
            {
                if (row.ActualHeight < 10.0)
                {
                    row.Height = new GridLength(0);
                }
                Canvas.SetTop(thumb, 0);
                UpdateThumbVisibility(thumb, row);
            };
        }
    }
}
