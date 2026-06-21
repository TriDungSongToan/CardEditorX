using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CardEditor.Controls
{
    public class TwoStateToggle : ToggleButton
    {
        static TwoStateToggle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TwoStateToggle),
                new FrameworkPropertyMetadata(typeof(TwoStateToggle)));

            IsThreeStateProperty.OverrideMetadata(
                typeof(TwoStateToggle),
                new FrameworkPropertyMetadata(false));
        }

        public TwoStateToggle() { }

        #region Display
        public Brush FalseBackgroundBrush
        {
            get => (Brush)GetValue(FalseBackgroundBrushProperty);
            set => SetValue(FalseBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty FalseBackgroundBrushProperty =
            DependencyProperty.Register("FalseBackgroundBrush", typeof(Brush), typeof(TwoStateToggle), new PropertyMetadata(Brushes.Red));
        public Brush TrueBackgroundBrush
        {
            get => (Brush)GetValue(TrueBackgroundBrushProperty);
            set => SetValue(TrueBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty TrueBackgroundBrushProperty =
            DependencyProperty.Register("TrueBackgroundBrush", typeof(Brush), typeof(TwoStateToggle), new PropertyMetadata(Brushes.Red));

        public Brush ThumbBackgroundBrush
        {
            get => (Brush)GetValue(ThumbBackgroundBrushProperty);
            set => SetValue(ThumbBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty ThumbBackgroundBrushProperty =
            DependencyProperty.Register("ThumbBackgroundBrush", typeof(Brush), typeof(TwoStateToggle), new PropertyMetadata(Brushes.White));
        public Brush ThumbBackgroundHoverBrush
        {
            get => (Brush)GetValue(ThumbBackgroundHoverBrushProperty);
            set => SetValue(ThumbBackgroundHoverBrushProperty, value);
        }
        public static readonly DependencyProperty ThumbBackgroundHoverBrushProperty =
            DependencyProperty.Register("ThumbBackgroundHoverBrush", typeof(Brush), typeof(TwoStateToggle), new PropertyMetadata(Brushes.White));
        public Brush ThumbBorderBrush
        {
            get => (Brush)GetValue(ThumbBorderBrushProperty);
            set => SetValue(ThumbBorderBrushProperty, value);
        }
        public static readonly DependencyProperty ThumbBorderBrushProperty =
            DependencyProperty.Register("ThumbBorderBrush", typeof(Brush), typeof(TwoStateToggle), new PropertyMetadata(Brushes.Transparent));
        public double ThumbBorderThickness
        {
            get => (double)GetValue(ThumbBorderThicknessProperty);
            set => SetValue(ThumbBorderThicknessProperty, value);
        }
        public static readonly DependencyProperty ThumbBorderThicknessProperty =
            DependencyProperty.Register("ThumbBorderThickness", typeof(double), typeof(TwoStateToggle), new PropertyMetadata(1.0));

        public Brush TrackBorderBrush
        {
            get => (Brush)GetValue(TrackBorderBrushProperty);
            set => SetValue(TrackBorderBrushProperty, value);
        }
        public static readonly DependencyProperty TrackBorderBrushProperty =
            DependencyProperty.Register("TrackBorderBrush", typeof(Brush), typeof(TwoStateToggle), new PropertyMetadata(Brushes.Transparent));
        public Thickness TrackBorderThickness
        {
            get => (Thickness)GetValue(TrackBorderThicknessProperty);
            set => SetValue(TrackBorderThicknessProperty, value);
        }
        public static readonly DependencyProperty TrackBorderThicknessProperty =
            DependencyProperty.Register("TrackBorderThickness", typeof(Thickness), typeof(TwoStateToggle),
                new PropertyMetadata(new Thickness(0)));
        public Thickness TrackPadding
        {
            get => (Thickness)GetValue(TrackPaddingProperty);
            set => SetValue(TrackPaddingProperty, value);
        }
        public static readonly DependencyProperty TrackPaddingProperty =
            DependencyProperty.Register("TrackPadding", typeof(Thickness), typeof(TwoStateToggle), new PropertyMetadata(new Thickness(0)));
        public CornerRadius TrackCornerRadius
        {
            get => (CornerRadius)GetValue(TrackCornerRadiusProperty);
            set => SetValue(TrackCornerRadiusProperty, value);
        }
        public static readonly DependencyProperty TrackCornerRadiusProperty =
            DependencyProperty.Register("TrackCornerRadius", typeof(CornerRadius), typeof(TwoStateToggle),
                new PropertyMetadata(new CornerRadius(0)));
        public CornerRadius TrackFalseCornerRadius
        {
            get => (CornerRadius)GetValue(TrackFalseCornerRadiusProperty);
            set => SetValue(TrackFalseCornerRadiusProperty, value);
        }
        public static readonly DependencyProperty TrackFalseCornerRadiusProperty =
            DependencyProperty.Register("TrackFalseCornerRadius", typeof(CornerRadius), typeof(TwoStateToggle),
                new PropertyMetadata(new CornerRadius(0)));
        public CornerRadius TrackTrueCornerRadius
        {
            get => (CornerRadius)GetValue(TrackTrueCornerRadiusProperty);
            set => SetValue(TrackTrueCornerRadiusProperty, value);
        }
        public static readonly DependencyProperty TrackTrueCornerRadiusProperty =
            DependencyProperty.Register("TrackTrueCornerRadius", typeof(CornerRadius), typeof(TwoStateToggle),
                new PropertyMetadata(new CornerRadius(0)));
        #endregion

        #region Cell Content
        public object FalseContent
        {
            get => GetValue(FalseContentProperty);
            set => SetValue(FalseContentProperty, value);
        }
        public static readonly DependencyProperty FalseContentProperty =
            DependencyProperty.Register("FalseContent", typeof(object), typeof(TwoStateToggle),
                new PropertyMetadata(null));
        public object TrueContent
        {
            get => GetValue(TrueContentProperty);
            set => SetValue(TrueContentProperty, value);
        }
        public static readonly DependencyProperty TrueContentProperty =
            DependencyProperty.Register("TrueContent", typeof(object), typeof(TwoStateToggle),
                new PropertyMetadata(null));

        public int FalseCellContentZIndex
        {
            get => (int)GetValue(FalseCellContentZIndexProperty);
            set => SetValue(FalseCellContentZIndexProperty, value);
        }
        public static readonly DependencyProperty FalseCellContentZIndexProperty =
            DependencyProperty.Register("FalseCellContentZIndex", typeof(int), typeof(TwoStateToggle), new PropertyMetadata(1));
        public int TrueCellContentZIndex
        {
            get => (int)GetValue(TrueCellContentZIndexProperty);
            set => SetValue(TrueCellContentZIndexProperty, value);
        }
        public static readonly DependencyProperty TrueCellContentZIndexProperty =
            DependencyProperty.Register("TrueCellContentZIndex", typeof(int), typeof(TwoStateToggle), new PropertyMetadata(1));

        public int ThumbZIndex
        {
            get => (int)GetValue(ThumbZIndexProperty);
            set => SetValue(ThumbZIndexProperty, value);
        }
        public static readonly DependencyProperty ThumbZIndexProperty =
            DependencyProperty.Register("ThumbZIndex", typeof(int), typeof(TwoStateToggle), new PropertyMetadata(1));

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }
        protected override void OnToggle()
        {
            if (IsChecked == false)
            {
                IsChecked = true;
            }
            else
            {
                IsChecked = false;
            }
        }
    }

    public class TriStateToggle : ToggleButton
    {
        private bool _lastNonNull = false;

        static TriStateToggle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TriStateToggle),
                new FrameworkPropertyMetadata(typeof(TriStateToggle)));

            IsThreeStateProperty.OverrideMetadata(
                typeof(TriStateToggle),
                new FrameworkPropertyMetadata(true));
        }

        public TriStateToggle() { }

        #region Display
        public Brush LeftBackgroundBrush
        {
            get => (Brush)GetValue(LeftBackgroundBrushProperty);
            set => SetValue(LeftBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty LeftBackgroundBrushProperty =
            DependencyProperty.Register("LeftBackgroundBrush", typeof(Brush), typeof(TriStateToggle), new PropertyMetadata(Brushes.Red));
        public Brush MiddleBackgroundBrush
        {
            get => (Brush)GetValue(MiddleBackgroundBrushProperty);
            set => SetValue(MiddleBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty MiddleBackgroundBrushProperty =
            DependencyProperty.Register("MiddleBackgroundBrush", typeof(Brush), typeof(TriStateToggle), new PropertyMetadata(Brushes.Gray));
        public Brush RightBackgroundBrush
        {
            get => (Brush)GetValue(RightBackgroundBrushProperty);
            set => SetValue(RightBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty RightBackgroundBrushProperty =
            DependencyProperty.Register("RightBackgroundBrush", typeof(Brush), typeof(TriStateToggle), new PropertyMetadata(Brushes.Green));
        public Brush ThumbBackgroundBrush
        {
            get => (Brush)GetValue(ThumbBackgroundBrushProperty);
            set => SetValue(ThumbBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty ThumbBackgroundBrushProperty =
            DependencyProperty.Register("ThumbBackgroundBrush", typeof(Brush), typeof(TriStateToggle), new PropertyMetadata(Brushes.White));
        public Brush ThumbBackgroundHoverBrush
        {
            get => (Brush)GetValue(ThumbBackgroundHoverBrushProperty);
            set => SetValue(ThumbBackgroundHoverBrushProperty, value);
        }
        public static readonly DependencyProperty ThumbBackgroundHoverBrushProperty =
            DependencyProperty.Register("ThumbBackgroundHoverBrush", typeof(Brush), typeof(TriStateToggle), new PropertyMetadata(Brushes.White));

        public Brush TrackBorderBrush
        {
            get => (Brush)GetValue(TrackBorderBrushProperty);
            set => SetValue(TrackBorderBrushProperty, value);
        }
        public static readonly DependencyProperty TrackBorderBrushProperty =
            DependencyProperty.Register("TrackBorderBrush", typeof(Brush), typeof(TriStateToggle), new PropertyMetadata(Brushes.Transparent));
        public Thickness TrackBorderThickness
        {
            get => (Thickness)GetValue(TrackBorderThicknessProperty);
            set => SetValue(TrackBorderThicknessProperty, value);
        }
        public static readonly DependencyProperty TrackBorderThicknessProperty =
            DependencyProperty.Register("TrackBorderThickness",typeof(Thickness),typeof(TriStateToggle),
                new PropertyMetadata(new Thickness(0)));

        public Brush ThumbBorderBrush
        {
            get => (Brush)GetValue(ThumbBorderBrushProperty);
            set => SetValue(ThumbBorderBrushProperty, value);
        }
        public static readonly DependencyProperty ThumbBorderBrushProperty =
            DependencyProperty.Register("ThumbBorderBrush", typeof(Brush), typeof(TriStateToggle), new PropertyMetadata(Brushes.Transparent));
        public double ThumbBorderThickness
        {
            get => (double)GetValue(ThumbBorderThicknessProperty);
            set => SetValue(ThumbBorderThicknessProperty, value);
        }
        public static readonly DependencyProperty ThumbBorderThicknessProperty =
            DependencyProperty.Register("ThumbBorderThickness", typeof(double), typeof(TriStateToggle), new PropertyMetadata(1.0));

        public Thickness TrackPadding
        {
            get => (Thickness)GetValue(TrackPaddingProperty);
            set => SetValue(TrackPaddingProperty, value);
        }
        public static readonly DependencyProperty TrackPaddingProperty =
            DependencyProperty.Register("TrackPadding", typeof(Thickness), typeof(TriStateToggle), new PropertyMetadata(new Thickness(0)));
        public CornerRadius TrackCornerRadius
        {
            get => (CornerRadius)GetValue(TrackCornerRadiusProperty);
            set => SetValue(TrackCornerRadiusProperty, value);
        }
        public static readonly DependencyProperty TrackCornerRadiusProperty =
            DependencyProperty.Register("TrackCornerRadius", typeof(CornerRadius), typeof(TriStateToggle),
                new PropertyMetadata(new CornerRadius(0)));
        public CornerRadius TrackLeftCornerRadius
        {
            get => (CornerRadius)GetValue(TrackLeftCornerRadiusProperty);
            set => SetValue(TrackLeftCornerRadiusProperty, value);
        }
        public static readonly DependencyProperty TrackLeftCornerRadiusProperty =
            DependencyProperty.Register("TrackLeftCornerRadius", typeof(CornerRadius), typeof(TriStateToggle),
                new PropertyMetadata(new CornerRadius(0)));
        public CornerRadius TrackMiddleCornerRadius
        {
            get => (CornerRadius)GetValue(TrackMiddleCornerRadiusProperty);
            set => SetValue(TrackMiddleCornerRadiusProperty, value);
        }
        public static readonly DependencyProperty TrackMiddleCornerRadiusProperty =
            DependencyProperty.Register("TrackMiddleCornerRadius", typeof(CornerRadius), typeof(TriStateToggle),
                new PropertyMetadata(new CornerRadius(0)));
        public CornerRadius TrackRightCornerRadius
        {
            get => (CornerRadius)GetValue(TrackRightCornerRadiusProperty);
            set => SetValue(TrackRightCornerRadiusProperty, value);
        }
        public static readonly DependencyProperty TrackRightCornerRadiusProperty =
            DependencyProperty.Register("TrackRightCornerRadius", typeof(CornerRadius), typeof(TriStateToggle),
                new PropertyMetadata(new CornerRadius(0)));
        #endregion

        #region Cell Content
        public UIElement LeftCellContent
        {
            get => (UIElement)GetValue(LeftCellContentProperty);
            set => SetValue(LeftCellContentProperty, value);
        }
        public static readonly DependencyProperty LeftCellContentProperty =
            DependencyProperty.Register(nameof(LeftCellContent), typeof(UIElement), typeof(TriStateToggle));
        public UIElement MiddleCellContent
        {
            get => (UIElement)GetValue(MiddleCellContentProperty);
            set => SetValue(MiddleCellContentProperty, value);
        }
        public static readonly DependencyProperty MiddleCellContentProperty =
            DependencyProperty.Register(nameof(MiddleCellContent), typeof(UIElement), typeof(TriStateToggle));
        public UIElement RightCellContent
        {
            get => (UIElement)GetValue(RightCellContentProperty);
            set => SetValue(RightCellContentProperty, value);
        }
        public static readonly DependencyProperty RightCellContentProperty =
            DependencyProperty.Register(nameof(RightCellContent), typeof(UIElement), typeof(TriStateToggle));

        public int LeftCellContentZIndex
        {
            get => (int)GetValue(LeftCellContentZIndexProperty);
            set => SetValue(LeftCellContentZIndexProperty, value);
        }
        public static readonly DependencyProperty LeftCellContentZIndexProperty =
            DependencyProperty.Register("LeftCellContentZIndex", typeof(int), typeof(TriStateToggle), new PropertyMetadata(1));
        public int MiddleCellContentZIndex
        {
            get => (int)GetValue(MiddleCellContentZIndexProperty);
            set => SetValue(MiddleCellContentZIndexProperty, value);
        }
        public static readonly DependencyProperty MiddleCellContentZIndexProperty =
            DependencyProperty.Register("MiddleCellContentZIndex", typeof(int), typeof(TriStateToggle), new PropertyMetadata(1));
        public int RightCellContentZIndex
        {
            get => (int)GetValue(RightCellContentZIndexProperty);
            set => SetValue(RightCellContentZIndexProperty, value);
        }
        public static readonly DependencyProperty RightCellContentZIndexProperty =
            DependencyProperty.Register("RightCellContentZIndex", typeof(int), typeof(TriStateToggle), new PropertyMetadata(1));

        public int ThumbZIndex
        {
            get => (int)GetValue(ThumbZIndexProperty);
            set => SetValue(ThumbZIndexProperty, value);
        }
        public static readonly DependencyProperty ThumbZIndexProperty =
            DependencyProperty.Register("ThumbZIndex", typeof(int), typeof(TriStateToggle), new PropertyMetadata(1));
        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (IsChecked == true) _lastNonNull = true;
            else if (IsChecked == false) _lastNonNull = false;
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ToggleButton.IsCheckedProperty)
            {
                var newVal = (bool?)e.NewValue;
                if (newVal.HasValue)
                {
                    _lastNonNull = newVal.Value;
                }
            }
        }
        protected override void OnToggle()
        {
            if (IsChecked == true || IsChecked == false)
            {
                IsChecked = null;
            }
            else
            {
                IsChecked = !_lastNonNull;
            }
        }
    }

    public class CircularTriStateToggle : ToggleButton
    {
        static CircularTriStateToggle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CircularTriStateToggle),
                new FrameworkPropertyMetadata(typeof(CircularTriStateToggle)));

            IsThreeStateProperty.OverrideMetadata(
                typeof(CircularTriStateToggle),
                new FrameworkPropertyMetadata(true));
        }

        public CircularTriStateToggle() { }

        #region Display
        public Brush TrueBackgroundBrush
        {
            get => (Brush)GetValue(TrueBackgroundBrushProperty);
            set => SetValue(TrueBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty TrueBackgroundBrushProperty =
            DependencyProperty.Register("TrueBackgroundBrush", typeof(Brush), typeof(CircularTriStateToggle), new PropertyMetadata(Brushes.Red));
        public Brush NullBackgroundBrush
        {
            get => (Brush)GetValue(NullBackgroundBrushProperty);
            set => SetValue(NullBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty NullBackgroundBrushProperty =
            DependencyProperty.Register("NullBackgroundBrush", typeof(Brush), typeof(CircularTriStateToggle), new PropertyMetadata(Brushes.Gray));
        public Brush FalseBackgroundBrush
        {
            get => (Brush)GetValue(FalseBackgroundBrushProperty);
            set => SetValue(FalseBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty FalseBackgroundBrushProperty =
            DependencyProperty.Register("FalseBackgroundBrush", typeof(Brush), typeof(CircularTriStateToggle), new PropertyMetadata(Brushes.Green));
        
        public Brush CircleBackgroundBrush
        {
            get => (Brush)GetValue(CircleBackgroundBrushProperty);
            set => SetValue(CircleBackgroundBrushProperty, value);
        }
        public static readonly DependencyProperty CircleBackgroundBrushProperty =
            DependencyProperty.Register("CircleBackgroundBrush", typeof(Brush), typeof(CircularTriStateToggle), new PropertyMetadata(Brushes.Black));
        public Brush CircleBorderBrush
        {
            get => (Brush)GetValue(CircleBorderBrushProperty);
            set => SetValue(CircleBorderBrushProperty, value);
        }
        public static readonly DependencyProperty CircleBorderBrushProperty =
            DependencyProperty.Register("CircleBorderBrush", typeof(Brush), typeof(CircularTriStateToggle), new PropertyMetadata(Brushes.Black));
        public Brush CircleBorderHoverBrush
        {
            get => (Brush)GetValue(CircleBorderBrushHoverProperty);
            set => SetValue(CircleBorderBrushHoverProperty, value);
        }
        public static readonly DependencyProperty CircleBorderBrushHoverProperty =
            DependencyProperty.Register("CircleBorderHoverBrush", typeof(Brush), typeof(CircularTriStateToggle), new PropertyMetadata(Brushes.Black));
        public double CircleBorderThickness
        {
            get => (double)GetValue(CircleBorderThicknessProperty);
            set => SetValue(CircleBorderThicknessProperty, value);
        }
        public static readonly DependencyProperty CircleBorderThicknessProperty =
            DependencyProperty.Register("CircleBorderThickness", typeof(double), typeof(CircularTriStateToggle), new PropertyMetadata(1.0));
        #endregion

        #region Cell Content
        public object FalseContent
        {
            get => GetValue(FalseContentProperty);
            set => SetValue(FalseContentProperty, value);
        }
        public static readonly DependencyProperty FalseContentProperty =
            DependencyProperty.Register("FalseContent", typeof(object), typeof(CircularTriStateToggle),
                new PropertyMetadata(null));
        public object TrueContent
        {
            get => GetValue(TrueContentProperty);
            set => SetValue(TrueContentProperty, value);
        }
        public static readonly DependencyProperty TrueContentProperty =
            DependencyProperty.Register("TrueContent", typeof(object), typeof(CircularTriStateToggle),
                new PropertyMetadata(null));
        public object NullContent
        {
            get => GetValue(NullContentProperty);
            set => SetValue(NullContentProperty, value);
        }
        public static readonly DependencyProperty NullContentProperty =
            DependencyProperty.Register("NullContent", typeof(object), typeof(CircularTriStateToggle),
                new PropertyMetadata(null));
        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }
        protected override void OnToggle()
        {
            if (IsChecked == false)
            {
                IsChecked = true;
            }
            else if (IsChecked == true)
            {
                IsChecked = null;
            }
            else
            {
                IsChecked = false;
            }
        }
    }
}
