using System;
using System.IO;
using System.Net.Http;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ComponentModel;
using MahApps.Metro.Controls;
using Dragablz;
using CardEditor.Enums;
using CardEditor.Views;
using CardEditor.Models;
using CardEditor.Manager;
using CardEditor.Helpers;
using CardEditor.Commands;
using CardEditor.Services;
using CardEditor.Constants;
using CardEditor.ImageGene;
using CardEditor.ViewModels;
using CardEditor.UserControls;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using CardAppContext = CardEditor.Models.AppContext;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged, IMainWindowService
    {
        #region Variable
        private bool isDeveloper = false;
        public bool isShuttingDown;
        private bool _isDragging = false;
        private bool isDragging = false;
        private bool isMouseDown = false;
        private double _initialWidth;
        private Point _startPoint;
        private Point dragStartPoint;
        private Point elementStartPosition;
        public ObservableCollection<CardEditor.Models.TabContent> Tabs { get; set; }
        public UserControl currentUserControl { get; set; }

        private RareEditor _rareEditor;
        private GenesysEditor _genesysEditor;

        private readonly DoubleAnimation _loadingAnimation = new()
        {
            From = 0,
            To = 360,
            Duration = TimeSpan.FromSeconds(1),
            RepeatBehavior = RepeatBehavior.Forever
        };
        #endregion

        #region Property
        private string _mainWindowtitle = string.Empty;
        public string MainWindowTitle
        {
            get => _mainWindowtitle;
            set
            {
                if (_mainWindowtitle != value)
                {
                    _mainWindowtitle = value;
                    OnPropertyChanged(nameof(MainWindowTitle));
                    OnPropertyChanged(nameof(CanBrowseWindowTitle));

                    BrowseFileCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        public bool CanBrowseWindowTitle =>
            !string.IsNullOrEmpty(MainWindowTitle)
            && File.Exists(MainWindowTitle);
        private bool _isSaved = true;
        public bool IsSaved
        {
            get => _isSaved;
            set
            {
                if (_isSaved != value)
                {
                    _isSaved = value;
                    OnPropertyChanged(nameof(IsSaved));
                }
            }
        }

        #region Visibility
        private Visibility _dataEditVisibility = Visibility.Collapsed;
        public Visibility DataEditVisibility
        {
            get => _dataEditVisibility;
            set
            {
                if (_dataEditVisibility != value)
                {
                    _dataEditVisibility = value;
                    OnPropertyChanged(nameof(DataEditVisibility));
                    OnPropertyChanged(nameof(CardVisibility));
                }
            }
        }
        private Visibility _scriptEditVisibility = Visibility.Collapsed;
        public Visibility ScriptEditVisibility
        {
            get => _scriptEditVisibility;
            set
            {
                if (_scriptEditVisibility != value)
                {
                    _scriptEditVisibility = value;
                    OnPropertyChanged(nameof(ScriptEditVisibility));
                    OnPropertyChanged(nameof(SaveVisibility));


                }
            }
        }
        private Visibility _deckEditVisibility = Visibility.Collapsed;
        public Visibility DeckEditVisibility
        {
            get => _deckEditVisibility;
            set
            {
                if (_deckEditVisibility != value)
                {
                    _deckEditVisibility = value;
                    OnPropertyChanged(nameof(DeckEditVisibility));
                    OnPropertyChanged(nameof(SaveVisibility));
                }
            }
        }
        private Visibility _banListEditVisibility = Visibility.Collapsed;
        public Visibility BanListEditVisibility
        {
            get => _banListEditVisibility;
            set
            {
                if (_banListEditVisibility != value)
                {
                    _banListEditVisibility = value;
                    OnPropertyChanged(nameof(BanListEditVisibility));
                    OnPropertyChanged(nameof(SaveVisibility));
                    OnPropertyChanged(nameof(CardVisibility));

                }
            }
        }

        public Visibility SaveVisibility
        {
            get
            {
                return (ScriptEditVisibility == Visibility.Visible ||
                    DeckEditVisibility == Visibility.Visible ||
                    BanListEditVisibility == Visibility.Visible)

                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public Visibility CardVisibility
        {
            get
            {
                return (DataEditVisibility == Visibility.Visible ||
                    BanListEditVisibility == Visibility.Visible)

                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        #endregion

        #region Commands
        public RelayCommand BrowseFileCommand { get; set; }
        #endregion

        #region Constructor 
        public MainWindow()
        {
            InitializeComponent();
            InitializeInputBindings();
            InitializeCommand();

            Tabs = new ObservableCollection<CardEditor.Models.TabContent>();
            //DataContext = UIConfigViewModel.Instance;
            DataContext = this;

            ((App)Application.Current).RegisterWindow(this);
        }
        public MainWindow(string[] args)
        {
            InitializeComponent();
            InitializeInputBindings();
            InitializeCommand();

            Tabs = new ObservableCollection<CardEditor.Models.TabContent>();
            //DataContext = UIConfigViewModel.Instance;
            DataContext = this;

            ((App)Application.Current).RegisterWindow(this);

            if (args.Length > 0 && File.Exists(args[0]))
            {
                HandleFileOpen(args[0]);
            }
        }
        private void InitializeInputBindings()
        {
            this.InputBindings.Add(new KeySequenceBinding(CustomCommands.NewDatabase,
                new OptimizedKeySequenceGesture(new KeyGesturePart(Key.N, ModifierKeys.Control), new KeyGesturePart(Key.D, ModifierKeys.Control))));
            this.InputBindings.Add(new KeySequenceBinding(CustomCommands.NewExcel,
                new OptimizedKeySequenceGesture(new KeyGesturePart(Key.N, ModifierKeys.Control), new KeyGesturePart(Key.E, ModifierKeys.Control))));
            this.InputBindings.Add(new KeySequenceBinding(CustomCommands.NewCeds,
                new OptimizedKeySequenceGesture(new KeyGesturePart(Key.N, ModifierKeys.Control), new KeyGesturePart(Key.C, ModifierKeys.Control))));

            this.InputBindings.Add(new KeySequenceBinding(CustomCommands.NewScript,
                new OptimizedKeySequenceGesture(new KeyGesturePart(Key.N, ModifierKeys.Control), new KeyGesturePart(Key.S, ModifierKeys.Control))));
            this.InputBindings.Add(new KeySequenceBinding(CustomCommands.NewDeck,
                new OptimizedKeySequenceGesture(new KeyGesturePart(Key.N, ModifierKeys.Control), new KeyGesturePart(Key.K, ModifierKeys.Control))));
            this.InputBindings.Add(new KeySequenceBinding(CustomCommands.NewBanList,
                new OptimizedKeySequenceGesture(new KeyGesturePart(Key.N, ModifierKeys.Control), new KeyGesturePart(Key.B, ModifierKeys.Control))));

            this.InputBindings.Add(new KeySequenceBinding(CustomCommands.OpenArchive,
                new OptimizedKeySequenceGesture(new KeyGesturePart(Key.O, ModifierKeys.Control), new KeyGesturePart(Key.A, ModifierKeys.Control))));
            this.InputBindings.Add(new KeySequenceBinding(CustomCommands.OpenDatabase,
                new OptimizedKeySequenceGesture(new KeyGesturePart(Key.O, ModifierKeys.Control), new KeyGesturePart(Key.D, ModifierKeys.Control))));
            this.InputBindings.Add(new KeySequenceBinding(CustomCommands.OpenScript,
                new OptimizedKeySequenceGesture(new KeyGesturePart(Key.O, ModifierKeys.Control), new KeyGesturePart(Key.S, ModifierKeys.Control))));
            this.InputBindings.Add(new KeySequenceBinding(CustomCommands.OpenDeck,
                new OptimizedKeySequenceGesture(new KeyGesturePart(Key.O, ModifierKeys.Control), new KeyGesturePart(Key.K, ModifierKeys.Control))));
            this.InputBindings.Add(new KeySequenceBinding(CustomCommands.OpenBanList,
                new OptimizedKeySequenceGesture(new KeyGesturePart(Key.O, ModifierKeys.Control), new KeyGesturePart(Key.B, ModifierKeys.Control))));

            this.InputBindings.Add(new KeyBinding(CardEditor.Commands.CustomCommands.Save, Key.S, ModifierKeys.Control));
            this.InputBindings.Add(new KeyBinding(CardEditor.Commands.CustomCommands.SaveAs, Key.S, ModifierKeys.Control | ModifierKeys.Shift));
            this.InputBindings.Add(new KeyBinding(CardEditor.Commands.CustomCommands.Setting, Key.S, ModifierKeys.Alt));
        }
        private void InitializeCommand()
        {
            BrowseFileCommand = new CardEditor.Commands.RelayCommand(_ => BrowseFileTitle(), _ => CanBrowseFileTitle());
        }
        public async void HandleFileOpen(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) return;

            if (filePath.EndsWith(".lflist.conf", StringComparison.OrdinalIgnoreCase))
            {
                await OpenBanListCommand(filePath);
                return;
            }

            string extension = System.IO.Path.GetExtension(filePath).ToLower();

            switch (extension.ToLowerInvariant())
            {
                case ".zip":
                case ".ypk":
                    await OpenArchiveCommand(filePath);
                    break;

                case ".cdb":
                case ".db":
                case ".sqlite":
                case ".ceds":
                case ".xlsx":
                    await OpenDatabaseCommand(filePath);
                    break;

                case ".lua":
                case ".txt":
                case ".md":
                case ".log":
                case ".yml":
                case ".conf":
                    await OpenScriptCommand(filePath);
                    break;

                case ".ydk":
                    await OpenDeckCommand(filePath);
                    break;

                default: CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    $"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText())} {filePath}",
                    new[] { CMess.ok.ToText() });
                    break;

            }
        }
        private void Instance_ThemeChanged(object sender, string newTheme)
        {
            var appResources = Application.Current.Resources;

            var oldTheme = appResources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("MaterialDesignColor"));
            if (oldTheme != null) appResources.MergedDictionaries.Remove(oldTheme);

            Uri themeUri = new Uri($"pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.{newTheme}.xaml", UriKind.Absolute);
            ResourceDictionary newResource = new ResourceDictionary { Source = themeUri };
            appResources.MergedDictionaries.Add(newResource);
        }
        #endregion

        #region load
        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBGImage();
            LoadConfig();
            LoadRecentMenuItems();
            CheckGit();

            StartLoading();

            await this.Dispatcher.BeginInvoke(new Action(LoadChatButton), System.Windows.Threading.DispatcherPriority.Loaded);
            await ImageCacheService.Instance.LoadAsync(); //ok
            await CardDataViewModel.Instance.LoadData();  //ok
            var (resultChar, messageChar) = await SpecialCharViewModel.Instance.LoadChar();
            var (resultRare, messageRare) = await RareRawDataViewModel.Instance.LoadData();
            var (resultGenesys, messageGenesys) = await GenesysRawDataViewModel.Instance.LoadData();
            var (resultPenLang, messagePenLang) = await PenLanguageViewModel.Instance.LoadAsync();
            var (resultCredit, messageCredit) = await CreditsViewModel.Instance.LoadData();
            LoadSeriesImage();

            if (!resultChar)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageChar}", new[] { CMess.ok.ToText() });
            }
            if (!resultRare)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageRare}", new[] { CMess.ok.ToText() });
            }
            if (!resultGenesys)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageGenesys}", new[] { CMess.ok.ToText() });
            }
            if (!resultPenLang)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messagePenLang}", new[] { CMess.ok.ToText() });
            }
            if (!resultCredit)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageCredit}", new[] { CMess.ok.ToText() });
            }

            await FinishLoading();
        }
        private void StartLoading()
        {
            LoadingIndicator.Visibility = Visibility.Visible;

            LoadingCircle.Visibility = Visibility.Visible;
            SuccessIcon.Visibility = Visibility.Collapsed;

            LoadingRotation.BeginAnimation(RotateTransform.AngleProperty, _loadingAnimation);
        }
        private async Task FinishLoading()
        {
            // Dừng xoay
            LoadingRotation.BeginAnimation(RotateTransform.AngleProperty, null);
            LoadingCircle.Visibility = Visibility.Collapsed;
            SuccessIcon.Visibility = Visibility.Visible;
            await Task.Delay(2000);
            LoadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void LoadBGImage()
        {
            string imagePath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, @"CardData\Images\MainLogo.png");
            BitmapImage bitmap = new BitmapImage();

            if (System.IO.File.Exists(imagePath))
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            else
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/CardEditor;component/Images/MainLogo.png", UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            bitmap.Freeze();
            imgmainbg.Source = bitmap;
        }

        public void LoadConfig()
        {
            #region Color
            string backgroundHex = ConfigViewModel.Instance.displaySetting.Background;
            string foregroundHex = ConfigViewModel.Instance.displaySetting.Foreground;
            Color backgroundColor = (Color)ColorConverter.ConvertFromString(backgroundHex);
            Color foregroundColor = (Color)ColorConverter.ConvertFromString(foregroundHex);
            Brush backgroundBrush = new SolidColorBrush(backgroundColor);
            Brush foregroundBrush = new SolidColorBrush(foregroundColor);
            #endregion

            this.WindowTitleBrush = backgroundBrush;
            this.TitleForeground = foregroundBrush;

            foreach (var item in menudatamain.Items)
            {
                if (item is MenuItem menuItem)
                {
                    SetMenuItemColors(menuItem, backgroundBrush, foregroundBrush);
                }
            }
            string encryptedDevelop = ConfigViewModel.Instance.DeveloperEncrypted;
            isDeveloper = SettingsEncryption.DecryptBoolSetting(encryptedDevelop);
            menuDev.Visibility = isDeveloper ? Visibility.Visible : Visibility.Collapsed;
            blMainChat.Width = ConfigViewModel.Instance.displaySetting.WidthChat;
            DrawerTransform.X = blMainChat.Width;
        }
        private void SetMenuItemColors(MenuItem menuItem, Brush background, Brush foreground)
        {
            menuItem.Background = background;
            menuItem.Foreground = foreground;

            foreach (var item in menuItem.Items)
            {
                if (item is MenuItem subMenuItem)
                {
                    SetMenuItemColors(subMenuItem, background, foreground);
                }
            }
        }
        private void LoadLanguage()
        {
            string currentLanguage = ConfigViewModel.Instance.userSetting.Language;
            LanguageManager.Initialize();
            LanguageManager.Instance.LoadLanguage(currentLanguage);
        }
        private void CheckGit()
        {
            try
            {
                string encryptedDevelop = ConfigViewModel.Instance.DeveloperEncrypted;
                isDeveloper = SettingsEncryption.DecryptBoolSetting(encryptedDevelop);

                //string scrapiyardPath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "scrapiyard");
                //string CardDataPath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "CardData");

                //if (isDeveloper)
                //{
                //    if (!(Directory.Exists(scrapiyardPath) && GitHubService.IsGitRepository(scrapiyardPath) &&
                //    Directory.Exists(CardDataPath) && GitHubService.IsGitRepository(CardDataPath)))
                //    {
                //        CMSG.Show("Warning", CMSG.MessageBoxIconType.Warning,
                //            "Scrapiyard or CardData source is missing or does not exist, Check Update and restart application.", new[] { "OK" });
                //    }
                //}
                //else
                //{
                //    if (!Directory.Exists(CardDataPath) && GitHubService.IsGitRepository(CardDataPath))
                //    {
                //        CMSG.Show("Warning", CMSG.MessageBoxIconType.Warning,
                //            "CardData source is missing or does not exist, Check Update and restart application.", new[] { "OK" });
                //    }
                //}
            }
            catch (Exception ex)
            {
                CMSG.Show("Error", CMSG.MessageBoxIconType.Error,
                    $"Unexpected error in CheckGit: {ex.Message}", new[] { "OK" });
            }
        }
        private void LoadChatButton()
        {
            Point position = ParsePoint(ConfigViewModel.Instance.displaySetting.ButtonChat);

            Canvas.SetLeft(ChatIcon, canvas.ActualWidth - ChatIcon.ActualWidth - position.X);
            Canvas.SetTop(ChatIcon, position.Y);
        }
        private void LoadRecentMenuItems()
        {
            menuRecentArchive.Items.Clear();
            menuRecentDatabases.Items.Clear();
            menuRecentScripts.Items.Clear();
            menuRecentDeck.Items.Clear();
            menuRecentBanList.Items.Clear();

            // Archive
            var recentArchive = OpenHistoryViewModel.Instance.GetRecentItems(0);
            foreach (var item in recentArchive)
            {
                var menuItem = new MenuItem
                {
                    Header = System.IO.Path.GetFileName(item.Url),
                    ToolTip = $"{item.Url}\n({CMess.Open.ToText()}: {item.Time})",
                    Command = Commands.CustomCommands.OpenArchive,
                    CommandParameter = item.Url
                };
                menuRecentArchive.Items.Add(menuItem);
            }
            if (recentArchive.Count > 0)
            {
                menuRecentArchive.Items.Add(new Separator());
                var clearArchiveItem = new MenuItem
                {
                    Header = CMess.ClearHistory.ToText(),
                };
                clearArchiveItem.Click += ClearArchiveItem_Click;
                menuRecentArchive.Items.Add(clearArchiveItem);
            }

            // Card Database
            var recentDatabases = OpenHistoryViewModel.Instance.GetRecentItems(1);
            foreach (var item in recentDatabases)
            {
                var menuItem = new MenuItem
                {
                    Header = System.IO.Path.GetFileName(item.Url),
                    ToolTip = $"{item.Url}\n({CMess.Open.ToText()}: {item.Time})",
                    Command = Commands.CustomCommands.OpenDatabase,
                    CommandParameter = item.Url
                };
                menuRecentDatabases.Items.Add(menuItem);
            }
            if (recentDatabases.Count > 0)
            {
                menuRecentDatabases.Items.Add(new Separator());
                var clearDBItem = new MenuItem
                {
                    Header = CMess.ClearHistory.ToText(),
                };
                clearDBItem.Click += ClearDBItem_Click;
                menuRecentDatabases.Items.Add(clearDBItem);
            }
            // Card Script
            var recentScripts = OpenHistoryViewModel.Instance.GetRecentItems(2);
            foreach (var item in recentScripts)
            {
                var menuItem = new MenuItem
                {
                    Header = System.IO.Path.GetFileName(item.Url),
                    ToolTip = $"{item.Url}\n({CMess.Open.ToText()}: {item.Time})",
                    Command = Commands.CustomCommands.OpenScript,
                    CommandParameter = item.Url
                };
                menuRecentScripts.Items.Add(menuItem);
            }
            if (recentScripts.Count > 0)
            {
                menuRecentScripts.Items.Add(new Separator());
                var clearScriptItem = new MenuItem
                {
                    Header = CMess.ClearHistory.ToText(),
                };
                clearScriptItem.Click += ClearScriptItem_Click;
                menuRecentScripts.Items.Add(clearScriptItem);
            }
            // Deck
            var recentDecks = OpenHistoryViewModel.Instance.GetRecentItems(3);
            foreach (var item in recentDecks)
            {
                var menuItem = new MenuItem
                {
                    Header = System.IO.Path.GetFileName(item.Url),
                    ToolTip = $"{item.Url}\n({CMess.Open.ToText()}: {item.Time})",
                    Command = Commands.CustomCommands.OpenDeck,
                    CommandParameter = item.Url
                };
                menuRecentDeck.Items.Add(menuItem);
            }
            if (recentDecks.Count > 0)
            {
                menuRecentDeck.Items.Add(new Separator());
                var clearDeckItem = new MenuItem
                {
                    Header = CMess.ClearHistory.ToText(),
                };
                clearDeckItem.Click += ClearDeckItem_Click;
                menuRecentDeck.Items.Add(clearDeckItem);
            }
            // Banlist
            var recentBanLists = OpenHistoryViewModel.Instance.GetRecentItems(4);
            foreach (var item in recentBanLists)
            {
                var menuItem = new MenuItem
                {
                    Header = System.IO.Path.GetFileName(item.Url),
                    ToolTip = $"{item.Url}\n({CMess.Open.ToText()}: {item.Time})",
                    Command = Commands.CustomCommands.OpenBanList,
                    CommandParameter = item.Url
                };
                menuRecentBanList.Items.Add(menuItem);
            }
            if (recentBanLists.Count > 0)
            {
                menuRecentBanList.Items.Add(new Separator());
                var clearBanListItem = new MenuItem
                {
                    Header = CMess.ClearHistory.ToText(),
                };
                clearBanListItem.Click += ClearBanListItem_Click;
                menuRecentBanList.Items.Add(clearBanListItem);
            }

            /////////////////
            menuRecentOpen.Visibility = (
                menuRecentArchive.Items.Count > 0 ||
                menuRecentDatabases.Items.Count > 0 ||
                menuRecentScripts.Items.Count > 0 ||
                menuRecentDeck.Items.Count > 0 ||
                menuRecentBanList.Items.Count > 0)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void LoadSeriesImage()
        {
            string cardImageFolderPath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "CardImage");

            if (!System.IO.Directory.Exists(cardImageFolderPath))
            {
                if (!PropertiesSettingService.GetBoolSetting("AskDownLoadCardImage")) return;

                int choose = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    CMess.quesDownloadCardImage.ToText(), new[] { CMess.yes.ToText(), CMess.no.ToText(), CMess.noAsk.ToText() });
                if (choose == 2)
                {
                    Properties.Settings.Default.AskDownLoadCardImage = false;
                    Properties.Settings.Default.Save();
                    return;
                }
                else if (choose == 1)
                {
                    return;
                }
                else if (choose == 0)
                {
                    try
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        string CardImageURL = ConfigurationManager.AppSettings["CardImageURL"];
                        string CardImagePath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "CardImage");

                        var downloader = new GithubReleaseDownloader();
                        var (success, message) = await downloader.DownloadGithubRelease(CardImageURL, "CardImage.zip", CardImagePath);

                        if (success)
                        {
                            Mouse.OverrideCursor = null;
                            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                                $"[Card Image] {CMess.updateCompe.ToText()}", new[] { CMess.ok.ToText() });
                        }
                        else
                        {
                            CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.load.ToText(), CMess.Data.ToText())} {message}"
                                , new[] { CMess.ok.ToText() });
                            return;
                        }
                    }
                    catch (HttpRequestException netEx)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"Network connection error or unable to download from GitHub:\n{netEx.Message}", new[] { CMess.ok.ToText() });
                        return;
                    }
                    catch (Exception ex)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                        return;
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
            }

            var (resultSeries, messageSeries) = GeneraImageViewModel.Instance.LoadSeriesList();
            if (!resultSeries)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageSeries}", new[] { CMess.ok.ToText() });
            }
        }

        private Point ParsePoint(string pointString)
        {
            try
            {
                string[] parts = pointString.Split(',');
                if (parts.Length == 2 &&
                    double.TryParse(parts[0], out double x) &&
                    double.TryParse(parts[1], out double y))
                {
                    return new Point(x, y);
                }
            }
            catch
            {
                // Nếu parse thất bại, trả về giá trị mặc định
            }
            return new Point(double.NaN, double.NaN);
        }
        private void BorderLeftWindowCommands_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        #endregion

        #region Menu

        #region File

        #region New
        private async void CommandNewDatabase_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await NewDatabaseCommand();
        }
        private async void CommandNewExcel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await NewExcelCommand();
        }
        private async void CommandNewCeds_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await NewCedsCommand();
        }
        private void CommandNewScript_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewScriptCommand();
        }
        private void CommandNewDeck_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewDeckCommand();
        }
        private void CommandNewBanList_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewBanListCommand();
        }
        #endregion

        #region Open
        private async void CommandOpenArchive_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string archivePath = e.Parameter as string ?? string.Empty;

            OpenArchiveWindow openArchiveWindow = new OpenArchiveWindow(archivePath);
            openArchiveWindow.ShowInTaskbar = false;
            openArchiveWindow.Owner = this;
            openArchiveWindow.MainWindowReference = this;
            openArchiveWindow.ShowDialog();

        }
        private async void CommandOpenDatabase_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string url = e.Parameter as string;
            await OpenDatabaseCommand(url);
        }
        private async void CommandOpenScript_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string url = e.Parameter as string;
            await OpenScriptCommand(url);
        }
        private async void CommandOpenDeck_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string url = e.Parameter as string;
            await OpenDeckCommand(url);
        }
        private async void CommandOpenBanList_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string url = e.Parameter as string;
            await OpenBanListCommand(url);
        }
        #endregion

        #region Recently Opened
        private void ClearArchiveItem_Click(object sender, RoutedEventArgs e)
        {
            ClearRecentArchive();
        }
        private void ClearDBItem_Click(object sender, RoutedEventArgs e)
        {
            ClearRecentDatabases();
        }
        private void ClearScriptItem_Click(object sender, RoutedEventArgs e)
        {
            ClearRecentScripts();
        }
        private void ClearDeckItem_Click(object sender, RoutedEventArgs e)
        {
            ClearRecentDeck();
        }
        private void ClearBanListItem_Click(object sender, RoutedEventArgs e)
        {
            ClearRecentBanList();
        }
        private void ClearRecentArchive()
        {
            var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                string.Format(CMess.confirmClearHistory.ToText(), CMess.CardDB.ToText()),
                new[] { CMess.yes.ToText(), CMess.no.ToText() });
            if (result == 0)
            {
                OpenHistoryViewModel.Instance.ClearRecentItems(0);
                LoadRecentMenuItems();
            }
        }
        private void ClearRecentDatabases()
        {
            var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                string.Format(CMess.confirmClearHistory.ToText(), CMess.CardDB.ToText()),
                new[] { CMess.yes.ToText(), CMess.no.ToText() });
            if (result == 0)
            {
                OpenHistoryViewModel.Instance.ClearRecentItems(1);
                LoadRecentMenuItems();
            }
        }
        private void ClearRecentScripts()
        {
            var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                string.Format(CMess.confirmClearHistory.ToText(), CMess.CardScript.ToText()),
                new[] { CMess.yes.ToText(), CMess.no.ToText() });
            if (result == 0)
            {
                OpenHistoryViewModel.Instance.ClearRecentItems(2);
                LoadRecentMenuItems();
            }
        }
        private void ClearRecentDeck()
        {
            var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                string.Format(CMess.confirmClearHistory.ToText(), CMess.Deck.ToText()),
                new[] { CMess.yes.ToText(), CMess.no.ToText() });
            if (result == 0)
            {
                OpenHistoryViewModel.Instance.ClearRecentItems(3);
                LoadRecentMenuItems();
            }
        }
        private void ClearRecentBanList()
        {
            var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                string.Format(CMess.confirmClearHistory.ToText(), CMess.BanList.ToText()),
                new[] { CMess.yes.ToText(), CMess.no.ToText() });
            if (result == 0)
            {
                OpenHistoryViewModel.Instance.ClearRecentItems(4);
                LoadRecentMenuItems();
            }
        }
        #endregion

        #region Save
        private async void menuSaveDB_Click(object sender, RoutedEventArgs e)
        {
            await SaveDatabase();
        }
        private async void menuSaveCeds_Click(object sender, RoutedEventArgs e)
        {
            await SaveExcel();
        }
        private async void menuSaveExcel_Click(object sender, RoutedEventArgs e)
        {
            await SaveCeds();
        }

        private async void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await CommandSaveExecuted();
        }
        #endregion

        #region Save As

        #region Save As DataEditor

        #region Card Database
        private async void menuSaveAsCDBSelectedCard_Click(object sender, RoutedEventArgs e)
        {
            await SaveAsCdbFile(ScopeCard.SelectedCards);
        }
        private async void menuSaveAsCDBFiltedCard_Click(object sender, RoutedEventArgs e)
        {
            await SaveAsCdbFile(ScopeCard.FiltedCards);
        }
        private async void menuSaveAsCDBAllCard_Click(object sender, RoutedEventArgs e)
        {
            await SaveAsCdbFile(ScopeCard.AllCards);
        }
        #endregion

        #region Excel
        private async void menuSaveAsXLSXSelectedCard_Click(object sender, RoutedEventArgs e)
        {
            await SaveAsXlsxFile(ScopeCard.SelectedCards);
        }
        private async void menuSaveAsXLSXFiltedCard_Click(object sender, RoutedEventArgs e)
        {
            await SaveAsXlsxFile(ScopeCard.FiltedCards);
        }
        private async void menuSaveAsXLSXAllCard_Click(object sender, RoutedEventArgs e)
        {
            await SaveAsXlsxFile(ScopeCard.AllCards);
        }
        #endregion

        #region Ceds
        private async void mmenuSaveAsCEDSSelectedCard_Click(object sender, RoutedEventArgs e)
        {
            await SaveAsCedsFile(ScopeCard.SelectedCards);
        }
        private async void menuSaveAsCEDSFiltedCard_Click(object sender, RoutedEventArgs e)
        {
            await SaveAsCedsFile(ScopeCard.FiltedCards);
        }
        private async void menuSaveAsCEDSAllCard_Click(object sender, RoutedEventArgs e)
        {
            await SaveAsCedsFile(ScopeCard.AllCards);
        }
        #endregion

        #endregion

        #region Save As Non-DataEditor
        private async void CommandSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await SaveAsExecuted();
        }
        #endregion

        #endregion

        #region Exit
        private void menuexit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        private async void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            if (Tabs != null && Tabs.Count > 1)
            {
                var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    $"{string.Format(CMess.NumberCloseTab.ToText(), Tabs.Count)} {CMess.QuestContinue.ToText()}",
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0)
                {
                    e.Cancel = true;
                    return;
                }
            }

            var unsavedTabs = Tabs.Where(tab => tab.Content is ISaveable saveable && !saveable.IsSaved).ToList();
            if (unsavedTabs.Any())
            {
                var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    $"{CMess.HasUnSaveData.ToText()} {CMess.QuestSaveChange.ToText()}",
                    new[] { CMess.Save.ToText(), CMess.noSave.ToText(), CMess.cancel.ToText() });

                if (result == 0)
                {
                    foreach (var tab in unsavedTabs)
                    {
                        if (tab.Content is ISaveable saveable)
                        {
                            bool saveSuccess = await saveable.Save();
                            if (!saveSuccess)
                            {
                                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Error,
                                    string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.Card.ToText()), new[] { CMess.ok.ToText() });
                                e.Cancel = true;
                                return;
                            }
                        }
                    }
                }
                else if (result == 1)
                {
                    ///
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (!isShuttingDown)
            {
                double left = Canvas.GetLeft(ChatIcon);
                double top = Canvas.GetTop(ChatIcon);

                double right = canvas.ActualWidth - left - ChatIcon.ActualWidth;
                ConfigViewModel.Instance.displaySetting.ButtonChat = $"{right},{top}";
                ConfigViewModel.Instance.SaveDisplaySettingFile();
            }
        }
        #endregion

        #endregion

        #region Windows
        private void menuimgeditor_Click(object sender, RoutedEventArgs e)
        {
            var newimageEditor = new ImageEditor(this);
            var newTab = new TabContent
            {
                Header = CMess.ImageEdit.ToText(),
                Content = newimageEditor
            };

            Tabs.Add(newTab);
            TabControlMain.SelectedItem = newTab;

            // DataViewModel.Instance.Tabs.Add(newTab);
            // DataViewModel.Instance.SelectedTab = newTab;
            // TabControlMain.SelectedItem = newTab;
        }
        private void menudataeditor_Click(object sender, RoutedEventArgs e)
        {
            var newdataEditor = new DataEditor(this);
            var newTab = new TabContent
            {
                Header = CMess.DataEdit.ToText(),
                Content = newdataEditor
            };

            Tabs.Add(newTab);
            TabControlMain.SelectedItem = newTab;
        }
        private void menuomegadataeditor_Click(object sender, RoutedEventArgs e)
        {
            var newdataEditor = new OmegaDataEditor(this);
            var newTab = new TabContent
            {
                Header = CMess.DataEdit.ToText(),
                Content = newdataEditor
            };

            Tabs.Add(newTab);
            TabControlMain.SelectedItem = newTab;
        }
        private void menudeckeditor_Click(object sender, RoutedEventArgs e)
        {
            var newdeckEditor = new DeckEditor(this);
            var newTab = new TabContent
            {
                Header = CMess.DeckEdit.ToText(),
                Content = newdeckEditor
            };

            Tabs.Add(newTab);
            TabControlMain.SelectedItem = newTab;
        }
        private void menucodeeditor_Click(object sender, RoutedEventArgs e)
        {
            var newcodeEditor = new CodeEditor(this);
            var newTab = new TabContent
            {
                Header = CMess.CodeEdit.ToText(),
                Content = newcodeEditor
            };
            Tabs.Add(newTab);
            TabControlMain.SelectedItem = newTab;
        }
        private void menubanlisteditor_Click(object sender, RoutedEventArgs e)
        {
            var newbanlistEditor = new BanListEditor(this);
            var newTab = new TabContent
            {
                Header = "BanList Editor",
                Content = newbanlistEditor
            };
            Tabs.Add(newTab);
            TabControlMain.SelectedItem = newTab;
        }
        private void menuchatbot_Click(object sender, RoutedEventArgs e)
        {
            if (blMainChat.Width != 0)
            {
                OpenChatTab();
            }
            else
            {
                CloseChatTab();
            }
        }
        private void menuScriptSupport_Click(object sender, RoutedEventArgs e)
        {
            OpemScriptSupportWindow();
        }
        #endregion

        #region Settings
        private void CommandSetting_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SettingCommand();
        }
        #endregion

        #region Card

        #region Copy Card
        private async void MenuItemCopySelected_Click(object sender, RoutedEventArgs e)
        {
            await CopySelectedCard();
        }
        private async void menuCopyFilted_Click(object sender, RoutedEventArgs e)
        {
            await CopyFiltedCard();
        }
        private async void MenuItemCopyAll_Click(object sender, RoutedEventArgs e)
        {
            await CopyAllCard();
        }
        #endregion

        #region paste Card
        private async void MenuItemPaste_Click(object sender, RoutedEventArgs e)
        {
            await PasteCard();
        }
        #endregion

        #region Filter Card
        private void menuItemFilterCard_Click(object sender, RoutedEventArgs e)
        {
            OpenFilterCard();
        }
        #endregion

        #region Create Image
        private void menuCreateImage_Click(object sender, RoutedEventArgs e)
        {
            OpenCreateImage();
        }
        #endregion

        #endregion

        #region Data

        #region Export Zip
        private async void exportzipSelected_Click(object sender, RoutedEventArgs e)
        {
            await ExportZipCard(ScopeCard.SelectedCards);
        }
        private async void exportzipFilted_Click(object sender, RoutedEventArgs e)
        {
            await ExportZipCard(ScopeCard.FiltedCards);
        }
        private async void exportzipAll_Click(object sender, RoutedEventArgs e)
        {
            await ExportZipCard(ScopeCard.AllCards);
        }
        #endregion

        #region Import Data
        private void menuImport_Click(object sender, RoutedEventArgs e)
        {
            ItemsEditor itemsEditor = new ItemsEditor(ItemsEdit.ImportData);
            itemsEditor.ShowInTaskbar = false;
            itemsEditor.Owner = this;
            itemsEditor.MainWindowReference = this;
            itemsEditor.ShowDialog();
        }
        private void menuReplace_Click(object sender, RoutedEventArgs e)
        {
            ItemsEditor itemsEditor = new ItemsEditor(ItemsEdit.ReplaceDesc);
            itemsEditor.ShowInTaskbar = false;
            itemsEditor.Owner = this;
            itemsEditor.MainWindowReference = this;
            itemsEditor.ShowDialog();
        }
        private void menuItemEditor_Click(object sender, RoutedEventArgs e)
        {
            ItemsEditor itemsEditor = new ItemsEditor(ItemsEdit.ReplaceField);
            itemsEditor.ShowInTaskbar = false;
            itemsEditor.Owner = this;
            itemsEditor.MainWindowReference = this;
            itemsEditor.ShowDialog();
        }
        #endregion

        #endregion

        #region Manager
        private void menuRarity_Click(object sender, RoutedEventArgs e)
        {
            if (_rareEditor == null || !_rareEditor.IsLoaded)
            {
                _rareEditor = new RareEditor();
                _rareEditor.Owner = this;
                _rareEditor.ShowInTaskbar = true;
                _rareEditor.Closed += (s, args) => _rareEditor = null;
                _rareEditor.Show();
            }
            else
            {
                if (_rareEditor.WindowState == WindowState.Minimized)
                {
                    _rareEditor.WindowState = WindowState.Normal;
                }
                _rareEditor.Activate();
                _rareEditor.Topmost = true;
                _rareEditor.Topmost = false;
            }
        }
        private void MenuGenesys_Click(object sender, RoutedEventArgs e)
        {
            if (_genesysEditor == null || !_genesysEditor.IsLoaded)
            {
                _genesysEditor = new GenesysEditor();
                _genesysEditor.Owner = this;
                _genesysEditor.ShowInTaskbar = true;
                _genesysEditor.Closed += (s, args) => _genesysEditor = null;
                _genesysEditor.Show();
            }
            else
            {
                if (_genesysEditor.WindowState == WindowState.Minimized)
                {
                    _genesysEditor.WindowState = WindowState.Normal;
                }
                _genesysEditor.Activate();
                _genesysEditor.Topmost = true;
                _genesysEditor.Topmost = false;
            }
        }
        #endregion

        #region Help
        private void menulinter_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserControl is CodeEditor currentCodeEditor)
            {
                currentCodeEditor.CheckLua();
            }
        }

        private async void CardDataChkUpdate_Click(object sender, RoutedEventArgs e)
        {
            string CardDataURL = ConfigurationManager.AppSettings["CardDataURL"];
            string CardDataPath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "CardData");
            await UpdateOneAsync(CardDataURL, CardDataPath, "Card Data");
        }
        private async void CardImageChkUpdate_Click(object sender, RoutedEventArgs e)
        {
            await UpdateCardImage(sender);
        }

        private void RegisterRegistry_Click(object sender, RoutedEventArgs e)
        {
            RegisterRegistry();
        }
        private void UnregisterRegistry_Click(object sender, RoutedEventArgs e)
        {
            UnregisterRegistry();
        }
        #endregion

        #region About
        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowInTaskbar = false;
            about.Owner = this;
            about.Show();
        }
        #endregion

        #region Devrloper
        private void DevrloperTool_Click(object sender, RoutedEventArgs e)
        {
            if (!isDeveloper)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    CMess.noRegularUser.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            DEVWindow devWindow = new DEVWindow();
            devWindow.ShowInTaskbar = false;
            devWindow.Owner = this;
            devWindow.ShowDialog();
        }
        #endregion

        #endregion

        #region Command
        private void CommandBindingNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void CommandBindingOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // e.CanExecute = true;
            if (e.Command == Commands.CustomCommands.OpenArchive ||
                e.Command == Commands.CustomCommands.OpenDatabase ||
                e.Command == Commands.CustomCommands.OpenScript ||
                e.Command == Commands.CustomCommands.OpenDeck ||
                e.Command == Commands.CustomCommands.OpenBanList)
            {
                string url = e.Parameter as string;
                e.CanExecute = string.IsNullOrEmpty(url) || File.Exists(url);
            }
        }
        private void CommandSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (currentUserControl != null)
            {
                if (currentUserControl is ImageEditor ||
                    currentUserControl is DataEditor ||
                    currentUserControl is DeckEditor ||
                    currentUserControl is CodeEditor ||
                    currentUserControl is BanListEditor)
                {
                    e.CanExecute = true;
                }
                else e.CanExecute = false;
            }
            else e.CanExecute = false;
        }
        private void CommandSaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (currentUserControl != null)
            {
                if (currentUserControl is ImageEditor ||
                    currentUserControl is CodeEditor ||
                    currentUserControl is DeckEditor ||
                    currentUserControl is BanListEditor)
                {
                    e.CanExecute = true;
                }
            }
            else { e.CanExecute = false; }
        }
        private void CommandBindingSetting_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        #endregion

        #region File

        #region New
        private async Task<string> CreateDatabase()
        {
            try
            {
                string filePath = FileDiaLogHelper.SaveDataBase();
                if (string.IsNullOrWhiteSpace(filePath)) return string.Empty;

                bool hasFlag;
                int chooseFlag = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    CMess.QuestHasFlag.ToText(), new[] { CMess.yes.ToText(), CMess.no.ToText(), CMess.cancel.ToText() });
                if (chooseFlag == 0) hasFlag = true;
                else if (chooseFlag == 1) hasFlag = false;
                else return string.Empty;

                var (resultCreate, messageCreate) = await CreateFileServices.CreateDatabaseCommand(filePath, hasFlag);
                if (!resultCreate)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText())} {messageCreate}",
                        new[] { CMess.ok.ToText() });
                    return string.Empty;
                }
                else return messageCreate;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                       $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText())} {ex.Message}",
                       new[] { CMess.ok.ToText() });
                return string.Empty;
            }
        }
        private async Task<string> CreateExcel()
        {
            try
            {
                string filePath = FileDiaLogHelper.SaveExcel();
                if (string.IsNullOrWhiteSpace(filePath)) return string.Empty;

                bool hasFlag;
                int chooseFlag = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    CMess.QuestHasFlag.ToText(), new[] { CMess.yes.ToText(), CMess.no.ToText(), CMess.cancel.ToText() });
                if (chooseFlag == 0) hasFlag = true;
                else if (chooseFlag == 1) hasFlag = false;
                else return string.Empty;

                var (resultCreate, messageCreate) = await CreateFileServices.CreateExcelCommand(filePath, hasFlag);

                if (!resultCreate)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText())} {messageCreate}",
                        new[] { CMess.ok.ToText() });
                    return string.Empty;
                }
                else return messageCreate;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                       $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText())} {ex.Message}",
                       new[] { CMess.ok.ToText() });
                return string.Empty;
            }
        }
        private async Task<string> CreateCeds()
        {
            try
            {
                string cedsFilePath = FileDiaLogHelper.SaveCeds();
                if (string.IsNullOrEmpty(cedsFilePath)) return string.Empty;

                bool hasFlag;
                int chooseFlag = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    CMess.QuestHasFlag.ToText(), new[] { CMess.yes.ToText(), CMess.no.ToText(), CMess.cancel.ToText() });
                if (chooseFlag == 0) hasFlag = true;
                else if (chooseFlag == 1) hasFlag = false;
                else return string.Empty;

                /// _cedsFileHasFlag = hasFlag;

                var (resultCreate, messageCreate) = await CreateFileServices.CreateCedsCommand(cedsFilePath);
                if (!resultCreate)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText())} {messageCreate}",
                        new[] { CMess.ok.ToText() });
                    return string.Empty;
                }
                else return messageCreate;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                       $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText())} {ex.Message}",
                       new[] { CMess.ok.ToText() });
                return string.Empty;
            }
        }
        private async Task<string> CreateScript(bool newScript)
        {
            try
            {
                string filePath = FileDiaLogHelper.SaveScript();
                if (string.IsNullOrWhiteSpace(filePath)) return string.Empty;

                var (resultCreate, messageCreate) = CreateFileServices.CreateScriptCommand(filePath, newScript);
                if (!resultCreate)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText())} {messageCreate}",
                        new[] { CMess.ok.ToText() });
                    return string.Empty;
                }
                else return messageCreate;

                //if (!string.IsNullOrEmpty(filePath))
                //{
                //    string scriptFilePath = filePath;
                //    string[] validExtensions = { ".lua", ".txt", ".md", ".log", ".ydk", ".yml", ".conf" };
                //    string extension = System.IO.Path.GetExtension(scriptFilePath).ToLower();

                //    if (string.IsNullOrEmpty(extension) || !validExtensions.Contains(extension))
                //    {
                //        scriptFilePath += ".lua";
                //    }
                //    if (System.IO.File.Exists(scriptFilePath))
                //    {
                //        var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                //            $"{CMess.filealreadyExit.ToText()} {CMess.QuestOverwrite.ToText()}",
                //            new[] { CMess.yes.ToText(), CMess.no.ToText() });
                //        if (result != 0) return (string.Empty);
                //    }

                //    string folderPath = System.IO.Path.GetDirectoryName(scriptFilePath);
                //    string scriptFileName = System.IO.Path.GetFileName(scriptFilePath);
                //    if (!System.IO.Directory.Exists(folderPath))
                //    {
                //        System.IO.Directory.CreateDirectory(folderPath);
                //    }
                //    var (resultCreate, messageCreate) = await Task.Run(() => CreateFileServices.CreateScript(folderPath, scriptFileName, newScript));
                //    if (resultCreate) return messageCreate;
                //    else
                //    {
                //        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                //            $"{CMess.errorOcc.ToText()} {messageCreate}", new[] { CMess.ok.ToText() });
                //        return string.Empty;
                //    }
                //}
                //else return string.Empty;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return string.Empty;
            }
        }
        private string CreateDeck()
        {
            try
            {
                string filePath = FileDiaLogHelper.SaveDeck();

                if (!string.IsNullOrEmpty(filePath))
                {
                    string deckFilePath = filePath;

                    string[] validExtensions = { ".ydk" };
                    string extension = System.IO.Path.GetExtension(deckFilePath).ToLower();
                    if (string.IsNullOrEmpty(extension) || !validExtensions.Contains(extension))
                        deckFilePath += ".ydk";

                    var (result, message) = DeckViewModel.Instance.CreateNewDeckFile(
                        System.IO.Path.GetFileNameWithoutExtension(deckFilePath),
                        deckFilePath, System.IO.Path.GetDirectoryName(deckFilePath));
                    if (!result)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                        return string.Empty;
                    }
                    else return message;
                }
                else return string.Empty;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return string.Empty;
            }
        }
        private string CreateBanList()
        {
            try
            {
                string filePath = FileDiaLogHelper.SaveBanList();

                if (string.IsNullOrWhiteSpace(filePath)) return string.Empty;

                string deckFilePath = filePath;

                string[] validExtensions = { ".lflist.conf" };
                string extension = System.IO.Path.GetExtension(deckFilePath).ToLower();
                if (string.IsNullOrEmpty(extension) || !validExtensions.Contains(extension))
                    deckFilePath += ".lflist.conf";

                using (FileStream fs = File.Create(deckFilePath)) { }
                if (File.Exists(deckFilePath)) return deckFilePath;
                return string.Empty;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return string.Empty;
            }
        }

        private async Task NewDatabaseCommand()
        {
            string dbFilePath = await CreateDatabase();
            if (string.IsNullOrEmpty(dbFilePath)) return;

            var result = CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"{string.Format(CMess.FourPlaceholderSuccess.ToText(), System.IO.Path.GetFileName(dbFilePath), CMess.CardDB.ToText(), CMess.File.ToText(), CMess.Create.ToText())}\n{CMess.QuestOpen.ToText()}",
                // FileName Card Database created successfully! Do you want to open it?
                new[] { CMess.yes.ToText(), CMess.no.ToText() });
            if (result != 0) return;

            string cdbFileName = System.IO.Path.GetFileName(dbFilePath);

            try
            {
                if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor &&
                    string.IsNullOrWhiteSpace(currentDataEditor.cdbFilePath) &&
                    string.IsNullOrWhiteSpace(currentDataEditor.cedsFilePath) &&
                    string.IsNullOrWhiteSpace(currentDataEditor.xlsxFilePath))
                {
                    var currentTab = Tabs.FirstOrDefault(t => t.Content == currentUserControl);
                    if (currentTab != null)
                    {
                        currentTab.Header = System.IO.Path.GetFileName(dbFilePath);
                    }
                    await currentDataEditor.LoadFileCardList(dbFilePath);
                }
                else
                {
                    var newDataEditor = new DataEditor(this);
                    var newTab = new CardEditor.Models.TabContent
                    {
                        Header = cdbFileName,
                        Content = newDataEditor
                    };
                    Tabs.Add(newTab);
                    await Dispatcher.BeginInvoke(new Func<Task>(async () =>
                    {
                        TabControlMain.SelectedItem = newTab;
                        await newDataEditor.LoadFileCardList(dbFilePath);
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task NewExcelCommand()
        {
            string excelFilePath = await CreateExcel();
            if (string.IsNullOrEmpty(excelFilePath)) return;

            var result = CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"{string.Format(CMess.FourPlaceholderSuccess.ToText(), System.IO.Path.GetFileName(excelFilePath), CMess.Excel.ToText(), CMess.File.ToText(), CMess.Create.ToText())}\n{CMess.QuestOpen.ToText()}",
                // FileName Excel File created successfully! Do you want to open it?
                new[] { CMess.yes.ToText(), CMess.no.ToText() });
            if (result != 0) return;

            string cdbFileName = System.IO.Path.GetFileName(excelFilePath);

            try
            {
                if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor &&
                    string.IsNullOrWhiteSpace(currentDataEditor.cdbFilePath) &&
                    string.IsNullOrWhiteSpace(currentDataEditor.cedsFilePath) &&
                    string.IsNullOrWhiteSpace(currentDataEditor.xlsxFilePath))
                {
                    var currentTab = Tabs.FirstOrDefault(t => t.Content == currentUserControl);
                    if (currentTab != null)
                    {
                        currentTab.Header = System.IO.Path.GetFileName(excelFilePath);
                    }
                    await currentDataEditor.LoadFileCardList(excelFilePath);
                }
                else
                {
                    var newDataEditor = new DataEditor(this);
                    var newTab = new CardEditor.Models.TabContent
                    {
                        Header = cdbFileName,
                        Content = newDataEditor
                    };
                    Tabs.Add(newTab);
                    await Dispatcher.BeginInvoke(new Func<Task>(async () =>
                    {
                        TabControlMain.SelectedItem = newTab;
                        await newDataEditor.LoadFileCardList(excelFilePath);
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task NewCedsCommand()
        {
            string cedsFilePath = await CreateCeds();
            if (string.IsNullOrEmpty(cedsFilePath)) return;

            var result = CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"{string.Format(CMess.FourPlaceholderSuccess.ToText(), System.IO.Path.GetFileName(cedsFilePath), CMess.Ceds.ToText(), CMess.File.ToText(), CMess.Create.ToText())}\n{CMess.QuestOpen.ToText()}",
                // FileName Ceds File created successfully! Do you want to open it?
                new[] { CMess.yes.ToText(), CMess.no.ToText() });
            if (result != 0) return;

            string cdbFileName = System.IO.Path.GetFileName(cedsFilePath);

            try
            {
                if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor &&
                    string.IsNullOrWhiteSpace(currentDataEditor.cdbFilePath) &&
                    string.IsNullOrWhiteSpace(currentDataEditor.cedsFilePath) &&
                    string.IsNullOrWhiteSpace(currentDataEditor.xlsxFilePath))
                {
                    var currentTab = Tabs.FirstOrDefault(t => t.Content == currentUserControl);
                    if (currentTab != null)
                    {
                        currentTab.Header = System.IO.Path.GetFileName(cedsFilePath);
                    }
                    await currentDataEditor.LoadFileCardList(cedsFilePath);
                }
                else
                {
                    var newDataEditor = new DataEditor(this);
                    var newTab = new CardEditor.Models.TabContent
                    {
                        Header = cdbFileName,
                        Content = newDataEditor
                    };
                    Tabs.Add(newTab);
                    await Dispatcher.BeginInvoke(new Func<Task>(async () =>
                    {
                        TabControlMain.SelectedItem = newTab;
                        await newDataEditor.LoadFileCardList(cedsFilePath);
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async void NewScriptCommand()
        {
            string createdScriptPath = await CreateScript(true);
            if (!string.IsNullOrWhiteSpace(createdScriptPath) && System.IO.File.Exists(createdScriptPath))
            {
                var result = CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    $"{string.Format(CMess.ThreePlaceholderSuccess.ToText(), System.IO.Path.GetFileName(createdScriptPath), CMess.CardScript.ToText(), CMess.Create.ToText())}\n{CMess.QuestOpen.ToText()}",
                    // FilePath Card Script created successfully! Do you want to open it?
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result == 0)
                {
                    string scriptFileName = System.IO.Path.GetFileName(createdScriptPath);
                    try
                    {
                        if (currentUserControl != null && currentUserControl is CodeEditor currentCodeEditor &&
                            string.IsNullOrWhiteSpace(currentCodeEditor.luaFilePath) &&
                            string.IsNullOrWhiteSpace(currentCodeEditor.luaFileName))
                        {
                            var currentTab = Tabs.FirstOrDefault(t => t.Content == currentUserControl);
                            if (currentTab != null)
                            {
                                currentTab.Header = scriptFileName;
                            }
                            currentCodeEditor.luaFilePath = createdScriptPath;
                            await currentCodeEditor.LoadLuaFile();
                        }
                        else
                        {
                            var newcodeEditor = new CodeEditor(this)
                            {
                                luaFileName = scriptFileName,
                                luaFilePath = createdScriptPath,
                            };
                            var newTab = new TabContent { Header = scriptFileName, Content = newcodeEditor };
                            Tabs.Add(newTab);
                            await Dispatcher.BeginInvoke(new Func<Task>(async () =>
                            {
                                TabControlMain.SelectedItem = newTab;
                                newcodeEditor.luaFilePath = createdScriptPath;
                                await newcodeEditor.LoadLuaFile();
                            }), System.Windows.Threading.DispatcherPriority.Render);
                        }
                    }
                    catch (Exception ex)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                    }
                }
            }
        }
        private void NewDeckCommand()
        {
            string deckFilePath = CreateDeck();
            if (!string.IsNullOrEmpty(deckFilePath) && System.IO.File.Exists(deckFilePath))
            {
                var result = CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    $"{string.Format(CMess.ThreePlaceholderSuccess.ToText(), System.IO.Path.GetFileName(deckFilePath), CMess.Deck.ToText(), CMess.Create.ToText())}\n{CMess.QuestOpen.ToText()}",
                    // FileName Card Database created successfully! Do you want to open it?
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result == 0)
                {
                    try
                    {
                        CardEditor.Models.Deck newDeck = new CardEditor.Models.Deck
                        {
                            Path = deckFilePath
                        };
                        DeckViewModel.Instance.Decks.Add(newDeck);

                        if (currentUserControl != null && currentUserControl is DeckEditor currentDeckEditor &&
                            string.IsNullOrWhiteSpace(currentDeckEditor.CurrentDeckPath))
                        {
                            currentDeckEditor.CurrentDeck = newDeck;
                        }
                        else
                        {
                            var newDeckEditor = new DeckEditor(this);
                            var newTab = new TabContent
                            {
                                Header = System.IO.Path.GetFileName(deckFilePath),
                                Content = newDeckEditor
                            };
                            Tabs.Add(newTab);
                            TabControlMain.SelectedItem = newTab;
                            Dispatcher.Yield();
                            newDeckEditor.CurrentDeck = newDeck;
                        }
                    }
                    catch (Exception ex)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                    }
                }
            }
        }
        private void NewBanListCommand()
        {
            string banListFilePath = CreateBanList();
            if (!string.IsNullOrEmpty(banListFilePath) && System.IO.File.Exists(banListFilePath))
            {
                var result = CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    $"{string.Format(CMess.ThreePlaceholderSuccess.ToText(), System.IO.Path.GetFileName(banListFilePath), CMess.BanList.ToText(), CMess.Create.ToText())}\n{CMess.QuestOpen.ToText()}",
                    // FileName Card Database created successfully! Do you want to open it?
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result == 0)
                {
                    try
                    {
                        CardEditor.Models.BanList newBanList = new CardEditor.Models.BanList
                        {
                            FilePath = banListFilePath,
                            FileName = System.IO.Path.GetFileName(banListFilePath),
                            Name = string.Empty,
                            CardList = new Dictionary<ulong, CardBanList>(),
                            WhiteList = false
                        };
                        BanListRawDataViewModel.Instance.BanLists.Add(newBanList);

                        if (currentUserControl != null && currentUserControl is BanListEditor currentBanListEditor &&
                            string.IsNullOrWhiteSpace(currentBanListEditor.CurrentBanListPath))
                        {
                            currentBanListEditor.SelectedBanList = newBanList;
                        }
                        else
                        {
                            var newBanListEditor = new BanListEditor(this);
                            var newTab = new TabContent
                            {
                                Header = System.IO.Path.GetFileName(banListFilePath),
                                Content = newBanListEditor
                            };
                            Tabs.Add(newTab);
                            TabControlMain.SelectedItem = newTab;
                            Dispatcher.Yield();
                            newBanListEditor.SelectedBanList = newBanList;
                        }
                    }
                    catch (Exception ex)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                    }
                }
            }
        }
        #endregion

        #region Open
        public (bool, List<CardEditor.Models.FileItem>) SelectFileToOpen(IReadOnlyList<(string fullPath, string archiveFilePath, string archiveEntryName)> entries)
        {
            var selectFileWindow = new SelectFileWindow(entries) { ShowInTaskbar = false, Owner = this };
            bool? result = selectFileWindow.ShowDialog();
            return result == true
                ? (true, selectFileWindow.Result)
                : (false, new List<CardEditor.Models.FileItem>());
        }
        public (bool, List<CardEditor.Models.FileItem>) SelectFileToOpen(IReadOnlyList<string> filePaths)
        {
            var entries = filePaths?.Select(p => (p, (string)null, (string)null)).ToList()
                ?? new List<(string, string, string)>();
            return SelectFileToOpen(entries);
        }
        public void UpdateArchiveRecentItem(string url = null)
        {
            if (string.IsNullOrEmpty(url) || !System.IO.File.Exists(url)) return;
            OpenHistoryViewModel.Instance.UpdateRecentItem(url, 0);
            LoadRecentMenuItems();
        }

        #region Open Commands
        private async Task OpenArchiveCommand(string url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                string filePath = FileDiaLogHelper.OpenCardPack();
                if (!string.IsNullOrEmpty(filePath))
                {
                    url = filePath;
                }
            }
            if (!string.IsNullOrEmpty(url) && File.Exists(url))
            {
                OpenArchiveWindow openArchiveWindow = new OpenArchiveWindow(url);
                openArchiveWindow.ShowInTaskbar = false;
                openArchiveWindow.Owner = this;
                openArchiveWindow.MainWindowReference = this;
                openArchiveWindow.ShowDialog();

                openArchiveWindow.ArchiveFilePath = url;
                await openArchiveWindow.LoadArchiveFile();
            }
        }
        private async Task OpenDatabaseCommand(string url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                string filePath = FileDiaLogHelper.OpenDataBase();
                if (!string.IsNullOrEmpty(filePath))
                {
                    url = filePath;
                }
            }
            if (!string.IsNullOrEmpty(url) && File.Exists(url))
            {
                await OpenDataBase(url);
                OpenHistoryViewModel.Instance.UpdateRecentItem(url, 1);
                LoadRecentMenuItems();
            }
        }
        private async Task OpenScriptCommand(string url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                string filePath = FileDiaLogHelper.OpenScript();
                if (!string.IsNullOrEmpty(filePath))
                {
                    url = filePath;
                }
            }
            if (!string.IsNullOrEmpty(url) && File.Exists(url))
            {
                await OpenScript(url);
                OpenHistoryViewModel.Instance.UpdateRecentItem(url, 2);
                LoadRecentMenuItems();
            }
        }
        private async Task OpenDeckCommand(string url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                string filePath = FileDiaLogHelper.OpenDeck();

                if (!string.IsNullOrEmpty(filePath))
                {
                    url = filePath;
                }
            }
            if (!string.IsNullOrEmpty(url) && File.Exists(url))
            {
                await OpenDeck(url);
                OpenHistoryViewModel.Instance.UpdateRecentItem(url, 3);
                LoadRecentMenuItems();
            }
        }
        private async Task OpenBanListCommand(string url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                string filePath = FileDiaLogHelper.OpenBanList();

                if (!string.IsNullOrEmpty(filePath))
                {
                    url = filePath;
                }
            }
            if (!string.IsNullOrEmpty(url) && File.Exists(url))
            {
                await OpenBanList(url);
                OpenHistoryViewModel.Instance.UpdateRecentItem(url, 4);
                LoadRecentMenuItems();
            }
        }
        #endregion

        #region Open Archive
        public async Task<(bool, string)> OpenArchiveDatabase(string archiveFilePath, IEnumerable<string> listEntryFullName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(archiveFilePath) || !System.IO.File.Exists(archiveFilePath))
                    return (false, CMess.fileNotExit.ToText());
                if (listEntryFullName == null || !listEntryFullName.Any())
                    return (false, CMess.noFileFound.ToText());

                var extractResult = await LoadDataServices.ExtractTempEntry(archiveFilePath, listEntryFullName);

                if (!extractResult.result) return (false, extractResult.message);
                if (extractResult.extractedEntries == null || extractResult.extractedEntries.Count == 0) return (false, CMess.noFileFound.ToText());

                foreach (var enTry in extractResult.extractedEntries)
                {
                    await OpenDataBase(enTry.TempPath, archiveFilePath, enTry.EntryFullName);
                    await Dispatcher.Yield(DispatcherPriority.Background);
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(bool, string)> OpenArchiveScript(string archiveFilePath, IEnumerable<string> listEntryFullName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(archiveFilePath) || !System.IO.File.Exists(archiveFilePath))
                    return (false, CMess.fileNotExit.ToText());
                if (listEntryFullName == null || !listEntryFullName.Any())
                    return (false, CMess.noFileFound.ToText());

                var extractResult = await LoadDataServices.ExtractTempEntry(archiveFilePath, listEntryFullName);

                if (!extractResult.result) return (false, extractResult.message);
                if (extractResult.extractedEntries == null || extractResult.extractedEntries.Count == 0) return (false, CMess.noFileFound.ToText());

                foreach (var enTry in extractResult.extractedEntries)
                {
                    await OpenScript(enTry.TempPath, archiveFilePath, enTry.EntryFullName);
                    await Dispatcher.Yield(DispatcherPriority.Background);
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(bool, string)> OpenArchiveDeck(string archiveFilePath, IEnumerable<string> listEntryFullName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(archiveFilePath) || !System.IO.File.Exists(archiveFilePath))
                    return (false, CMess.fileNotExit.ToText());
                if (listEntryFullName == null || !listEntryFullName.Any())
                    return (false, CMess.noFileFound.ToText());

                var extractResult = await LoadDataServices.ExtractTempEntry(archiveFilePath, listEntryFullName);

                if (!extractResult.result) return (false, extractResult.message);
                if (extractResult.extractedEntries == null || extractResult.extractedEntries.Count == 0) return (false, CMess.noFileFound.ToText());

                List<Deck> deckList = new List<Deck>();
                foreach (var enTry in extractResult.extractedEntries)
                {
                    Deck deck = await DeckViewModel.Instance.LoadDeckFromFile(filePath: enTry.TempPath, archiveFilePath: archiveFilePath, archiveEntryName: enTry.EntryFullName);
                    if (deck != null) deckList.Add(deck);
                }
                if (deckList.Count == 0) return (false, CMess.noFileFound.ToText());
                DeckViewModel.Instance.Decks.AddRange(deckList);

                if (currentUserControl != null && currentUserControl is DeckEditor currentDeckEditor)
                {
                    currentDeckEditor.CurrentDeck = deckList[deckList.Count - 1];
                }
                else
                {
                    var newDeckEditor = new DeckEditor(this);
                    var newTab = new TabContent { Header = CMess.DeckEdit.ToText(), Content = newDeckEditor };
                    Tabs.Add(newTab);

                    await Dispatcher.BeginInvoke(new Func<Task>(async () =>
                    {
                        TabControlMain.SelectedItem = newTab;
                        await Task.Yield();
                        newDeckEditor.CurrentDeck = deckList[deckList.Count - 1];
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(bool, string)> OpenArchiveBanList(string archiveFilePath, IEnumerable<string> listEntryFullName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(archiveFilePath) || !System.IO.File.Exists(archiveFilePath))
                    return (false, CMess.fileNotExit.ToText());
                if (listEntryFullName == null || !listEntryFullName.Any())
                    return (false, CMess.noFileFound.ToText());

                var extractResult = await LoadDataServices.ExtractTempEntry(archiveFilePath, listEntryFullName);

                if (!extractResult.result) return (false, extractResult.message);
                if (extractResult.extractedEntries == null || extractResult.extractedEntries.Count == 0) return (false, CMess.noFileFound.ToText());

                List<BanList> banlists = new List<BanList>();
                foreach (var entry in extractResult.extractedEntries)
                {
                    BanList banlist = await BanListRawDataViewModel.Instance.LoadFileBanList(filePath: entry.TempPath, archiveFilePath: archiveFilePath, archiveEntryName: entry.EntryFullName);
                    if (banlist != null) banlists.Add(banlist);
                }
                if (banlists.Count == 0) return (false, CMess.noFileFound.ToText());
                BanListRawDataViewModel.Instance.BanLists.AddRange(banlists);

                if (currentUserControl != null && currentUserControl is BanListEditor currentBanlistEditor)
                {
                    currentBanlistEditor.SelectedBanList = banlists[banlists.Count - 1];
                }
                else
                {
                    var newBanlistEditor = new BanListEditor(this);
                    var newTab = new TabContent { Header = CMess.BanListEdit.ToText(), Content = newBanlistEditor };
                    Tabs.Add(newTab);

                    await Dispatcher.BeginInvoke(new Func<Task>(async () =>
                    {
                        TabControlMain.SelectedItem = newTab;
                        await Task.Yield();
                        newBanlistEditor.SelectedBanList = banlists[banlists.Count - 1];
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region Open Methods
        public async Task<(bool, string)> OpenDataBase(string selectedFilePath, string archiveFilePath = null, string archiveEntryName = null)
        {
            if (string.IsNullOrWhiteSpace(selectedFilePath) || !File.Exists(selectedFilePath)) return (false, CMess.fileNotExit.ToText());

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                LoadCardDataResult resultCard = Path.GetExtension(selectedFilePath).ToLowerInvariant() switch
                {
                    var ext when ConstantExtension.CardDBExtensions.Contains(ext) => await LoadDataServices.LoadDatabaseCard(selectedFilePath),
                    var ext when ConstantExtension.ExcelExtensions.Contains(ext) => await LoadDataServices.LoadExcelCard(selectedFilePath),
                    var ext when ConstantExtension.CedsExtensions.Contains(ext) => await LoadDataServices.LoadCedsCard(selectedFilePath),
                    _ => new LoadCardDataResult { Result = false, Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()) }
                };

                if (resultCard.Result && resultCard.CardList != null)
                {
                    string fileName = System.IO.Path.GetFileName(selectedFilePath);
                    if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor &&
                        string.IsNullOrWhiteSpace(currentDataEditor.cdbFilePath) &&
                        string.IsNullOrWhiteSpace(currentDataEditor.cedsFilePath) &&
                        string.IsNullOrWhiteSpace(currentDataEditor.xlsxFilePath))
                    {
                        var currentTab = Tabs.FirstOrDefault(t => t.Content == currentUserControl);
                        if (currentTab != null)
                        {
                            currentTab.Header = System.IO.Path.GetFileName(selectedFilePath);
                        }
                        currentDataEditor.archiveFilePath = archiveFilePath;
                        currentDataEditor.archiveEntryName = archiveEntryName;
                        switch (Path.GetExtension(selectedFilePath).ToLowerInvariant())
                        {
                            case var ext when ConstantExtension.CardDBExtensions.Contains(ext):
                                currentDataEditor.cdbFilePath = selectedFilePath;
                                currentDataEditor._cdbFileHasFlag = resultCard.HasFlag;
                                break;

                            case var ext when ConstantExtension.ExcelExtensions.Contains(ext):
                                currentDataEditor.xlsxFilePath = selectedFilePath;
                                currentDataEditor._xlsxFileHasFlag = resultCard.HasFlag;
                                break;

                            case var ext when ConstantExtension.CedsExtensions.Contains(ext):
                                currentDataEditor.cedsFilePath = selectedFilePath;
                                currentDataEditor._cedsFileHasFlag = resultCard.HasFlag;
                                break;
                        }
                        currentDataEditor.ImportAppendwrite(resultCard.CardList);
                        currentDataEditor.IsSaved = true;
                    }
                    else
                    {
                        var newDataEditor = new DataEditor(this);
                        var newTab = new TabContent
                        {
                            Header = fileName,
                            Content = newDataEditor
                        };
                        Tabs.Add(newTab);

                        await Dispatcher.BeginInvoke(new Func<Task>(async () =>
                        {
                            TabControlMain.SelectedItem = newTab;
                            newDataEditor.archiveFilePath = archiveFilePath;
                            newDataEditor.archiveEntryName = archiveEntryName;
                            switch (Path.GetExtension(selectedFilePath).ToLowerInvariant())
                            {
                                case var ext when ConstantExtension.CardDBExtensions.Contains(ext):
                                    newDataEditor.cdbFilePath = selectedFilePath;
                                    newDataEditor._cdbFileHasFlag = resultCard.HasFlag;
                                    break;
                                
                                case var ext when ConstantExtension.ExcelExtensions.Contains(ext):
                                    newDataEditor.xlsxFilePath = selectedFilePath;
                                    newDataEditor._xlsxFileHasFlag = resultCard.HasFlag;
                                    break;
                                case var ext when ConstantExtension.CedsExtensions.Contains(ext):
                                    newDataEditor.cedsFilePath = selectedFilePath;
                                    newDataEditor._cedsFileHasFlag = resultCard.HasFlag;
                                    break;
                            }
                            newDataEditor.ImportCreateNew(resultCard.CardList);
                        }), System.Windows.Threading.DispatcherPriority.Render);

                        newDataEditor.IsSaved = true;
                    }

                    return (true, string.Empty);
                }
                else return (false, resultCard.Message);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        public async Task<(bool, string)> OpenScript(string selectedFilePath, string archiveFilePath = null, string archiveEntryName = null)
        {
            if (string.IsNullOrWhiteSpace(selectedFilePath) || !File.Exists(selectedFilePath)) return (false, CMess.fileNotExit.ToText());

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                string fileName = System.IO.Path.GetFileName(selectedFilePath);

                if (currentUserControl != null && currentUserControl is CodeEditor currentCodeEditor &&
                string.IsNullOrWhiteSpace(currentCodeEditor.luaFilePath) &&
                string.IsNullOrWhiteSpace(currentCodeEditor.luaFileName))
                {
                    var currentTab = Tabs.FirstOrDefault(t => t.Content == currentUserControl);
                    if (currentTab != null)
                    {
                        currentTab.Header = fileName;
                    }
                    currentCodeEditor.luaFilePath = selectedFilePath;
                    currentCodeEditor.luaFileName = fileName;
                    currentCodeEditor.archiveFilePath = archiveFilePath;
                    currentCodeEditor.archiveEntryName = archiveEntryName;

                    await currentCodeEditor.LoadLuaFile();
                }
                else
                {
                    var newCodeEditor = new CodeEditor(this);
                    var newTab = new TabContent
                    {
                        Header = fileName,
                        Content = newCodeEditor
                    };
                    Tabs.Add(newTab);

                    await Dispatcher.BeginInvoke(new Func<Task>(async () =>
                    {
                        TabControlMain.SelectedItem = newTab;
                        newCodeEditor.archiveFilePath = archiveFilePath;
                        newCodeEditor.archiveEntryName = archiveEntryName;
                        newCodeEditor.luaFilePath = selectedFilePath;
                        newCodeEditor.luaFileName = fileName;
                        await newCodeEditor.LoadLuaFile();
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        private async Task OpenDeck(string selectedFilePath)
        {
            try
            {
                if (!CardEXDataViewModel.Instance.IsLoadedCard)
                    await CardEXDataViewModel.Instance.LoadCardsEXAsync();
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return;
            }

            string fileName = System.IO.Path.GetFileName(selectedFilePath);
            try
            {
                Deck deck = await DeckViewModel.Instance.LoadDeckFromFile(selectedFilePath);
                if (deck != null)
                {
                    DeckViewModel.Instance.Decks.Add(deck);
                    if (currentUserControl != null && currentUserControl is DeckEditor currentDeckeditor &&
                        string.IsNullOrWhiteSpace(currentDeckeditor.CurrentDeckPath))
                    {
                        var currentTab = Tabs.FirstOrDefault(t => t.Content == currentUserControl);
                        if (currentTab != null)
                        {
                            currentTab.Header = fileName;
                        }
                        currentDeckeditor.CurrentDeck = deck;
                    }
                    else
                    {
                        var newDeckEditor = new DeckEditor(this);
                        var newTab = new TabContent { Header = fileName, Content = newDeckEditor };
                        Tabs.Add(newTab);

                        await Dispatcher.BeginInvoke(new Func<Task>(async () =>
                        {
                            TabControlMain.SelectedItem = newTab;
                            await Task.Yield();
                            newDeckEditor.CurrentDeck = deck;
                        }), System.Windows.Threading.DispatcherPriority.Render);
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task OpenBanList(string selectedFilePath)
        {
            try
            {
                if (!BanListRawDataViewModel.Instance.IsLoaded)
                {
                    var (resultLoad, messageLoad) = await BanListRawDataViewModel.Instance.LoadBanLists();
                    if (!resultLoad)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {messageLoad}", new[] { CMess.ok.ToText() });
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }

            string fileName = System.IO.Path.GetFileName(selectedFilePath);
            try
            {
                BanList banList = await BanListRawDataViewModel.Instance.LoadFileBanList(selectedFilePath);
                if (banList != null)
                {
                    BanListRawDataViewModel.Instance.BanLists.Add(banList);
                    if (currentUserControl != null && currentUserControl is BanListEditor currentBanListEditor &&
                        string.IsNullOrWhiteSpace(currentBanListEditor.CurrentBanListPath))
                    {
                        var currentTab = Tabs.FirstOrDefault(t => t.Content == currentUserControl);
                        if (currentTab != null)
                        {
                            currentTab.Header = fileName;
                        }
                        currentBanListEditor.SelectedBanList = banList;
                    }
                    else
                    {
                        var newBanListEditor = new BanListEditor(this);
                        var newTab = new TabContent { Header = fileName, Content = newBanListEditor };
                        Tabs.Add(newTab);

                        await Dispatcher.BeginInvoke(new Func<Task>(async () =>
                        {
                            TabControlMain.SelectedItem = newTab;
                            await Task.Yield();
                            newBanListEditor.SelectedBanList = banList;
                        }), System.Windows.Threading.DispatcherPriority.Render);
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region Open DataEditor Tab
        public async Task OpenDataEditorTab(IEnumerable<string> filePaths, ulong id)
        {
            foreach (var file in filePaths)
            {
                if (string.IsNullOrWhiteSpace(file) || !File.Exists(file)) continue;
                await OpenDataEditorTab(file, id);
                await Dispatcher.Yield(DispatcherPriority.Background);
            }
        }
        public async Task OpenDataEditorTab(string filePath, ulong id)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath) || Tabs == null) return;

            var existingTab = Tabs.FirstOrDefault(t => t.Content is DataEditor dataEditor && dataEditor.cdbFilePath == filePath);
            if (existingTab != null)
            {
                TabControlMain.SelectedItem = existingTab;
                if (existingTab.Content is DataEditor dataEditor)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        dataEditor.FocusCardById(id);
                    }, DispatcherPriority.Render);
                }
            }
            else
            {
                var dataEditor = new DataEditor(this) { };

                var newTab = new TabContent { Header = System.IO.Path.GetFileName(filePath), Content = dataEditor };
                Tabs.Add(newTab);
                TabControlMain.SelectedItem = newTab;

                await Dispatcher.Yield(DispatcherPriority.Loaded);
                await dataEditor.LoadFileCardList(filePath);
                await Dispatcher.InvokeAsync(() =>
                {
                    dataEditor.FocusCardById(id);
                }, DispatcherPriority.Render);
            }
        }
        public async Task OpenDataEditorTab(ulong id)
        {
            if (id == 0) return;
            string dataSource = ConfigViewModel.Instance.userSetting.DataSource;
            if (string.IsNullOrWhiteSpace(dataSource) || !Directory.Exists(dataSource)) return;

            if (!CardEXDataViewModel.Instance.IsLoadedCard) await CardEXDataViewModel.Instance.LoadCardsEXAsync();


            var listPath = CardEXDataViewModel.Instance.TryGetCardPath(id);
            if (listPath == null) return;
            if (listPath.Count == 0) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"{CMess.fileNotExit.ToText()}", new[] { CMess.ok.ToText() });
            else if (listPath.Count == 1) await OpenDataEditorTab(listPath[0], id);
            else
            {
                var (selectFileResult, selectedItems) = SelectFileToOpen(listPath);
                if (selectFileResult)
                {
                    foreach (var item in selectedItems)
                    {
                        if (!File.Exists(item.FullPath))
                        {
                            CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                CMess.fileNotExit.ToText(), new[] { CMess.ok.ToText() });
                            continue;
                        }
                        await OpenDataEditorTab(item.FullPath, id);
                    }
                }
            }
        }

        public async Task OpenDataEditorTab(IEnumerable<string> filePaths)
        {
            foreach (var file in filePaths)
            {
                if (string.IsNullOrWhiteSpace(file) || !File.Exists(file)) continue;
                await OpenDataBase(file);
                await Dispatcher.Yield(DispatcherPriority.Background);
            }
        }
        public async Task OpenDataEditorTab(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath) || Tabs == null) return;

            var existingTab = Tabs.FirstOrDefault(t => t.Content is DataEditor dataEditor && dataEditor.cdbFilePath == filePath);
            if (existingTab != null)
            {
                TabControlMain.SelectedItem = existingTab;
            }
            else
            {
                var dataEditor = new DataEditor(this);
                var newTab = new TabContent { Header = Path.GetFileName(filePath), Content = dataEditor };
                Tabs.Add(newTab);
                TabControlMain.SelectedItem = newTab;

                await Dispatcher.Yield(DispatcherPriority.Loaded);
                await dataEditor.LoadFileCardList(filePath);
            }
        }
        #endregion

        #region Open CodeEditor Tab
        public async Task OpenCodeEditorTab(IEnumerable<string> filePaths)
        {
            foreach (var file in filePaths)
            {
                if (string.IsNullOrWhiteSpace(file) || !File.Exists(file)) continue;
                await OpenCodeEditorTab(file);
                await Dispatcher.Yield(DispatcherPriority.Background);
            }
        }
        public async Task OpenCodeEditorTab(string filePath, string archiveFilePath = null, string archiveEntryName = null)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath) || Tabs == null) return;
            string fileName = System.IO.Path.GetFileName(filePath);

            var existingTab = Tabs.FirstOrDefault(t => t.Content is CodeEditor codeEditor && codeEditor.luaFilePath == filePath);
            if (existingTab != null)
            {
                TabControlMain.SelectedItem = existingTab;
            }
            else
            {
                var newCodeEditor = new CodeEditor(this)
                {
                    luaFilePath = filePath,
                    luaFileName = fileName,
                    archiveFilePath = archiveFilePath,
                    archiveEntryName = archiveEntryName,
                };
                var newTab = new TabContent { Header = fileName, Content = newCodeEditor };
                Tabs.Add(newTab);
                TabControlMain.SelectedItem = newTab;

                await newCodeEditor.LoadLuaFile();
            }
            await Dispatcher.Yield(DispatcherPriority.Render);
        }
        public async Task OpenCodeEditorTab(ulong id)
        {
            if (id == 0) return;
            string dataSource = ConfigViewModel.Instance.userSetting.DataSource;
            if (string.IsNullOrWhiteSpace(dataSource) || !Directory.Exists(dataSource)) return;

            if (!CardEXDataViewModel.Instance.IsLoadedScript) await CardEXDataViewModel.Instance.LoadScriptAsync();

            var listPath = CardEXDataViewModel.Instance.TryGetScriptpath(id);
            if (listPath == null) return;
            if (listPath.Count == 0) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"{CMess.fileNotExit.ToText()}", new[] { CMess.ok.ToText() });
            else if (listPath.Count == 1) await OpenCodeEditorTab(listPath[0]);
            else
            {
                var (selectFileResult, selectedItems) = SelectFileToOpen(listPath);
                if (selectFileResult)
                {
                    foreach (var item in selectedItems)
                    {
                        if (!File.Exists(item.FullPath))
                        {
                            CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                CMess.fileNotExit.ToText(), new[] { CMess.ok.ToText() });
                            continue;
                        }
                        await OpenCodeEditorTab(item.FullPath, item.ArchiveFilePath, item.ArchiveEntryName);
                    }
                }
            }
        }
        #endregion

        #region Open Image
        public void OpenViewImage(string ImageUrl)
        {
            if (!string.IsNullOrWhiteSpace(ImageUrl) && System.IO.File.Exists(ImageUrl))
            {
                var viewer = new ImageViewerWindow(ImageUrl);
                viewer.Owner = this;
                viewer.ShowDialog();
            }
            else
            {
                ///
            }
        }
        public void OpenFileLocation(string ImageUrl)
        {
            if (!string.IsNullOrWhiteSpace(ImageUrl) && System.IO.File.Exists(ImageUrl))
            {
                try
                {
                    string argument = $"/select,\"{ImageUrl}\"";
                    Process.Start("explorer.exe", argument);
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
        }
        #endregion

        #region Open File Explorer
        private void BrowseFileTitle()
        {
            if (string.IsNullOrEmpty(MainWindowTitle) || !System.IO.File.Exists(MainWindowTitle)) return;

            OpenFileLocation(MainWindowTitle);
        }
        private bool CanBrowseFileTitle()
        {
            return CanBrowseWindowTitle;
        }
        #endregion

        #region Open WWeb
        private async Task<bool> LoadKonamiID()
        {
            var (resultLoad, messageLoad) = await KonamiIDViewModel.Instance.LoadKonamiID();
            if (resultLoad) return true;
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageLoad}", new[] { CMess.ok.ToText() });
                return false;
            }
        }
        public async Task OpenKonamiDB(ulong id, string name)
        {
            if (!KonamiIDViewModel.Instance.IsLoaded)
            {
                bool loadResult = await LoadKonamiID();
                if (!loadResult) return;
            }
            try
            {
                int? konamiID = null;
                string URL = string.Empty;
                if (id > 600000000)
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                        CMess.konamiIDnotFou.ToText(), new[] { CMess.ok.ToText() });
                    return;
                }
                if (id < 100000000)
                {
                    konamiID = KonamiIDViewModel.Instance.GetOfficialKonamiID(id);
                    if (konamiID != null) URL = $"https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&request_locale=ja&cid={konamiID}";
                }
                else if (id >= 160000000 && id < 300000000)
                {
                    konamiID = KonamiIDViewModel.Instance.GetRushKonamiID(name);
                    if (konamiID != null) URL = $"https://www.db.yugioh-card.com/rushdb/card_search.action?ope=2&request_locale=ja&cid={konamiID}";
                }
                else
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                        CMess.konamiIDnotFou.ToText(), new[] { CMess.ok.ToText() });
                    return;
                }
                if (konamiID.HasValue && !string.IsNullOrEmpty(URL)) BrowserURL.NavigateBrowser(URL);
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        public async Task OpenYugipedia(ulong id, string name)
        {
            if (!KonamiIDViewModel.Instance.IsLoaded)
            {
                bool loadResult = await LoadKonamiID();
                if (!loadResult) return;
            }
            try
            {
                if (id >= 600000000 || string.IsNullOrWhiteSpace(name))
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                        CMess.yugiPedianotFou.ToText(), new[] { CMess.ok.ToText() });
                    return;
                }
                string URL = $"https://yugipedia.com/wiki/{NameReplaceHelper.ProcessName(name, id)}";
                BrowserURL.NavigateBrowser(URL);
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        public async Task OpenYGOResources(ulong id, string name)
        {
            if (!KonamiIDViewModel.Instance.IsLoaded)
            {
                bool loadResult = await LoadKonamiID();
                if (!loadResult) return;
            }
            try
            {
                int? konamiID;
                if (id >= 600000000)
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                        CMess.konamiIDnotFou.ToText(), new[] { CMess.ok.ToText() });
                    return;
                }
                if (id < 100000000)
                {
                    konamiID = KonamiIDViewModel.Instance.GetOfficialKonamiID(id);
                }
                else if (id >= 160000000 && id < 300000000)
                {
                    konamiID = KonamiIDViewModel.Instance.GetRushKonamiID(name);
                }
                else
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                        CMess.konamiIDnotFou.ToText(), new[] { CMess.ok.ToText() });
                    return;
                }
                if (konamiID.HasValue)
                {
                    string URL = $"https://db.ygoresources.com/card#{konamiID}";
                    BrowserURL.NavigateBrowser(URL);
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #endregion

        #region Save
        private async Task SaveDatabase()
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                if (!string.IsNullOrWhiteSpace(currentDataEditor.cdbFilePath) && File.Exists(currentDataEditor.cdbFilePath))
                {
                    await currentDataEditor.ModifyCard();
                    await currentDataEditor.SaveCardDBFile();
                }
                else
                {
                    string dbFilePath = await CreateDatabase();
                    if (string.IsNullOrEmpty(dbFilePath)) return;
                    currentDataEditor.cdbFilePath = dbFilePath;
                    await currentDataEditor.ModifyCard();
                    await currentDataEditor.SaveCardDBFile();
                }
            }
        }
        private async Task SaveExcel()
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                if (!string.IsNullOrWhiteSpace(currentDataEditor.xlsxFilePath) && File.Exists(currentDataEditor.xlsxFilePath))
                {
                    await currentDataEditor.ModifyCard();
                    await currentDataEditor.SaveExcelFile();
                }
                else
                {
                    string excelFilePath = await CreateExcel();
                    if (string.IsNullOrEmpty(excelFilePath)) return;
                    currentDataEditor.xlsxFilePath = excelFilePath;
                    await currentDataEditor.ModifyCard();
                    await currentDataEditor.SaveExcelFile();
                }
            }
        }
        private async Task SaveCeds()
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                if (!string.IsNullOrWhiteSpace(currentDataEditor.cedsFilePath) && File.Exists(currentDataEditor.cedsFilePath))
                {
                    await currentDataEditor.ModifyCard();
                    await currentDataEditor.SaveCedsFile();
                }
                else
                {
                    string cedsFilePath = await CreateCeds();
                    if (string.IsNullOrEmpty(cedsFilePath)) return;
                    currentDataEditor.cedsFilePath = cedsFilePath;
                    await currentDataEditor.ModifyCard();
                    await currentDataEditor.SaveCedsFile();
                }
            }
        }

        private async Task CommandSaveExecuted()
        {
            if (currentUserControl != null)
            {
                if (currentUserControl is DataEditor currentDataEditor) await SaveDatabase();
                else if (currentUserControl is CodeEditor currentCodeEditor)
                {
                    if (!string.IsNullOrWhiteSpace(currentCodeEditor.luaFilePath) &&
                        File.Exists(currentCodeEditor.luaFilePath))
                    {
                        await currentCodeEditor.SaveCodeCommand();
                    }
                    else
                    {
                        string luaFilePath = await CreateScript(false);
                        currentCodeEditor.luaFilePath = luaFilePath;
                        await currentCodeEditor.SaveCodeCommand();
                    }
                }
                else if (currentUserControl is DeckEditor currentDeckEditor)
                {
                    await currentDeckEditor.Save();
                }
                else if (currentUserControl is BanListEditor currentBanListEditor)
                {
                    await currentBanListEditor.SaveBanListFile();
                }
                else if (currentUserControl is ImageEditor)
                {
                    ///
                }
                else
                {
                    /// 
                }
            }
            else
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noSelecWin.ToText(), new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region Save As

        #region Save As DataEditor
        private async Task SaveAsCdbFile(ScopeCard scope)
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                string dbFilePath = FileDiaLogHelper.SaveDataBase();
                if (string.IsNullOrEmpty(dbFilePath)) return;

                bool hasFlag = currentDataEditor._cdbFileHasFlag;
                var (resultCreate, messageCreate) = await CreateFileServices.CreateDatabaseCommand(dbFilePath, hasFlag);
                if (!resultCreate)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText())} {messageCreate}",
                        new[] { CMess.ok.ToText() });
                    return;
                }

                var (resultSaveAs, messageSaveAs) = scope switch
                {
                    ScopeCard.SelectedCards => await currentDataEditor.SaveAsCardDBFileSelectedData(messageCreate),
                    ScopeCard.FiltedCards => await currentDataEditor.SaveAsCardDBFileFiltedData(messageCreate),
                    ScopeCard.AllCards => await currentDataEditor.SaveAsCardDBFileAllData(messageCreate),
                    _ => (false, string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText()))
                };

                if (resultSaveAs) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.ThreePlaceholderSuccess.ToText(), CMess.SaveAs.ToText(), CMess.CardDB.ToText(), CMess.File.ToText()),
                    new[] { CMess.ok.ToText() });
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.SaveAs.ToText(), CMess.CardDB.ToText(), CMess.File.ToText())} {messageSaveAs}",
                    new[] { CMess.ok.ToText() });

            }
        }
        private async Task SaveAsXlsxFile(ScopeCard scope)
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                string xlsxFilePath = FileDiaLogHelper.SaveExcel();
                if (string.IsNullOrEmpty(xlsxFilePath)) return;

                bool hasFlag = currentDataEditor._xlsxFileHasFlag;
                var (resultCreate, messageCreate) = await CreateFileServices.CreateExcelCommand(xlsxFilePath, hasFlag);
                if (!resultCreate)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.Excel.ToText(), CMess.File.ToText())} {messageCreate}",
                        new[] { CMess.ok.ToText() });
                    return;
                }

                var (resultSaveAs, messageSaveAs) = scope switch
                {
                    ScopeCard.SelectedCards => await currentDataEditor.SaveAsExcelFileSelectedData(messageCreate),
                    ScopeCard.FiltedCards => await currentDataEditor.SaveAsExcelFileFiltedData(messageCreate),
                    ScopeCard.AllCards => await currentDataEditor.SaveAsExcelFileAllData(messageCreate),
                    _ => (false, string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText()))
                };

                if (resultSaveAs) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.ThreePlaceholderSuccess.ToText(), CMess.SaveAs.ToText(), CMess.Excel.ToText(), CMess.File.ToText()),
                    new[] { CMess.ok.ToText() });
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.SaveAs.ToText(), CMess.Excel.ToText(), CMess.File.ToText())} {messageSaveAs}",
                    new[] { CMess.ok.ToText() });
            }
        }
        private async Task SaveAsCedsFile(ScopeCard scope)
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                string cedsFilePath = FileDiaLogHelper.SaveCeds();
                if (string.IsNullOrEmpty(cedsFilePath)) return;

                bool hasFlag = currentDataEditor._cedsFileHasFlag;
                var (resultCreate, messageCreate) = await CreateFileServices.CreateCedsCommand(cedsFilePath);
                if (!resultCreate)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.Ceds.ToText(), CMess.File.ToText())} {messageCreate}",
                        new[] { CMess.ok.ToText() });
                    return;
                }

                var (resultSaveAs, messageSaveAs) = scope switch
                {
                    ScopeCard.SelectedCards => await currentDataEditor.SaveAsCedsFileSelectedData(messageCreate),
                    ScopeCard.FiltedCards => await currentDataEditor.SaveAsCedsFileFiltedData(messageCreate),
                    ScopeCard.AllCards => await currentDataEditor.SaveAsCedsFileAllData(messageCreate),
                    _ => (false, string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText()))
                };

                if (resultSaveAs) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.ThreePlaceholderSuccess.ToText(), CMess.SaveAs.ToText(), CMess.Ceds.ToText(), CMess.File.ToText()),
                    new[] { CMess.ok.ToText() });
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.SaveAs.ToText(), CMess.Ceds.ToText(), CMess.File.ToText())} {messageSaveAs}",
                    new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region Save As Non-DataEditor
        private async Task SaveAsExecuted()
        {
            if (currentUserControl is CodeEditor currentCodeEditor)
            {
                string luaFilePath = await CreateScript(false);
                if (!string.IsNullOrEmpty(luaFilePath) && File.Exists(luaFilePath))
                    await currentCodeEditor.SaveCodeCommand(luaFilePath);
            }
            else if (currentUserControl is DeckEditor currentDeckEditor)
            {
                if (string.IsNullOrWhiteSpace(currentDeckEditor.NewFolderPath) ||
                    !Directory.Exists(currentDeckEditor.NewFolderPath) ||
                    string.IsNullOrWhiteSpace(currentDeckEditor.NewNameDeck))
                {
                    currentDeckEditor.OpenReNameDeck();
                }
                else
                {
                    await currentDeckEditor.SaveAsDeck();
                }
            }
            else if (currentUserControl is BanListEditor currentBanListEditor)
            {
                if (string.IsNullOrWhiteSpace(currentBanListEditor.NewFolderPath) ||
                    !Directory.Exists(currentBanListEditor.NewFolderPath) ||
                    string.IsNullOrWhiteSpace(currentBanListEditor.NewFileName))
                {
                    currentBanListEditor.NewBanList();
                }
                else
                {
                    await currentBanListEditor.SaveAsBanList();
                }
            }
            else return;
        }

        #endregion

        #endregion

        #endregion

        #region Window
        private void OpenChatTab()
        {
            AnimateDrawer(blMainChat.Width, 0);
        }
        public void CloseChatTab()
        {
            AnimateDrawer(0, blMainChat.Width);
        }

        #region Script Support
        private string GetScriptSupportPath()
        {
            string cardEditorPath = Directory.GetParent(CardAppContext.Instance.ExeFilePath).Parent.FullName;

            string scriptSPPathFinal = System.IO.Path.Combine(cardEditorPath, @"ScriptSupport\ScriptSupport.exe");
            if (File.Exists(scriptSPPathFinal)) return scriptSPPathFinal;

            string cardEditorX = Directory.GetParent(CardAppContext.Instance.ExeFilePath).Parent.Parent.Parent.FullName;

            string scriptSPPathDebug = System.IO.Path.Combine(cardEditorX, @"ScriptSupport\bin\Debug\net8.0-windows\ScriptSupport.exe");
            if (File.Exists(scriptSPPathDebug)) return scriptSPPathDebug;

            string scriptSPPathRelease = System.IO.Path.Combine(cardEditorX, @"ScriptSupport\bin\Release\net8.0-windows\ScriptSupport.exe");
            if (File.Exists(scriptSPPathRelease)) return scriptSPPathRelease;

            string configPath = ConfigurationManager.AppSettings["scriptsupportpath"];
            if (File.Exists(configPath)) return configPath;

            return string.Empty;
        }
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        // Các hằng số của WinAPI
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;

        private const int WS_CHILD = 0x40000000;
        private const int WS_POPUP = unchecked((int)0x80000000);

        private const int WS_CAPTION = 0xC00000;
        private const int WS_TOOLWINDOW = 0x00000080;  // Cửa sổ công cụ
        private const int WS_OVERLAPPEDWINDOW = 0x00CF0000;  // Cửa sổ bình thường
        private const int WS_THICKFRAME = 0x00040000;  // Cửa sổ có viền thay đổi kích thước (viền mỏng)
        private const int WS_SYSMENU = 0x00080000; // Cửa sổ có menu hệ thống (nếu cần thiết)

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        private uint _hostThreadId;
        private uint _childThreadId;
        private bool _threadsAttached = false;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_FRAMECHANGED = 0x0020;


        private void OpemScriptSupportWindow()
        {
            string ScriptSupportPath = GetScriptSupportPath();
            if (string.IsNullOrEmpty(ScriptSupportPath) || !File.Exists(ScriptSupportPath))
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.ScriptSupport.ToText()} {string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText())}", new[] { CMess.ok.ToText() });
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ScriptSupportPath,
                Arguments = "--embedded true",
                UseShellExecute = false
            };

            Process projectBProcess = Process.Start(startInfo);
            projectBProcess.EnableRaisingEvents = true;
            projectBProcess.Exited += (s, args) =>
            {
                if (_threadsAttached)
                {
                    AttachThreadInput(_hostThreadId, _childThreadId, false);
                    _threadsAttached = false;
                }
            };
            // Đợi process khởi động và có window handle
            projectBProcess.WaitForInputIdle();

            IntPtr externalAppHandle = IntPtr.Zero;
            int attempts = 0;
            while (externalAppHandle == IntPtr.Zero && attempts < 30)
            {
                projectBProcess.Refresh();
                externalAppHandle = projectBProcess.MainWindowHandle;
                if (externalAppHandle == IntPtr.Zero)
                {
                    System.Threading.Thread.Sleep(100);
                    attempts++;
                }
            }
            if (externalAppHandle != IntPtr.Zero)
            {
                WindowInteropHelper helper = new WindowInteropHelper(this);
                IntPtr mainWindowHandle = helper.Handle;
                SetParent(externalAppHandle, mainWindowHandle);
                SetWindowToToolWindow(externalAppHandle);

                int x = (int)(this.ActualWidth - 380);
                int y = 40;
                int width = 650;
                int height = 600;
                MoveWindow(externalAppHandle, x, y, width, height, true);

                // --- Fix bàn phím ---
                _hostThreadId = GetCurrentThreadId();
                _childThreadId = GetWindowThreadProcessId(externalAppHandle, out _);

                _threadsAttached = AttachThreadInput(_hostThreadId, _childThreadId, true);

                // Đẩy focus ban đầu vào Child để nó sẵn sàng nhận phím ngay
                SetFocus(externalAppHandle);
            }
        }

        private void SetWindowToResizableWithThinBorder(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_STYLE);
            SetWindowLong(hWnd, GWL_STYLE, exStyle & ~WS_CAPTION);
        }
        // Hàm để thiết lập cửa sổ con
        private void SetWindowToToolWindow(IntPtr hWnd)
        {
            int style = GetWindowLong(hWnd, GWL_STYLE);

            style &= ~WS_CAPTION;
            style &= ~WS_POPUP;      // bỏ popup
            style |= WS_CHILD;       // BẮT BUỘC để MoveWindow hiểu tọa độ theo parent
            style |= WS_THICKFRAME | WS_SYSMENU;

            SetWindowLong(hWnd, GWL_STYLE, style);

            // Bắt buộc gọi SetWindowPos với SWP_FRAMECHANGED
            // để Windows áp dụng lại style/non-client area ngay lập tức
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);

            //// Lấy kiểu cửa sổ hiện tại của cửa sổ con
            //int exStyle = GetWindowLong(hWnd, GWL_STYLE);

            //// Loại bỏ thanh tiêu đề (WS_CAPTION)
            //exStyle &= ~WS_CAPTION;

            //// Giữ lại viền thay đổi kích thước (WS_THICKFRAME) và menu hệ thống (nếu cần thiết)
            //exStyle |= WS_THICKFRAME | WS_SYSMENU;

            //// Cập nhật kiểu cửa sổ
            //SetWindowLong(hWnd, GWL_STYLE, exStyle);
        }
        #endregion

        #region Chat
        private void ChatIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            ChatIcon.Opacity = 1;
        }
        private void ChatIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            isDragging = false;

            dragStartPoint = e.GetPosition(canvas);

            // Lưu vị trí hiện tại của icon
            elementStartPosition = new Point(
                double.IsNaN(Canvas.GetLeft(ChatIcon)) ? 0 : Canvas.GetLeft(ChatIcon),
                double.IsNaN(Canvas.GetTop(ChatIcon)) ? 0 : Canvas.GetTop(ChatIcon));
            ChatIcon.CaptureMouse();
        }
        private void ChatIcon_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isMouseDown) return;

            Point currentPoint = e.GetPosition(canvas);
            Vector diff = currentPoint - dragStartPoint;

            if (!isDragging && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                isDragging = true;
            }

            if (isDragging)
            {
                double newLeft = elementStartPosition.X + diff.X;
                double newTop = elementStartPosition.Y + diff.Y;

                // Giới hạn trong vùng Canvas
                newLeft = Math.Max(0, Math.Min(newLeft, canvas.ActualWidth - ChatIcon.ActualWidth));
                newTop = Math.Max(0, Math.Min(newTop, canvas.ActualHeight - ChatIcon.ActualHeight));

                Canvas.SetLeft(ChatIcon, newLeft);
                Canvas.SetTop(ChatIcon, newTop);
            }
        }
        private void ChatIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChatIcon.ReleaseMouseCapture();

            if (!isDragging)
            {
                OpenChatTab();
            }

            isMouseDown = false;
            isDragging = false;
        }
        private void ChatIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            ChatIcon.Opacity = 0.7;
        }

        public void OpenChatSetting()
        {
            ChatConfig chatConfig = new ChatConfig();
            chatConfig.ShowInTaskbar = false;
            chatConfig.Owner = this;
            chatConfig.ShowDialog();
        }

        
        private void AnimateDrawer(double from, double to)
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to ,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = to == 0 ? EasingMode.EaseOut : EasingMode.EaseIn }
            };
            DrawerTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void DragHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _startPoint = e.GetPosition(this);
            _initialWidth = blMainChat.Width;
            DragHandle.CaptureMouse();
        }
        private void DragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var currentPoint = e.GetPosition(this);
                double deltaX = currentPoint.X - _startPoint.X;
                double newWidth = _initialWidth - deltaX;

                // Giới hạn chiều rộng (tối thiểu 200, tối đa 800)
                newWidth = Math.Max(200, Math.Min(800, newWidth));
                blMainChat.Width = newWidth;
            }
        }
        private void DragHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                DragHandle.ReleaseMouseCapture();
                ConfigViewModel.Instance.displaySetting.WidthChat = (int)blMainChat.Width;
                ConfigViewModel.Instance.SaveDisplaySettingFile();
            }
        }
        private void DragHandle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                DragHandle.ReleaseMouseCapture();
                ConfigViewModel.Instance.displaySetting.WidthChat = (int)blMainChat.Width;
                ConfigViewModel.Instance.SaveDisplaySettingFile();
            }
        }

        #endregion

        #endregion

        #region Settings
        public void SettingCommand()
        {
            ConfigEditor configEditor = new ConfigEditor();
            configEditor.ShowInTaskbar = false;
            configEditor.Owner = this;
            configEditor.MainWindowReference = this;
            configEditor.ConfigChanged += ConfigEditor_ConfigChanged;
            configEditor.ShowInTaskbar = false;
            try
            {
                configEditor.ShowDialog();
            }
            finally
            {
                configEditor.ConfigChanged -= ConfigEditor_ConfigChanged;
            }
        }
        private async void ConfigEditor_ConfigChanged()
        {
            UIConfigViewModel.Instance.LoadConfig();
            await CardDataViewModel.Instance.LoadData();
            LoadConfig();
            LoadLanguage();
            foreach (var tab in Tabs)
            {
                if (tab.Content is UserControl userControl)
                {
                    MethodInfo loadConfigMethod = userControl.GetType().GetMethod("LoadConfig", BindingFlags.Public | BindingFlags.Instance);
                    if (loadConfigMethod != null)
                    {
                        userControl.Dispatcher.Invoke(() =>
                        {
                            loadConfigMethod.Invoke(userControl, null);
                            userControl.UpdateLayout();
                        });
                    }
                }
            }
            foreach (var tab in Tabs)
            {
                if (tab.Content is UserControl userControl)
                {
                    MethodInfo loadConfigMethod = userControl.GetType().GetMethod("LoadFilter", BindingFlags.Public | BindingFlags.Instance);
                    if (loadConfigMethod != null)
                    {
                        userControl.Dispatcher.Invoke(() =>
                        {
                            loadConfigMethod.Invoke(userControl, null);
                            userControl.UpdateLayout();
                        });
                    }
                }
            }
            EnumsViewModel.Instance.ReLoadDisplayName();
        }
        #endregion

        #region Card

        #region Copy/Paste Card
        private async Task CopySelectedCard()
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                await currentDataEditor.CopySelectedCard();
            }
            else if (currentUserControl != null && currentUserControl is BanListEditor currentBanListEditor)
            {
                await currentBanListEditor.CopySelectedCard();
            }
        }
        private async Task CopyFiltedCard()
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                await currentDataEditor.CopyAllFilterCard();
            }
            else if (currentUserControl != null && currentUserControl is BanListEditor currentBanListEditor)
            {
                await currentBanListEditor.CopyAllFilterCard();
            }
        }
        private async Task CopyAllCard()
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                await currentDataEditor.CopyAllCard();
            }
            else if (currentUserControl != null && currentUserControl is BanListEditor currentBanListEditor)
            {
                await currentBanListEditor.CopyAllCard();
            }
        }

        private async Task PasteCard()
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                try
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                    });

                    var pastedCardsResult = await LoadDataServices.LoadClipboardCard();
                    if (pastedCardsResult != null && pastedCardsResult.Result)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 0) // Ask
                            {
                                int resultImport = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, CMess.confirmWriteData.ToText(),
                                    new[] { CMess.OverwriteDupli.ToText(), CMess.Appendwrite.ToText(), CMess.CreateNew.ToText(), CMess.cancel.ToText() });
                                if (resultImport == 0) currentDataEditor.ImportOverwrite(pastedCardsResult.CardList); // Overwrite
                                else if (resultImport == 1) currentDataEditor.ImportAppendwrite(pastedCardsResult.CardList); // Append
                                else if (resultImport == 2) ImportDataCreateNewDataEdit(pastedCardsResult.CardList); // Create New
                                else return;
                            }
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 1) currentDataEditor.ImportOverwrite(pastedCardsResult.CardList); // Overwrite
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 2) currentDataEditor.ImportAppendwrite(pastedCardsResult.CardList); // Append
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 3) ImportDataCreateNewDataEdit(pastedCardsResult.CardList); // Create New
                            else return;
                        });
                    }
                    else
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {pastedCardsResult.Message}", new[] { CMess.ok.ToText() });
                    }
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
            else if (currentUserControl != null && currentUserControl is BanListEditor currentBanListEditor)
            {
                try
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                    });

                    var pastedCardsResult = await LoadDataServices.LoadClipboardCardBanList();
                    if (pastedCardsResult != null && pastedCardsResult.Result)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 0) // Ask
                            {
                                int resultImport = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, CMess.confirmWriteData.ToText(),
                                    new[] { CMess.OverwriteDupli.ToText(), CMess.Appendwrite.ToText(), CMess.CreateNew.ToText(), CMess.cancel.ToText() });
                                if (resultImport == 0) currentBanListEditor.ImportOverwrite(pastedCardsResult.CardList); // Overwrite
                                else if (resultImport == 1) currentBanListEditor.ImportAppendwrite(pastedCardsResult.CardList); // Append
                                else if (resultImport == 2) ImportDataCreateNewBanListEdit(pastedCardsResult.CardList); // Create New
                                else return;
                            }
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 1) currentBanListEditor.ImportOverwrite(pastedCardsResult.CardList); // Overwrite
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 2) currentBanListEditor.ImportAppendwrite(pastedCardsResult.CardList); // Append
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 3) ImportDataCreateNewBanListEdit(pastedCardsResult.CardList); // Create New
                            else return;
                        });
                    }
                    else
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {pastedCardsResult.Message}", new[] { CMess.ok.ToText() });
                    }
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
        }
        #endregion

        #region Filter Card
        private void OpenFilterCard()
        {
            FilterCard filterCard = new FilterCard();
            filterCard.ShowInTaskbar = false;
            filterCard.Owner = this;
            filterCard.MainWindowReference = this;
            filterCard.ShowDialog();
        }
        public async Task<ResultItem> FilterCardByLanguage(int languageCode, bool isInclude)
        {
            if (currentUserControl is DataEditor currentDataEditor)
            {
                return await currentDataEditor.FilterCardByLanguage(languageCode, isInclude);
            }
            else
            {
                return new ResultItem
                {
                    Succeeded = true,
                    FilteredCount = 0,
                    TotalCount = 0,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };
            }
        }
        public async Task<ResultItem> FilterCardByPenLang(PendulumLanguageRule rule, bool isInclude)
        {
            if (currentUserControl is DataEditor currentDataEditor)
            {
                return await currentDataEditor.FilterCardByPenLang(rule, isInclude);
            }
            else
            {
                return new ResultItem
                {
                    Succeeded = true,
                    FilteredCount = 0,
                    TotalCount = 0,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };
            }
        }
        public async Task<ResultItem> FilterCardByCDBFile(string filePath, bool isDuplicate)
        {
            if (currentUserControl is DataEditor currentDataEditor)
            {
                return await currentDataEditor.FilterCardByCDBFile(filePath, isDuplicate);
            }
            else if (currentUserControl is BanListEditor currentBanListEditor)
            {
                return await currentBanListEditor.FilterCardByCDBFile(filePath, isDuplicate);
            }
            else
            {
                return new ResultItem
                {
                    Succeeded = true,
                    FilteredCount = 0,
                    TotalCount = 0,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };
            }
        }
        public async Task<ResultItem> FilterCardByYDKFile(string filePath, bool isDuplicate)
        {
            if (currentUserControl is DataEditor currentDataEditor)
            {
                return await currentDataEditor.FilterCardByYDKFile(filePath, isDuplicate);
            }
            else if (currentUserControl is BanListEditor currentBanListEditor)
            {
                return await currentBanListEditor.FilterCardByYDKFile(filePath, isDuplicate);
            }
            else
            {
                return new ResultItem
                {
                    Succeeded = true,
                    FilteredCount = 0,
                    TotalCount = 0,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };
            }
        }
        #endregion

        #region Create Image
        public void OpenCreateImage()
        {
            CreateImage createImage = new CreateImage();
            createImage.ShowInTaskbar = false;
            createImage.Owner = this;
            createImage.MainWindowReference = this;
            createImage.ShowDialog();
        }

        public async Task<(bool Success, string Message)> CreateImageMainWindow(int Scope, int Series, Action<int, int> onProgress = null)
        {
            if (ImageValidate.isValid == false ||
                GeneraImageViewModel.Instance.IsLoadedImageCache == false ||
                RareRawDataViewModel.Instance.IsLoadedImageRareCache == false ||
                RareRawDataViewModel.Instance.IsLoadedImageRareRect == false)
            {
                return (false, CMess.errorLoadImageCache.ToText());
            }
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                var (result, message) = await currentDataEditor.CreateMultiImageDataEditor(Scope, Series, onProgress);
                return (result, result ? message : $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.Image.ToText(), CMess.Card.ToText())}\n{message}");
            }
            else return (false, CMess.noSelecWin.ToText());
        }
        public void CancelCreateImageMainWindow()
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                currentDataEditor.CancelCreateImageDataEditor();
            }
        }
        #endregion

        #endregion

        #region Data

        #region Export

        #region Export Zip
        private async Task ExportZipCard(ScopeCard scope)
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                string zipFilePath = FileDiaLogHelper.SaveZip();
                if (string.IsNullOrEmpty(zipFilePath)) return;

                var (resultExportZip, messageExportZip) = scope switch
                {
                    ScopeCard.SelectedCards => await currentDataEditor.ExportCompressedSelectedData(zipFilePath),
                    ScopeCard.FiltedCards => await currentDataEditor.ExportCompressedFiltedData(zipFilePath),
                    ScopeCard.AllCards => await currentDataEditor.ExportCompressedAllData(zipFilePath),
                    _ => (false, string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText()))
                };

                if (resultExportZip) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    $"{CMess.expoZIPSuc.ToText()} {messageExportZip}", new[] { CMess.ok.ToText() });
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Export.ToText(), CMess.Zip.ToText(), CMess.Card.ToText())} {messageExportZip}",
                    new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #endregion

        #region Item Editor
        public void OpenFinterSetting()
        {
            ItemsEditor itemsEditor = new ItemsEditor(ItemsEdit.Setting);
            itemsEditor.ShowInTaskbar = false;
            itemsEditor.Owner = this;
            itemsEditor.MainWindowReference = this;
            itemsEditor.ShowDialog();
        }

        #region Replace Desc
        public async Task<ResultItem> ReplaceDesc(string findWhat, string replaceWith, int scope)
        {
            if (string.IsNullOrWhiteSpace(findWhat))
            {
                return new ResultItem
                {
                    Succeeded = false,
                    Message = $"{CMess.FindWhat.ToText()} {CMess.cannotEmpty.ToText()}"
                };
            }
            if (string.IsNullOrWhiteSpace(replaceWith))
            {
                return new ResultItem
                {
                    Succeeded = false,
                    Message = $"{CMess.Replacewith.ToText()} {CMess.cannotEmpty.ToText()}"
                };
            }
            if (scope < 1 || scope > 3)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };
            }
            if (currentUserControl is DataEditor currentDataEditor)
            {
                var result = await currentDataEditor.ReplaceText(findWhat, replaceWith, scope);
                return result;
            }
            return new ResultItem { Succeeded = false, Message = CMess.noSelecWin.ToText() };
        }
        #endregion

        #region Replace Fields
        public (bool, List<ulong>) CheckDuplicateIds()
        {
            if (currentUserControl != null)
            {
                if (currentUserControl is DataEditor currentDataEditor)
                {
                    return currentDataEditor.CheckDuplicateIds();
                }
                else if (currentUserControl is BanListEditor currentBanListEditor)
                {
                    return currentBanListEditor.CheckDuplicateIds();
                }
                else return (false, null);
            }
            else return (false, null);
        }
        // Replace Card List chính bằng Card List phụ, theo từng thuộc tính được chọn. Có tùy chọn Add các Card có id không xuất hiện trong Card list chính.
        public async Task<(bool Success, int ReplacedCard, int TotalCard, string Message)> ReplaceField(string filePath, ulong flags, bool IsAddNew)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                return (false, 0, 0, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText()));
            //if (!CheckDatabase.IsDatabaseFile(filePath)) return (false, CMess.notDatabase.ToText());

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (currentUserControl != null)
                {
                    if (currentUserControl is DataEditor currentDataEditor)
                    {
                        LoadCardDataResult resultCard = Path.GetExtension(filePath).ToLowerInvariant() switch
                        {
                            var ext when ConstantExtension.CardDBExtensions.Contains(ext) => await LoadDataServices.LoadDatabaseCard(filePath),
                            var ext when ConstantExtension.ExcelExtensions.Contains(ext) => await LoadDataServices.LoadExcelCard(filePath),
                            var ext when ConstantExtension.CedsExtensions.Contains(ext) => await LoadDataServices.LoadCedsCard(filePath),
                            _ => new LoadCardDataResult { Result = false, Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()) }
                        };
                        if (!resultCard.Result || resultCard.CardList == null) return (false, 0, 0, resultCard.Message);

                        return currentDataEditor.ReplaceDataCommand(resultCard.CardList, flags, IsAddNew);
                    }
                    else if (currentUserControl is BanListEditor currentBanListEditor)
                    {
                        string fileName = Path.GetFileName(filePath).ToLowerInvariant();
                        if (string.IsNullOrEmpty(fileName)) return (false, 0, 0, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText()));
                        if (fileName.EndsWith(".lflist.conf"))
                        {
                            LoadBanListResult banListResult = await LoadDataServices.LoadFileBanList(filePath);
                            if (!banListResult.Result) return (false, 0, 0, banListResult.Message);

                            return currentBanListEditor.ReplaceDataCommand(banListResult.BanList.CardList.Values, flags, IsAddNew);
                        }
                        else
                        {
                            LoadCardBanDataResult resultCard = Path.GetExtension(filePath).ToLowerInvariant() switch
                            {
                                var ext when ConstantExtension.CardDBExtensions.Contains(ext) => await LoadDataServices.LoadDatabaseCardBanList(filePath),
                                var ext when ConstantExtension.ExcelExtensions.Contains(ext) => await LoadDataServices.LoadExcelCardBanList(filePath),
                                var ext when ConstantExtension.CedsExtensions.Contains(ext) => await LoadDataServices.LoadCedsCardBanList(filePath),
                                _ => new LoadCardBanDataResult { Result = false, Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()) }
                            };
                            if (!resultCard.Result || resultCard.CardList == null) return (false, 0, 0, resultCard.Message);

                            return currentBanListEditor.ReplaceDataCommand(resultCard.CardList, flags, IsAddNew);
                        }
                    }
                    else return (false, 0, 0, CMess.noSelecWin.ToText());
                }
                else return (false, 0, 0, CMess.noSelecWin.ToText());
            }
            catch (Exception ex)
            {
                return (false, 0, 0, ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        #endregion

        #region Import Data
        // Add các Card có id không xuất hiện trong Card List phụ vào Card List chính. Tùy chọn replace thuộc tính được chọn đối với các Card trong Card List chính có id xuất hiện trong Card List phụ.
        public async Task<(bool, string)> ImportData(string filePath, ulong flags)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath)) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText()));
            //if (!CheckDatabase.IsDatabaseFile(filePath)) return (false, CMess.notDatabase.ToText());

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (currentUserControl != null)
                {
                    if (currentUserControl is DataEditor currentDataEditor)
                    {
                        LoadCardDataResult resultCard = Path.GetExtension(filePath).ToLowerInvariant() switch
                        {
                            var ext when ConstantExtension.CardDBExtensions.Contains(ext) => await LoadDataServices.LoadDatabaseCard(filePath),
                            var ext when ConstantExtension.ExcelExtensions.Contains(ext) => await LoadDataServices.LoadExcelCard(filePath),
                            var ext when ConstantExtension.CedsExtensions.Contains(ext) => await LoadDataServices.LoadCedsCard(filePath),
                            _ => new LoadCardDataResult { Result = false, Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()) }
                        };

                        if (!resultCard.Result || resultCard.CardList == null) return (false, resultCard.Message);

                        return currentDataEditor.ImportDataCommand(resultCard.CardList, flags);
                    }
                    else if (currentUserControl is BanListEditor currentBanListEditor)
                    {
                        string fileName = Path.GetFileName(filePath).ToLowerInvariant();
                        if (string.IsNullOrEmpty(fileName)) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText()));
                        if (fileName.EndsWith(".lflist.conf"))
                        {
                            LoadBanListResult banListResult = await LoadDataServices.LoadFileBanList(filePath);
                            if (!banListResult.Result) return (false, banListResult.Message);

                            return currentBanListEditor.ImportDataCommand(banListResult.BanList.CardList.Values, flags);
                        }
                        else
                        {
                            LoadCardBanDataResult resultCard = Path.GetExtension(filePath).ToLowerInvariant() switch
                            {
                                var ext when ConstantExtension.CardDBExtensions.Contains(ext) => await LoadDataServices.LoadDatabaseCardBanList(filePath),
                                var ext when ConstantExtension.ExcelExtensions.Contains(ext) => await LoadDataServices.LoadExcelCardBanList(filePath),
                                var ext when ConstantExtension.CedsExtensions.Contains(ext) => await LoadDataServices.LoadCedsCardBanList(filePath),
                                _ => new LoadCardBanDataResult { Result = false, Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()) }
                            };
                            if (!resultCard.Result || resultCard.CardList == null) return (false, resultCard.Message);

                            return currentBanListEditor.ImportDataCommand(resultCard.CardList, flags);
                        }
                    }
                    else return (false, CMess.noSelecWin.ToText());
                }
                else return (false, CMess.noSelecWin.ToText());
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public void ImportDataCreateNewDataEdit(IEnumerable<CardEditor.Models.Card> importedCards)
        {
            var newDataEditor = new DataEditor(this);
            var newTab = new TabContent
            {
                Header = CMess.DataEdit.ToText(),
                Content = newDataEditor
            };
            Tabs.Add(newTab);
            TabControlMain.SelectedItem = newTab;

            newDataEditor.Dispatcher.BeginInvoke(new Action(() =>
            {
                newDataEditor.ImportCreateNew(importedCards);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
        public void ImportDataCreateNewBanListEdit(IEnumerable<CardEditor.Models.CardBanList> importedCards)
        {
            var newBanListEditor = new BanListEditor(this);
            var newTab = new TabContent
            {
                Header = CMess.BanListEdit.ToText(),
                Content = newBanListEditor
            };
            Tabs.Add(newTab);
            TabControlMain.SelectedItem = newTab;

            newBanListEditor.Dispatcher.BeginInvoke(new Action(() =>
            {
                newBanListEditor.ImportCreateNew(importedCards);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        /*
        private async void menuImportCeds_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                string filePath = FileDiaLogHelper.OpenCeds();
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return;

                try
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                    });

                    var (importedCards, message) = await LoadDataServices.LoadCedsCard(filePath);
                    if (importedCards != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 0) // Ask
                            {
                                int resultImport = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, CMess.confirmWriteData.ToText(),
                                    new[] { CMess.Overwrite.ToText(), CMess.Appendwrite.ToText(), CMess.CreateNew.ToText(), CMess.cancel.ToText() });
                                if (resultImport == 0) currentDataEditor.ImportOverwrite(importedCards); // Overwrite
                                else if (resultImport == 1) currentDataEditor.ImportAppendwrite(importedCards); // Append
                                else if (resultImport == 2) ImportDataCreateNewDataEdit(importedCards); // Create New
                                else return;
                            }
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 1) currentDataEditor.ImportOverwrite(importedCards); // Overwrite
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 2) currentDataEditor.ImportAppendwrite(importedCards); // Append
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 3) ImportDataCreateNewDataEdit(importedCards); // Create New
                            else return;
                        });
                    }
                    else
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                    }
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
        }
        private async void menuImportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (currentUserControl != null && currentUserControl is DataEditor currentDataEditor)
            {
                string filePath = FileDiaLogHelper.OpenExcel();
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return;

                try
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                    });

                    var (importedCards, message) = await LoadDataServices.LoadExcelCard(filePath);
                    if (importedCards != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 0) // Ask
                            {
                                int resultImport = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, CMess.confirmWriteData.ToText(),
                                    new[] { CMess.Overwrite.ToText(), CMess.Appendwrite.ToText(), CMess.CreateNew.ToText(), CMess.cancel.ToText() });
                                if (resultImport == 0) currentDataEditor.ImportOverwrite(importedCards); // Overwrite
                                else if (resultImport == 1) currentDataEditor.ImportAppendwrite(importedCards); // Append
                                else if (resultImport == 2) ImportDataCreateNewDataEdit(importedCards); // Create New
                                else return;
                            }
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 1) currentDataEditor.ImportOverwrite(importedCards); // Overwrite
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 2) currentDataEditor.ImportAppendwrite(importedCards); // Append
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 3) ImportDataCreateNewDataEdit(importedCards); // Create New
                            else return;
                        });
                    }
                    else
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                    }
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
            else if (currentUserControl != null && currentUserControl is BanListEditor currentBanListEditor)
            {
                string filePath = FileDiaLogHelper.OpenExcel();
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return;

                try
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                    });

                    var (importedCards, message) = await LoadDataServices.LoadExcelCardBanList(filePath);
                    if (importedCards != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 0) // Ask
                            {
                                int resultImport = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, CMess.confirmWriteData.ToText(),
                                    new[] { CMess.Overwrite.ToText(), CMess.Appendwrite.ToText(), CMess.CreateNew.ToText(), CMess.cancel.ToText() });
                                if (resultImport == 0) currentBanListEditor.ImportOverwrite(importedCards); // Overwrite
                                else if (resultImport == 1) currentBanListEditor.ImportAppendwrite(importedCards); // Append
                                else if (resultImport == 2) ImportDataCreateNewBanListEdit(importedCards); // Create New
                                else return;
                            }
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 1) currentBanListEditor.ImportOverwrite(importedCards); // Overwrite
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 2) currentBanListEditor.ImportAppendwrite(importedCards); // Append
                            else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 3) ImportDataCreateNewBanListEdit(importedCards); // Create New
                            else return;
                        });
                    }
                    else
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                    }
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
        }
        */
        #endregion

        #region Pendulum Language
        public void OpenPreViewDescWindow(Card card, PendulumLanguageRule rule)
        {
            PreviewDescWindow previewDescWindow = new PreviewDescWindow();
            previewDescWindow.ShowInTaskbar = false;
            previewDescWindow.Owner = this;
            previewDescWindow.MainWindowReference = this;
            previewDescWindow.InitializePartEffect(card, rule);
            previewDescWindow.ShowDialog();
        }
        public async Task<PenDescProcessSummary> PendulumLanguage(PendulumLanguageRule rule, int scope, bool overwrite)
        {
            if (rule == null || scope < 0 || scope > 2)
            {
                PenDescProcessSummary nullItem = new PenDescProcessSummary
                {
                    Result = false,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };
                return nullItem;
            }

            if (currentUserControl is DataEditor currentDataEditor)
            {
                var result = await currentDataEditor.PendulumLanguage(rule, scope, overwrite);
                return result;
            }
            else return new PenDescProcessSummary { Result = false, Message = CMess.noSelecWin.ToText() };
        }
        public void ApplyPendulumLanguage(string desc)
        {
            if (currentUserControl is DataEditor currentDataEditor)
            {
                currentDataEditor.ApplyPendulumLanguage(desc);
            }
        }
        public void StopApplyPenLang()
        {
            if (currentUserControl is DataEditor currentDataEditor)
            {
                currentDataEditor.StopApplyPenLang();
            }
        }
        public (bool, string) RollBackPenlang()
        {
            if (currentUserControl is DataEditor currentDataEditor)
            {
                return currentDataEditor.RollBackPenlang();
            }
            else return (false, CMess.noSelecWin.ToText());
        }
        #endregion

        #region Credit
        public async Task<ResultItem> CreditTeam(CreditItem credit, int scope, int writeMode)
        {
            if (credit == null || scope < 0 || scope > 2 || writeMode < 1 || writeMode > 4)
            {
                ResultItem nullItem = new ResultItem
                {
                    Succeeded = false,
                    TotalCount = 0,
                    FilteredCount = 0,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };
                return nullItem;
            }

            if (currentUserControl is DataEditor currentDataEditor)
            {
                var result = await currentDataEditor.CreditTeam(credit, scope, writeMode);
                return result;
            }
            else return new ResultItem { Succeeded = false, Message = CMess.noSelecWin.ToText() };
        }
        public async Task<ResultItem> RemoveCredit(int scope)
        {
            if (scope < 0 || scope > 2)
            {
                ResultItem nullItem = new ResultItem
                {
                    Succeeded = false,
                    TotalCount = 0,
                    FilteredCount = 0,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };
                return nullItem;
            }

            if (currentUserControl is DataEditor currentDataEditor)
            {
                var result = await currentDataEditor.RemoveCredit(scope);
                return result;
            }
            else return new ResultItem { Succeeded = false, Message = CMess.noSelecWin.ToText() };
        }
        public (bool, string) RollbackCredit()
        {
            if (currentUserControl is DataEditor currentDataEditor)
            {
                return currentDataEditor.RollbackCredits();
            }
            else return (false, CMess.noSelecWin.ToText());
        }
        public void StopApplyCredits()
        {
            if (currentUserControl is DataEditor currentDataEditor)
            {
                currentDataEditor.StopApplyCredits();
            }
        }
        #endregion

        #endregion

        #endregion

        #region Help
        private async Task UpdateOneAsync(string gitHubUrl, string folderPath, string displayName)
        {
            if (string.IsNullOrWhiteSpace(gitHubUrl)) return;

            (bool hasUpdate, string remoteSha, string branch) checkResult;

            try
            {
                this.Cursor = Cursors.Wait;
                checkResult = await GitHubService.CheckForUpdateAsync(folderPath, gitHubUrl);
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"[{displayName}] {CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return;
            }
            finally
            {
                this.Cursor = null;
            }

            if (!checkResult.hasUpdate)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    $"[{displayName}] {CMess.noUpdate.ToText()}", new[] { CMess.ok.ToText() });
                return;
            }

            // Hỏi User xác nhận trước khi tải
            var confirm = CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"[{displayName}] {CMess.quesDownloadUpdate.ToText()}",
                new[] { CMess.yes.ToText(), CMess.no.ToText() });

            if (confirm != 0) return;

            try
            {
                this.Cursor = Cursors.Wait;
                var (success, message) = await GitHubService.SyncSnapshotAsync(folderPath, gitHubUrl, checkResult.remoteSha);

                if (success)
                {
                    ConfigEditor_ConfigChanged();
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        $"[{displayName}] {CMess.updateCompe.ToText()}", new[] { CMess.ok.ToText() });
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"[{displayName}] {CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"[{displayName}] {CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            finally
            {
                this.Cursor = null;
            }
        }
        private async Task UpdateCardImage(object sender)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null) menuItem.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                string CardImageURL = ConfigurationManager.AppSettings["CardImageURL"];
                string CardImagePath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "CardImage");

                var downloader = new GithubReleaseDownloader();
                var (success, message) = await downloader.DownloadGithubRelease(CardImageURL, "CardImage.zip", CardImagePath);

                if (success)
                {
                    Mouse.OverrideCursor = null;
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        $"[Card Image] {CMess.updateCompe.ToText()}", new[] { CMess.ok.ToText() });
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.load.ToText(), CMess.Data.ToText())} {message}"
                        , new[] { CMess.ok.ToText() });
                }
            }
            catch (HttpRequestException netEx)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"Network connection error or unable to download from GitHub:\n{netEx.Message}", new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            finally
            {
                Mouse.OverrideCursor = null;
                if (menuItem != null) menuItem.IsEnabled = true;
            }
        }

        private void RegisterRegistry()
        {
            string filePath = FileDiaLogHelper.SaveRes();

            Debug.WriteLine(filePath);

            if (string.IsNullOrWhiteSpace(filePath)) return;

            var (Regresult, Regmessage) = RegistryHelper.CreateRegistry(filePath);
            Debug.WriteLine($"Registry Result: {Regresult}, Message: {Regmessage}");

            if (Regresult)
            {
                int result = CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    $"{CMess.registrySuc.ToText()}\n{Regmessage}.\n\n" +
                    $"{CMess.registryType.ToText()}\n" +
                    $"{CMess.openDirectly.ToText()}\n.ypk .cdb .ceds .lua .ydk\n" +
                    $"{CMess.openWithOnly.ToText()}\n.zip .db .sqlite .xlsx .txt .md .log .yml .conf\n\n" +
                    $"{CMess.setRegistry.ToText()}\n" +
                    $"{CMess.QuestOpen.ToText()}",
                new[] { CMess.yes.ToText(), CMess.no.ToText() });

                if (result != 0) return;

                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{Regmessage}\"",
                    UseShellExecute = true
                });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {Regmessage}",
                    new[] { CMess.ok.ToText() });
            }
        }
        private void UnregisterRegistry()
        {
            var (unRegresult, unRegmessage) = RegistryHelper.UnregisterRegistry();
            if (unRegresult)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    unRegmessage, new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {unRegmessage}",
                    new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region Change
        private object TabControlMain_NewItemFactory()
        {
            var chooseWindow = new ChooseTabWindow();
            TabContent newTab = null;
            bool? result = chooseWindow.ShowDialog();

            if (result == true)
            {
                imgmainbg.Visibility = Visibility.Hidden;
                switch (chooseWindow.SelectedOption)
                {
                    case EditorType.Image:
                        newTab = new TabContent { Header = CMess.ImageEdit.ToText(), Content = new ImageEditor(this) };
                        break;
                    case EditorType.Data:
                        newTab = new TabContent { Header = CMess.DataEdit.ToText(), Content = new DataEditor(this) };
                        break;
                    case EditorType.Omega:
                        newTab = new TabContent { Header = CMess.DataEdit.ToText(), Content = new OmegaDataEditor(this) };
                        break;
                    case EditorType.Deck:
                        newTab = new TabContent { Header = CMess.DeckEdit.ToText(), Content = new DeckEditor(this) };
                        break;
                    case EditorType.Code:
                        newTab = new TabContent { Header = CMess.CodeEdit.ToText(), Content = new CodeEditor(this) };
                        break;
                    case EditorType.BanList:
                        newTab = new TabContent { Header = CMess.BanListEdit.ToText(), Content = new BanListEditor(this) };
                        break;

                    default:
                        newTab = new TabContent { Header = CMess.Home.ToText(), Content = new Home(this) };
                        break;
                }
            }
            else
            {
                newTab = new TabContent { Header = CMess.Home.ToText(), Content = new Home(this) };
            }

            TabControlMain.SelectedItem = newTab;
            return newTab;
        }
        private async void TabControlMain_ClosingItemCallback(ItemActionCallbackArgs<TabablzControl> args)
        {
            var tabToPreClose = args.DragablzItem.DataContext as TabContent;
            if (tabToPreClose == null) return;

            if (tabToPreClose.Content is ISaveable saveable && !saveable.IsSaved)
            {
                var CloseResult = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    $"{CMess.HasUnSaveData.ToText()} {CMess.QuestSaveChange.ToText()}",
                    new[] { CMess.Save.ToText(), CMess.noSave.ToText(), CMess.cancel.ToText() });
                if (CloseResult == 0)
                {
                    bool saveSuccess = await saveable.Save();
                    if (!saveSuccess)
                    {
                        args.Cancel();
                        return;
                    }
                }
                else if (CloseResult == 1)
                {
                    /// 
                }
                else
                {
                    args.Cancel();
                    return;
                }
            }

            var tabToClose = args.DragablzItem.DataContext as TabContent;
            if (Tabs.Count > 1)
            {
                Tabs.Remove(tabToClose);
                TabControlMain.SelectedItem = Tabs.Count > 0 ? Tabs[0] : null;
            }
            else if (Tabs.Count == 1)
            {
                Tabs.Remove(tabToClose);
                var newHomeTab = new TabContent { Header = CMess.Home.ToText(), Content = new Home(this) };
                Tabs.Add(newHomeTab);
                TabControlMain.SelectedItem = newHomeTab;

                args.Cancel();
            }
        }
        private void TabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = sender as TabablzControl;
            if (tabControl != null)
            {
                imgmainbg.Visibility = Visibility.Hidden;
                var selectedTabContent = tabControl.SelectedItem as TabContent;
                string mainTitle = string.Empty;
                if (selectedTabContent != null)
                {
                    currentUserControl = selectedTabContent.Content;

                    switch (currentUserControl)
                    {
                        case BanListEditor currentBanListEditor: UpdateWindowTitle(currentBanListEditor.MainWindowTitle); break;
                        case CodeEditor currentCodeEditor: UpdateWindowTitle(currentCodeEditor.MainWindowTitle); break;
                        case DataEditor currentDataEditor: UpdateWindowTitle(currentDataEditor.MainWindowTitle); break;
                        case DeckEditor currentDeckEditor: UpdateWindowTitle(currentDeckEditor.MainWindowTitle); break;
                        case ImageEditor currentImageEditor: UpdateWindowTitle(currentImageEditor.MainWindowTitle); break;
                        case Home currentHome: UpdateWindowTitle(currentHome.MainWindowTitle); break;
                    }
                }
                UpdateMenuItem();
            }
            grHeader.Height = (Tabs.Count == 0 || currentUserControl == null) ? new GridLength(13) : new GridLength(40);
        }

        public void ChangeDataEditorTabHeader(string fileName)
        {
            // if (string.IsNullOrWhiteSpace(fileName) || DataViewModel.Instance.Tabs == null)
            if (string.IsNullOrWhiteSpace(fileName) || Tabs == null)
                return;

            // var existingTab = DataViewModel.Instance.Tabs.FirstOrDefault(t => t.Content is DataEditor dataEditor && dataEditor.cdbFilePath == fileName);
            var existingTab = Tabs.FirstOrDefault(t => t.Content is DataEditor dataEditor && dataEditor.cdbFilePath == fileName);

            if (existingTab != null)
            {
                existingTab.Header = System.IO.Path.GetFileName(fileName);
            }
        }
        private void UpdateDataEditorTabHeader(string selectedFilePath, DataEditor currentDataEditor)
        {
            CardDataViewModel.Instance.UpdateTabHeader(currentDataEditor, System.IO.Path.GetFileName(selectedFilePath));
        }
        private void UpdateCodeEditorTabHeader(string selectedFilePath, CodeEditor currentCodeEditor)
        {
            CardDataViewModel.Instance.UpdateTabHeader(currentCodeEditor, System.IO.Path.GetFileName(selectedFilePath));
        }
        private void UpdateImageEditorTabHeader(string selectedFilePath, ImageEditor currentImageEditor)
        {
            CardDataViewModel.Instance.UpdateTabHeader(currentImageEditor, System.IO.Path.GetFileName(selectedFilePath));
        }
        public void UpdateMenuItem()
        {
            if (currentUserControl != null)
            {
                DataEditVisibility = (currentUserControl is DataEditor) ? Visibility.Visible : Visibility.Collapsed;
                ScriptEditVisibility = (currentUserControl is CodeEditor) ? Visibility.Visible : Visibility.Collapsed;
                DeckEditVisibility = (currentUserControl is DeckEditor) ? Visibility.Visible : Visibility.Collapsed;
                BanListEditVisibility = (currentUserControl is BanListEditor) ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                DataEditVisibility = Visibility.Collapsed;
                ScriptEditVisibility = Visibility.Collapsed;
                DeckEditVisibility = Visibility.Collapsed;
                BanListEditVisibility = Visibility.Collapsed;
            }
        }

        public void UpdateWindowTitle(string title)
        {
            MainWindowTitle = title;
        }
        public void UpdateWindowSavedFlag(bool isSaved)
        {
            IsSaved = isSaved;
        }
        public void UpdateTabItemHeader(string title)
        {
            var currentTab = Tabs.FirstOrDefault(t => t.Content == currentUserControl);
            if (currentTab == null)
            {
                string defaultTitle = string.Empty;

                if (currentUserControl is BanListEditor) defaultTitle = CMess.BanListEdit.ToText();
                else if (currentUserControl is CodeEditor) defaultTitle = CMess.CodeEdit.ToText();
                else if (currentUserControl is DataEditor) defaultTitle = CMess.DataEdit.ToText();
                else if (currentUserControl is DeckEditor) defaultTitle = CMess.DeckEdit.ToText();
                else if (currentUserControl is ImageEditor) defaultTitle = CMess.ImageEdit.ToText();
                else if (currentUserControl is Home) defaultTitle = CMess.Home.ToText();

                currentTab.Header = defaultTitle;
            }
            else currentTab.Header = title;
        }

        private void MetroWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LoadChatButton();
        }
        #endregion

        #region Event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}