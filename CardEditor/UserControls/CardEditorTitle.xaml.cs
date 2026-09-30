using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.ViewModels;
using Path = System.Windows.Shapes.Path;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for CardEditorTitle.xaml
    /// </summary>
    public partial class CardEditorTitle : UserControl
    {
        #region Flip Title
        private List<Path> letterPaths = new List<Path>();
        private List<ImageBrush> currentBrushes = new();
        private int blankPos = 0;
        private bool movingRight = true;
        private DispatcherTimer timer;
        #endregion

        #region Constructor
        public CardEditorTitle()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this)) return;

            Loaded += UserControl_Loaded;
            Unloaded += UserControl_Unloaded;
        }
        #endregion

        #region Methods
        void BuildLetters(string word)
        {
            var cardBrushes = HomeImageViewModel.Instance.CardBrushes;
            if (cardBrushes == null || cardBrushes.Count == 0) return;

            var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
            double fontSize = 80;
            double x = 0;

            currentBrushes = new List<ImageBrush>(cardBrushes);
            blankPos = 0;
            movingRight = true;

            for (int i = 0; i < word.Length; i++)
            {
                var ft = new FormattedText(
                    word[i].ToString(),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    fontSize,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                var geometry = ft.BuildGeometry(new Point(x, 0));

                var path = new Path
                {
                    Data = geometry,
                    Fill = currentBrushes[i % currentBrushes.Count],
                    Stroke = UIConfigViewModel.Instance.ThemeColor,
                    StrokeThickness = 0.5,
                    StrokeLineJoin = PenLineJoin.Round
                };

                letterPaths.Add(path);
                LetterCanvas.Children.Add(path);

                x += ft.WidthIncludingTrailingWhitespace;
            }

            LetterCanvas.Width = x;
            LetterCanvas.Height = fontSize * 1.2;
        }
        void StartRotation()
        {
            timer?.Stop();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (currentBrushes == null || currentBrushes.Count < 2) return;

            int nextPos = movingRight ? blankPos + 1 : blankPos - 1;

            // Đảm bảo không vượt giới hạn
            if (nextPos < 0 || nextPos >= currentBrushes.Count) return;

            // Swap
            (currentBrushes[blankPos], currentBrushes[nextPos]) = (currentBrushes[nextPos], currentBrushes[blankPos]);

            blankPos = nextPos;

            // Đảo hướng khi tới đầu
            if (blankPos == currentBrushes.Count - 1) movingRight = false;
            else if (blankPos == 0) movingRight = true;

            // Update Fill
            for (int i = 0; i < letterPaths.Count; i++)
            {
                letterPaths[i].Fill = currentBrushes[i % currentBrushes.Count];
            }
        }
        private void ClearLetters()
        {
            LetterCanvas.Children.Clear();

            letterPaths.Clear();
            currentBrushes.Clear();

            LetterCanvas.Width = 0;
            LetterCanvas.Height = 0;

            blankPos = 0;
            movingRight = true;
        }
        #endregion

        #region Event
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Đảm bảo HomeImageViewModel đã load data
            var (resultHome, messageHome) = await HomeImageViewModel.Instance.LoadBrushes();
            if (!resultHome)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load card brushes: {messageHome}");
                return;
            }

            // Phòng trường hợp Loaded xảy ra lại
            ClearLetters();

            BuildLetters("CardEditor");

            StartRotation();
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            timer?.Stop();

            if (timer != null)
            {
                timer.Tick -= Timer_Tick;
                timer = null;
            }

            ClearLetters();
        }
        #endregion
    }
}
