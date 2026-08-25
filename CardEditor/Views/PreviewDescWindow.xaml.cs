using System.Windows;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.ComponentModel;
using MaterialDesignThemes.Wpf;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Commands;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Views
{
    /// <summary>
    /// Interaction logic for PreviewDescWindow.xaml
    /// </summary>
    public partial class PreviewDescWindow : Window, INotifyPropertyChanged
    {
        public MainWindow MainWindowReference { get; set; }

        #region Property

        #region Text
        private string _pendulumEffect = string.Empty;
        public string PendulumEffect
        {
            get => _pendulumEffect;
            set
            {
                if (_pendulumEffect != value)
                {
                    _pendulumEffect = value;
                    OnPropertyChanged(nameof(PendulumEffect));
                    CopyPenEffectCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private string _monsterEffect = string.Empty;
        public string MonsterEffect
        {
            get => _monsterEffect;
            set
            {
                if (_monsterEffect != value)
                {
                    _monsterEffect = value;
                    OnPropertyChanged(nameof(MonsterEffect));
                    CopyPenEffectCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private string _cardDescription = string.Empty;
        public string CardDescription
        {
            get => _cardDescription;
            set
            {
                if (_cardDescription != value)
                {
                    _cardDescription = value;
                    OnPropertyChanged(nameof(CardDescription));

                    ApplyLanguageCommand?.RaiseCanExecuteChanged();
                    CopyCardDescCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion

        #region Scale
        private int? _leftScale;
        public int? LeftScale
        {
            get => _leftScale;
            set
            {
                var safeValue = value is null || value < 0 ? 0 : value;
                if (_leftScale == safeValue) return;
                _leftScale = safeValue;
                _leftScaleText = safeValue.ToString();
                OnPropertyChanged(nameof(LeftScale));
                OnPropertyChanged(nameof(LeftScaleText));
            }
        }
        private string _leftScaleText = string.Empty;
        public string LeftScaleText
        {
            get => _leftScaleText;
            set
            {
                if (_leftScaleText == value) return;
                _leftScaleText = value;
                var parsed = ParseScale(value);
                if (_leftScale != parsed)
                {
                    _leftScale = parsed;
                    OnPropertyChanged(nameof(LeftScale));
                }
                OnPropertyChanged(nameof(LeftScaleText));
            }
        }

        private int? _rightScale;
        public int? RightScale
        {
            get => _rightScale;
            set
            {
                var safeValue = value is null || value < 0 ? 0 : value;
                if (_rightScale == safeValue) return;
                _rightScale = safeValue;
                _rightScaleText = safeValue.ToString();
                OnPropertyChanged(nameof(RightScale));
                OnPropertyChanged(nameof(RightScaleText));
            }
        }
        private string _rightScaleText = string.Empty;
        public string RightScaleText
        {
            get => _rightScaleText;
            set
            {
                if (_rightScaleText == value) return;
                _rightScaleText = value;
                var parsed = ParseScale(value);
                if (_rightScale != parsed)
                {
                    _rightScale = parsed;
                    OnPropertyChanged(nameof(RightScale));
                }
                OnPropertyChanged(nameof(RightScaleText));
            }
        }

        private static int ParseScale(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            if (!int.TryParse(text, out var value)) return 0;
            return value < 0 ? 0 : value;
        }
        #endregion

        #region Icon
        private MaterialDesignThemes.Wpf.PackIconKind _copyPenEffIcon = PackIconKind.ContentCopy;
        public MaterialDesignThemes.Wpf.PackIconKind CopyPenEffIcon
        {
            get => _copyPenEffIcon;
            set
            {
                if (_copyPenEffIcon != value)
                {
                    _copyPenEffIcon = value;
                    OnPropertyChanged(nameof(CopyPenEffIcon));
                }
            }
        }
        private MaterialDesignThemes.Wpf.PackIconKind _copyMonsEffIcon = PackIconKind.ContentCopy;
        public MaterialDesignThemes.Wpf.PackIconKind CopyMonsEffIcon
        {
            get => _copyMonsEffIcon;
            set
            {
                if (_copyMonsEffIcon != value)
                {
                    _copyMonsEffIcon = value;
                    OnPropertyChanged(nameof(CopyMonsEffIcon));
                }
            }
        }
        private MaterialDesignThemes.Wpf.PackIconKind _copyCardDescIcon = PackIconKind.ContentCopy;
        public MaterialDesignThemes.Wpf.PackIconKind CopyCardDescIcon
        {
            get => _copyCardDescIcon;
            set
            {
                if (_copyCardDescIcon != value)
                {
                    _copyCardDescIcon = value;
                    OnPropertyChanged(nameof(CopyCardDescIcon));
                }
            }
        }
        #endregion

        private bool _isNormalCard = false;
        public bool IsNormalCard
        {
            get => _isNormalCard;
            set
            {
                if (_isNormalCard != value)
                {
                    _isNormalCard = value;
                    OnPropertyChanged(nameof(IsNormalCard));
                }
            }
        }

        private PendulumLanguageRule _selectedLanguage;
        public PendulumLanguageRule SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged(nameof(SelectedLanguage));

                    SelectLanguageCommand?.RaiseCanExecuteChanged();
                    ApplyLanguageCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion

        #region Command
        public RelayCommand ReloadLanguageCommand { get; set; }
        public RelayCommand SelectLanguageCommand { get; set; }
        public RelayCommand ApplyLanguageCommand { get; set; }

        public RelayCommand CopyPenEffectCommand { get; set; }
        public RelayCommand CopyMonsterEffectCommand { get; set; }
        public RelayCommand CopyCardDescCommand { get; set; }

        public RelayCommand CloseWindowCommand { get; set; }
        #endregion

        #region Contructor
        public PreviewDescWindow()
        {
            InitializeComponent();
            InitializeCommands();

            this.DataContext = this;
        }
        private void InitializeCommands()
        {
            ReloadLanguageCommand = new CardEditor.Commands.RelayCommand(async _ => await ReloadPenLanguage());
            SelectLanguageCommand = new CardEditor.Commands.RelayCommand(_ => SelectLanguage(), _ => SelectedLanguageNotNull());
            ApplyLanguageCommand = new CardEditor.Commands.RelayCommand(_ => ApplyLanguage(), _ => SelectedLanguageNotNull() && !string.IsNullOrWhiteSpace(CardDescription));

            CopyPenEffectCommand = new CardEditor.Commands.RelayCommand(async _ => await CopyPenEffectExecute(), _ => CanCopyPenEffect());
            CopyMonsterEffectCommand = new CardEditor.Commands.RelayCommand(async _ => await CopyMonsterEffectExecute(), _ => CanCopyMonsterEffect());
            CopyCardDescCommand = new CardEditor.Commands.RelayCommand(async _ => await CopyCardDescExecute(), _ => CanCopyCardDesc());

            CloseWindowCommand = new CardEditor.Commands.RelayCommand(_ => this.Close());
        }
        public void InitializePartEffect(CardEditor.Models.Card card, PendulumLanguageRule rule)
        {
            SelectedLanguage = rule;

            if (card == null)
            {
                PendulumEffect = string.Empty;
                MonsterEffect = string.Empty;
                CardDescription = string.Empty;

                LeftScale = null;
                RightScale = null;

                IsNormalCard = false;
                return;
            }

            PenAnalysisResult result = PenLanguageViewModel.Instance.Analyze(card.desc);
            PenDescResult effect = result.DescResult;
            PendulumEffect = effect.PendulumEffect;
            MonsterEffect = effect.MonsterEffect;

            IsNormalCard = (card.type & (ulong)CardType.Normal) != 0;

            PenScale scale = GetPenScaleHelp.GetPenScale(card.level);

            LeftScale = scale.LeftScale;
            RightScale = scale.RightScale;

            CardDescription = BuildCardDesc();
        }
        #endregion

        #region Load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeContentMenu();
        }
        private void InitializeContentMenu()
        {
            ControlContextMenuService.Attach(txtLeftScale);
            ControlContextMenuService.Attach(txtRightScale);
            ControlContextMenuService.Attach(txtPendulumEffect);
            ControlContextMenuService.Attach(txtMonsterEffect);
            ControlContextMenuService.Attach(txtCardDescription);
        }
        #endregion

        #region Functions
        public string BuildCardDesc()
        {
            PenDescResult effect = new PenDescResult { PendulumEffect = PendulumEffect, MonsterEffect = MonsterEffect };
            PenScale scale = new PenScale { LeftScale = LeftScale.HasValue ? LeftScale.Value : 0, RightScale = RightScale.HasValue ? RightScale.Value : 0 };
            string desc = PenLanguageViewModel.Instance.BuildDesc(SelectedLanguage, effect, scale, IsNormalCard);
            return desc;
        }
        private async Task ReloadPenLanguage()
        {
            var (resultPenLang, messagePenLang) = await PenLanguageViewModel.Instance.LoadAsync();
            if (!resultPenLang)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messagePenLang}", new[] { CMess.ok.ToText() });
            }
        }
        private void SelectLanguage()
        {
            if (SelectedLanguage == null) return;
            if (SelectedLanguage?.Locale is PendulumLocales.EDOProNonPen or PendulumLocales.Unknown) return;

            CardDescription = BuildCardDesc();
        }
        private void ApplyLanguage()
        {
            if (SelectedLanguage == null || string.IsNullOrWhiteSpace(CardDescription)) return;
            MainWindowReference.ApplyPendulumLanguage(CardDescription);

            this.Close();
        }
        private bool SelectedLanguageNotNull()
        {
            return SelectedLanguage != null;
        }

        #region Copy
        private async Task CopyPenEffectExecute()
        {
            bool success = CopyPenEffect();
            if (!success) return;

            CopyPenEffIcon = PackIconKind.Check;

            await Task.Delay(5000);
            CopyPenEffIcon = PackIconKind.ContentCopy;
        }
        private bool CopyPenEffect()
        {
            try
            {
                Clipboard.SetText(PendulumEffect);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool CanCopyPenEffect()
        {
            return !string.IsNullOrWhiteSpace(PendulumEffect);
        }

        private async Task CopyMonsterEffectExecute()
        {
            bool success = CopyMonsterEffect();
            if (!success) return;

            CopyMonsEffIcon = PackIconKind.Check;

            await Task.Delay(5000);
            CopyMonsEffIcon = PackIconKind.ContentCopy;
        }
        private bool CopyMonsterEffect()
        {
            try
            {
                Clipboard.SetText(MonsterEffect);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool CanCopyMonsterEffect()
        {
            return !string.IsNullOrWhiteSpace(MonsterEffect);
        }

        private async Task CopyCardDescExecute()
        {
            bool success = CopyCardDesc();
            if (!success) return;

            CopyCardDescIcon = PackIconKind.Check;

            await Task.Delay(5000);
            CopyCardDescIcon = PackIconKind.ContentCopy;
        }
        private bool CopyCardDesc()
        {
            try
            {
                Clipboard.SetText(CardDescription);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool CanCopyCardDesc()
        {
            return !string.IsNullOrWhiteSpace(CardDescription);
        }
        #endregion

        #endregion

        #region Event
        private void blHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void btn_Checked(object sender, RoutedEventArgs e) // Left
        {
            if (MainHost.IsRightDrawerOpen == true)
            {
                MainHost.IsRightDrawerOpen = false;
                btnLanguageList.IsChecked = false;
            }
            MainHost.IsLeftDrawerOpen = true;
        }
        private void btn_Unchecked(object sender, RoutedEventArgs e) // Left
        {
            MainHost.IsLeftDrawerOpen = false;
        }
        private void btnLanguageList_Checked(object sender, RoutedEventArgs e) // Right
        {
            if (MainHost.IsLeftDrawerOpen == true)
            {
                MainHost.IsLeftDrawerOpen = false;
                btn.IsChecked = false;
            }
            MainHost.IsRightDrawerOpen = true;
        }
        private void btnLanguageList_Unchecked(object sender, RoutedEventArgs e) // Right
        {
            MainHost.IsRightDrawerOpen = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        #endregion

    }
}
