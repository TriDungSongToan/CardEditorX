using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for RotatingTitle.xaml
    /// </summary>
    public partial class RotatingTitle : UserControl, INotifyPropertyChanged
    {
        #region Rotating Title
        private const int DayStartHour = 6;    // 06:00 -> bắt đầu ban ngày
        private const int NightStartHour = 22; // 22:00 -> bắt đầu ban đêm

        private static readonly TimeSpan RotateInterval = TimeSpan.FromSeconds(5);

        private readonly List<string> _dayTitles = new()
        {
            "The Ultimate Yu-Gi-Oh! Development Ecosystem",
            "Build. Edit. Script. Create.",
            "Forge your cards. Forge your meta.",
            "Where Duels Are Built.",
            "The Yu-Gi-Oh! Development Environment",
            "One Hub. Every Card.",
            "Craft. Balance. Duel.",
            "Your workspace for cards, decks, data and beyond.",
            "The Core Toolkit for Yu-Gi-Oh! Development",
            "Everything a Duelist Needs."
        };
        private readonly List<string> _nightTitles = new()
        {
            "Burning the midnight oil? Let's forge some cards.",
            "The Yu-Gi-Oh! Development Laboratory — Open all night.",
            "From Omega to Infinity.",
            "The Codex of Every Card.",
            "Where the meta is shaped, one script at a time.",
            "Silence. Focus. Scripts.",
            "Every card has a story. Write yours.",
            "The Duelist's Workshop — After Hours.",
            "Build in the dark. Shine in the meta.",
            "Night shift for the next meta."
        };

        private readonly DispatcherTimer _titleTimer;
        private readonly Random _random = new();
        private readonly Queue<string> _shuffleQueue = new();
        private string _currentTitle = string.Empty;
        public string CurrentTitle
        {
            get => _currentTitle;
            private set
            {
                if (_currentTitle == value) return;
                _currentTitle = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public RotatingTitle()
        {
            InitializeComponent();

            this.DataContext = this;

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            _titleTimer = new DispatcherTimer { Interval = RotateInterval };
            _titleTimer.Tick += (_, _) => RotateTitle();

            Loaded += UserControl_Loaded;
            Unloaded += UserControl_Unloaded;
        }
        #endregion

        #region Methods
        private void RotateTitle()
        {
            var pool = GetActivePool();
            if (pool.Count == 0) return;

            // Shuffle-bag: hết cả bộ mới xáo lại, tránh vừa hiện xong lại hiện ngay câu đó
            if (_shuffleQueue.Count == 0)
            {
                var shuffled = pool.OrderBy(_ => _random.Next()).ToList();
                foreach (var t in shuffled) _shuffleQueue.Enqueue(t);
            }

            CurrentTitle = _shuffleQueue.Dequeue();
        }
        private List<string> GetActivePool()
        {
            var hour = DateTime.Now.Hour;
            var isDay = hour >= DayStartHour && hour < NightStartHour;
            return isDay ? _dayTitles : _nightTitles;
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RotateTitle();
            _titleTimer?.Start();
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _titleTimer?.Stop();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
