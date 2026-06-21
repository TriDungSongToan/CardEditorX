using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;

namespace CardEditor.Controls
{
    public class MiniMapControl : Control
    {
        private Point _clickStartPoint;
        private TextEditor _textEditor;
        private WriteableBitmap _miniMapBitmap;
        private Rect _viewportRect;
        private bool _isDragging;
        private DispatcherTimer _updateTimer;
        private Border _previewBorder;
        private TextBlock _previewText;
        private bool _isPreviewVisible;
        static MiniMapControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MiniMapControl),
                new FrameworkPropertyMetadata(typeof(MiniMapControl)));
        }
        public MiniMapControl()
        {
            this.Width = 120;
            this.ClipToBounds = true;
            this.Cursor = Cursors.Hand;
            // Initialize preview tooltip
            _previewBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(230, 30, 30, 30)),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8),
                CornerRadius = new CornerRadius(4),
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false
            };
            _previewText = new TextBlock
            {
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                MaxWidth = 400,
                TextWrapping = TextWrapping.NoWrap
            };
            _previewBorder.Child = _previewText;
            // Timer for scroll updates
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _updateTimer.Tick += (s, e) => InvalidateVisual();
        }

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register(nameof(BackgroundColor), typeof(Color), typeof(MiniMapControl),
                new FrameworkPropertyMetadata(Color.FromArgb(100, 40, 40, 40),
                    FrameworkPropertyMetadataOptions.AffectsRender, OnColorPropertyChanged));
        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.Register(nameof(ForegroundColor), typeof(Color), typeof(MiniMapControl),
                new FrameworkPropertyMetadata(Color.FromArgb(255, 212, 212, 212),
                    FrameworkPropertyMetadataOptions.AffectsRender, OnColorPropertyChanged));
        public static readonly DependencyProperty SliderColorProperty =
            DependencyProperty.Register(nameof(SliderColor), typeof(Color), typeof(MiniMapControl),
                new FrameworkPropertyMetadata(Color.FromArgb(150, 100, 100, 100),
                    FrameworkPropertyMetadataOptions.AffectsRender, OnColorPropertyChanged));
        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }
        public Color ForegroundColor
        {
            get => (Color)GetValue(ForegroundColorProperty);
            set => SetValue(ForegroundColorProperty, value);
        }
        public Color SliderColor
        {
            get => (Color)GetValue(SliderColorProperty);
            set => SetValue(SliderColorProperty, value);
        }

        public TextEditor TextEditor
        {
            get { return _textEditor; }
            set
            {
                if (_textEditor != null)
                {
                    _textEditor.TextChanged -= OnTextChanged;
                    var scrollViewer = GetScrollViewer(_textEditor);
                    if (scrollViewer != null) scrollViewer.ScrollChanged -= OnScrollChanged;
                    _textEditor.SizeChanged -= OnEditorSizeChanged;
                }
                _textEditor = value;
                if (_textEditor != null)
                {
                    _textEditor.TextChanged += OnTextChanged;
                    // Find ScrollViewer in visual tree
                    _textEditor.Loaded += (s, e) =>
                    {
                        var scrollViewer = GetScrollViewer(_textEditor);
                        if (scrollViewer != null) scrollViewer.ScrollChanged += OnScrollChanged;
                    };
                    _textEditor.SizeChanged += OnEditorSizeChanged;
                    _updateTimer.Start();
                    UpdateMiniMap();
                }
            }
        }
        private ScrollViewer GetScrollViewer(DependencyObject obj)
        {
            if (obj == null) return null;
            if (obj is ScrollViewer scrollViewer) return scrollViewer;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }
        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MiniMapControl control)
            {
                // Khi Background hoặc Foreground color thay đổi, cần regenerate bitmap
                if (e.Property == BackgroundColorProperty || e.Property == ForegroundColorProperty)
                {
                    control.UpdateMiniMap();
                }

                // Update preview colors
                control.UpdatePreviewColors();
            }
        }
        private void OnTextChanged(object sender, EventArgs e)
        {
            UpdateMiniMap();
        }
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            InvalidateVisual();
        }
        private void OnEditorSizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateVisual();
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (_textEditor == null) return;
            // Draw background
            drawingContext.DrawRectangle(new SolidColorBrush(BackgroundColor), null, new Rect(0, 0, ActualWidth, ActualHeight));
            // Draw minimap bitmap
            if (_miniMapBitmap != null) drawingContext.DrawImage(_miniMapBitmap, new Rect(0, 0, ActualWidth, ActualHeight));
            // Draw viewport rectangle
            UpdateViewportRect();
            if (_viewportRect.Height > 0)
            {
                var fillColor = Color.FromArgb((byte)(SliderColor.A * 0.4), SliderColor.R, SliderColor.G, SliderColor.B);
                var brush = new SolidColorBrush(fillColor);
                var borderColor = Color.FromArgb((byte)(SliderColor.A * 1.0), SliderColor.R, SliderColor.G, SliderColor.B);
                var pen = new Pen(new SolidColorBrush(borderColor), 1);

                //var brush = new SolidColorBrush(Color.FromArgb(60, 100, 150, 255));
                //var pen = new Pen(new SolidColorBrush(Color.FromArgb(150, 100, 150, 255)), 1);
                drawingContext.DrawRectangle(brush, pen, _viewportRect);
            }
        }
        private void UpdatePreviewColors()
        {
            if (_previewBorder != null)
            {
                // Sử dụng BackgroundColor cho preview, làm tối hơn một chút
                var previewBg = Color.FromArgb(230,
                    (byte)(BackgroundColor.R * 0.5),
                    (byte)(BackgroundColor.G * 0.5),
                    (byte)(BackgroundColor.B * 0.5));
                _previewBorder.Background = new SolidColorBrush(previewBg);
            }

            if (_previewText != null)
            {
                _previewText.Foreground = new SolidColorBrush(ForegroundColor);
            }
        }
        private void UpdateMiniMap()
        {
            if (_textEditor == null || ActualHeight <= 0) return;
            try
            {
                var document = _textEditor.Document;
                if (document == null) return;
                int lineCount = document.LineCount;
                int width = (int)ActualWidth;
                int height = Math.Max((int)ActualHeight, lineCount);
                _miniMapBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
                _miniMapBitmap.Lock();
                try
                {
                    IntPtr backBuffer = _miniMapBitmap.BackBuffer;
                    int stride = _miniMapBitmap.BackBufferStride;
                    // Background
                    int bgColor = ColorToInt(BackgroundColor);
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int offset = y * stride + x * 4;
                            System.Runtime.InteropServices.Marshal.WriteInt32(backBuffer + offset, bgColor);
                        }
                    }
                    // Draw text representation
                    int fgColor = ColorToInt(ForegroundColor);
                    for (int lineNum = 1; lineNum <= lineCount && lineNum - 1 < height; lineNum++)
                    {
                        var line = document.GetLineByNumber(lineNum);
                        string text = document.GetText(line.Offset, line.Length);
                        int y = lineNum - 1;
                        if (y >= height) break;

                        // Simple representation: draw pixels based on character density
                        string trimmedText = text.TrimStart();
                        int charCount = Math.Min(trimmedText.Length, width);
                        for (int x = 0; x < charCount; x++)
                        {
                            if (x < width && !char.IsWhiteSpace(trimmedText[x]))
                            {
                                int offset = y * stride + x * 4;
                                System.Runtime.InteropServices.Marshal.WriteInt32(backBuffer + offset, fgColor);
                            }
                        }
                    }
                    _miniMapBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                }
                finally
                {
                    _miniMapBitmap.Unlock();
                }
                InvalidateVisual();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MiniMap update error: {ex.Message}");
            }
        }
        private int ColorToInt(Color color)
        {
            // Format: ARGB (Big-endian: 0xAARRGGBB)
            return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
        }
        private void UpdateViewportRect()
        {
            if (_textEditor == null || _miniMapBitmap == null) return;
            var document = _textEditor.Document;
            if (document == null) return;
            double totalLines = document.LineCount;
            if (totalLines == 0) return;

            var textView = _textEditor.TextArea.TextView;
            double lineHeight = textView.DefaultLineHeight;
            double totalHeight = totalLines * lineHeight;  // Approx total pixels

            var scrollViewer = GetScrollViewer(_textEditor);
            double scrollOffset = scrollViewer != null ? scrollViewer.VerticalOffset : 0;  // Pixels
            double viewportHeightPixels = textView.ActualHeight;  // Pixels

            double mapHeight = ActualHeight;

            // Tính rectTop và rectHeight dựa trên pixels ratio
            double rectTop = (scrollOffset / totalHeight) * mapHeight;
            double rectHeight = (viewportHeightPixels / totalHeight) * mapHeight;

            // Clamp để tránh overflow
            rectTop = Math.Max(0, Math.Min(mapHeight - rectHeight, rectTop));
            rectHeight = Math.Max(20, Math.Min(mapHeight - rectTop, rectHeight));  // Min height cho visible

            _viewportRect = new Rect(0, rectTop, ActualWidth, rectHeight);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point currentPos = e.GetPosition(this);

            // Nếu đã di chuyển quá 4px → coi là đang drag
            if (!_isDragging && Math.Abs(currentPos.X - _clickStartPoint.X) > 4 || Math.Abs(currentPos.Y - _clickStartPoint.Y) > 4)
            {
                _isDragging = true;
            }

            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                ScrollToMiniMapPosition(currentPos);
            }
            else if (!_isDragging)
            {
                // Vẫn đang hover → show preview bình thường
                ShowPreviewAtPosition(currentPos);
            }
            e.Handled = true;
        }
        private void ShowPreviewAtPosition(Point position)
        {
            if (_textEditor == null || _textEditor.Document == null) return;
            try
            {
                double totalLines = _textEditor.Document.LineCount;
                double mapHeight = ActualHeight;
                int lineNumber = (int)((position.Y / mapHeight) * totalLines) + 1;
                lineNumber = Math.Max(1, Math.Min(lineNumber, (int)totalLines));
                // Get 3 lines of context
                int startLine = Math.Max(1, lineNumber - 1);
                int endLine = Math.Min((int)totalLines, lineNumber + 1);
                string previewContent = "";
                for (int i = startLine; i <= endLine; i++)
                {
                    var line = _textEditor.Document.GetLineByNumber(i);
                    string lineText = _textEditor.Document.GetText(line.Offset, line.Length);
                    previewContent += $"{i}: {lineText}\n";
                }
                _previewText.Text = previewContent.TrimEnd();
                _previewBorder.Visibility = Visibility.Visible;
                _isPreviewVisible = true;
                // Position preview to the left of minimap
                Canvas.SetLeft(_previewBorder, -_previewBorder.ActualWidth - 10);
                Canvas.SetTop(_previewBorder, position.Y - 30);
            }
            catch { }
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.LeftButton != MouseButtonState.Pressed) return;

            _isDragging = false;
            _clickStartPoint = e.GetPosition(this);

            this.CaptureMouse();

            // ẨN preview khi thao tác
            _previewBorder.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            Point releasePos = e.GetPosition(this);

            if (!_isDragging)
            {
                // Đây là click thật sự → jump ngay đến vị trí thả chuột
                ScrollToMiniMapPosition(releasePos);
            }

            _isDragging = false;
            this.ReleaseMouseCapture();
            e.Handled = true;
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _previewBorder.Visibility = Visibility.Collapsed;
            _isPreviewVisible = false;
        }

        private void ScrollToMiniMapPosition(Point positionOnMiniMap)
        {
            var scrollViewer = GetScrollViewer(_textEditor);
            if (scrollViewer == null || scrollViewer.ExtentHeight <= 0) return;

            double ratio = Clamp(positionOnMiniMap.Y / ActualHeight, 0.0, 1.0);

            // Tính offset sao cho vị trí click nằm giữa viewport (chuẩn IDE)
            double targetOffset = ratio * scrollViewer.ExtentHeight;
            double centeredOffset = targetOffset - scrollViewer.ViewportHeight / 2;

            centeredOffset = Math.Max(0,
                Math.Min(centeredOffset, scrollViewer.ExtentHeight - scrollViewer.ViewportHeight));

            _textEditor.ScrollToVerticalOffset(centeredOffset);
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public Border PreviewBorder => _previewBorder;
    }
}