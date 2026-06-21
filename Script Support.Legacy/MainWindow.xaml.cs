using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using System.Data.Common;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Configuration;
using System.ComponentModel;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ScriptSupport.Legacy.Views;
using ScriptSupport.Legacy.Models;
using ScriptSupport.Legacy.Theming;
using ScriptSupport.Legacy.Localization;
using ScriptSupport.Legacy.ViewModels;
using ScriptSupport.Legacy.Helper;
using ScriptSupport.Legacy.Collections;
using ScriptSupport.Legacy.Commands;
using ScriptSupport.Legacy.Services;
using Path = System.IO.Path;
using CMess = ScriptSupport.Legacy.Localization.Language;

namespace ScriptSupport.Legacy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variable
        private double WidthLeft;
        private double WidthRight;
        private double HeightCard;
        private double HeightScrapi;
        private double HeightGrdecs;
        private double HeightTextEditor;
        private Point MousePosition;
        private Point _basePosition;

        private int MaxItemsPerBatch;
        private int itemCountDesc;
        private int itemCountLua;

        private string dataSourceFolder;
        private string dataFilePath;
        private string currentFilePath = null;
        private string currentPassword = null;
        private string currentName = null;
        private string nullImagePath;

        private List<string> keywordsscript;
        private List<string> keywordsdesc;
        private readonly List<string> _validCdbFiles = new List<string>();
        private List<(ulong bit, string name)> listrule, listsetname, listtype, listlinkmarker, listrace, listchar, listattri;

        private bool isAutoSearch;
        private bool isAllowSave;
        private bool isAllowNew;
        private bool _isScrolling;
        private bool isResizing = false;
        public bool isShuttingDown = false;

        private Brush themeColor;
        private OpenFileDialog openFileDialog;
        private DispatcherTimer _autoscrollTimer;
        private LineHighlightRenderer _lineHighlighter;
        private static readonly Regex regex = new Regex(@"[^a-zA-Z0-9]");

        public ObservableCollection<DescInfo> DescItem { get; set; }
        public ObservableCollection<LuaInfo> LuaItems { get; set; }
        public ObservableCollection<ScrapiName> ScrapiNameItem { get; set; }
        public ObservableCollection<ScrapiDesc> ScrapiDescItem { get; set; }


        private TaskCompletionSource<bool> _taskCompletionDesc;
        private TaskCompletionSource<bool> _taskCompletionLua;

        private CancellationTokenSource _tbsearchdescCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _tbsearchluaCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _tbsearch_scrapiyard_nameCancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _tbsearch_scrapiyard_descCancellationTokenSource = new CancellationTokenSource();
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            LoadWindowSettings();

            dataSourceFolder = Properties.Settings.Default.datasource as string;
            if (string.IsNullOrEmpty(dataSourceFolder))
            {
                dataSourceFolder = @"C:\ProjectIgnis";
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    $"{CMess.dataSourceErr.ToText()} {CMess.useDefault.ToText()}", new[] { CMess.ok.ToText() });
            }
            nullImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "unknown.png");
            if (!File.Exists(nullImagePath))
            {
                nullImagePath = "pack://application:,,,/Images/DataEditor/default.png";
            }

            _autoscrollTimer = new DispatcherTimer();
            _autoscrollTimer.Interval = TimeSpan.FromMilliseconds(100);
            _lineHighlighter = new LineHighlightRenderer(textEditor);

            DescItem = new ObservableCollection<DescInfo>();
            LuaItems = new ObservableCollection<LuaInfo>();
            ScrapiNameItem = new ObservableCollection<ScrapiName>();
            ScrapiDescItem = new ObservableCollection<ScrapiDesc>();
            libnamedesc.ItemsSource = DescItem;
            libnamelua.ItemsSource = LuaItems;
            libscrapiyard_name.ItemsSource = ScrapiNameItem;
            libscrapiyard_desc.ItemsSource = ScrapiDescItem;

            textEditor.TextArea.TextView.BackgroundRenderers.Add(_lineHighlighter);

            this.InputBindings.Add(new KeyBinding(ApplicationCommands.New, Key.N, ModifierKeys.Control));
            this.InputBindings.Add(new KeyBinding(ApplicationCommands.Open, Key.O, ModifierKeys.Control));
            this.InputBindings.Add(new KeyBinding(ApplicationCommands.Save, Key.S, ModifierKeys.Control));
            this.InputBindings.Add(new KeyBinding(ApplicationCommands.SaveAs, Key.S, ModifierKeys.Control | ModifierKeys.Shift));

            this.PreviewMouseDown += MainWindow_PreviewMouseDown;
        }

        #region Load
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadControlsSize();
            LoadFont();
            LoadConfig();
            CheckGit();
            await Task.WhenAll(CheckValidCdbFilesAsync(), LoadTermiKeywordsAsync(), LoadScrapiKeywordsAsync(), SetImageSource());
            await Task.WhenAll(LoadListInfo());
            LoadInfoLabel();
        }

        #region Load Setting
        private bool CheckIfChildWindow()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Contains("-isChildWindow") && args[Array.IndexOf(args, "-isChildWindow") + 1] == "true")
            {
                return true;
            }
            else return false;
        }
        private void LoadWindowSettings()
        {
            // Lấy vị trí và kích thước từ setting
            string mainWindowsSettings = Properties.Settings.Default.MainWindow;
            if (!string.IsNullOrEmpty(mainWindowsSettings))
            {
                string[] values = mainWindowsSettings.Split(',');
                if (values.Length == 4 &&
                    double.TryParse(values[0], out double left) &&
                    double.TryParse(values[1], out double top) &&
                    double.TryParse(values[2], out double width) &&
                    double.TryParse(values[3], out double height))
                {
                    this.Left = left;
                    this.Top = top;
                    this.Width = width;
                    this.Height = height;
                }
            }
        }
        private void LoadControlsSize()
        {
            // Lấy kích thước control từ config
            string mainControlsSize = Properties.Settings.Default.controlsize;
            if (!string.IsNullOrEmpty(mainControlsSize))
            {
                var sizes = mainControlsSize.Split(',');
                if (sizes.Length == 3 &&
                    double.TryParse(sizes[0], out double gridleft) &&
                    double.TryParse(sizes[1], out double tbboxcard) &&
                    double.TryParse(sizes[2], out double tbboxdesc)
                    )
                {
                    grleft.Width = new GridLength(gridleft);
                    grcard.Height = new GridLength(tbboxcard);
                    grdecs.Height = new GridLength(tbboxdesc);
                    cardinfo.Width = gridleft;
                    UpdateTabHeaderLeft(gridleft);
                    UpdateTabHeaderHeights(tbboxdesc);
                }
            }
        }
        #endregion

        #region Load Configuration
        private void LoadFont()
        {
            
        }

        private void LoadConfig()
        {



            #region Theme



            panelnavi.Visibility = CheckIfChildWindow() ? Visibility.Visible : Visibility.Collapsed;
            #endregion


            LoadLanguage();

            #region HightLight
            try
            {
                //HighLightService highLightService = new HighLightService();
                //string highlightFilePath = highLightService.HighLightFilePath();
                //using (Stream stream = File.OpenRead(highlightFilePath))
                //using (XmlTextReader reader = new XmlTextReader(stream))
                //{
                //    tbscrapiyard.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                //}
                //using (Stream stream = File.OpenRead(highlightFilePath))
                //using (XmlTextReader reader = new XmlTextReader(stream))
                //{
                //    textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                //}

            }
            catch (FileNotFoundException fnfEx)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.hightLightNotExist.ToText()} {fnfEx.Message}", new[] { CMess.ok.ToText() });
            }
            catch (XmlException xmlEx)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorRead.ToText()} {xmlEx.Message}", new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            #endregion

            #region Max Item
            //MaxItemsPerBatch = SettingService.GetIntSetting("maxItem", 100);
            #endregion

            #region List Header
            //bool liscardtop = SettingService.GetBoolSetting("listheadercard", false);
            //bool lisscrtop = SettingService.GetBoolSetting("listheaderscrapi", false);
            //bool lisdesctop = SettingService.GetBoolSetting("listheaderdesc", false);

            //tablist.TabStripPlacement = liscardtop ? Dock.Top : Dock.Bottom;
            //tablistScrapi.TabStripPlacement = lisscrtop ? Dock.Top : Dock.Bottom;
            //tabcresult.TabStripPlacement = lisdesctop ? Dock.Left : Dock.Right;

            //NavigationRailAssist.SetSelectionCornerRadius(tablist, liscardtop ? new System.Windows.CornerRadius(10, 10, 2, 2) : new System.Windows.CornerRadius(2, 2, 10, 10));
            //NavigationRailAssist.SetSelectionCornerRadius(tablistScrapi, lisscrtop ? new System.Windows.CornerRadius(10, 10, 2, 2) : new System.Windows.CornerRadius(2, 2, 10, 10));
            #endregion

            #region Wordwrap
            //bool wordWrap = SettingService.GetBoolSetting("wordwrap", false);
            //textEditor.WordWrap = wordWrap;
            //tbscrapiyard.WordWrap = wordWrap;
            #endregion

            #region Auto Search
            //isAutoSearch = SettingService.GetBoolSetting("autosearch", false);
            #endregion

            #region Allow Save
            //isAllowSave = SettingService.GetBoolSetting("allowsave", false);
            //menuitemsaveas.Visibility = isAllowSave ? Visibility.Visible : Visibility.Collapsed;
            #endregion

            #region Allow New
            //isAllowNew = SettingService.GetBoolSetting("allownew", false);
            //menuitemnew.Visibility = isAllowNew ? Visibility.Visible : Visibility.Collapsed;
            //TabItem tabnew = tablist.Items[3] as TabItem;
            //tabnew.Visibility = isAllowNew ? Visibility.Visible : Visibility.Collapsed;
            #endregion

            UpdateTabHeaderLeft(grleft.ActualWidth);
            UpdateTabHeaderHeights(grdecs.ActualHeight);

        }

        private void LoadLanguage()
        {
            string currentLanguage = Properties.Settings.Default.Language;
            LanguageManager.Initialize();
            LanguageManager.Instance.LoadLanguage(currentLanguage);
        }

        #endregion

        #region Check Git
        private void CheckGit()
        {
            dataFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            if (!Directory.Exists(dataFilePath))
            {
                Directory.CreateDirectory(dataFilePath);
            }

            string scrapiyardPath = System.IO.Path.Combine(dataFilePath, "scrapiyard");
            string scrapibookPath = System.IO.Path.Combine(dataFilePath, "scrapibook");
            string CardDataPath = System.IO.Path.Combine(dataFilePath, "CardData");

            if (!(Directory.Exists(scrapiyardPath) && GitHubService.IsGitRepository(scrapiyardPath) &&
                Directory.Exists(scrapibookPath) && GitHubService.IsGitRepository(scrapibookPath) &&
                Directory.Exists(CardDataPath) && GitHubService.IsGitRepository(CardDataPath)))
            {
                tbsearch_scrapiyard_name.IsEnabled = false;
                tbsearch_scrapiyard_desc.IsEnabled = false;
                tbscrapiyard.IsEnabled = false;
                panelkonami.Visibility = Visibility.Collapsed;
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    CMess.dataSourceMiss.ToText(), new[] { CMess.ok.ToText() });
            }
        }

        #endregion

        #region Load CDB
        private async Task CheckValidCdbFilesAsync()
        {
            string projectFolder = dataSourceFolder;
            // Kiểm tra xem projectFolder có phải là đường dẫn hợp lệ và có tồn tại thư mục không
            if (string.IsNullOrEmpty(projectFolder) || !Directory.Exists(projectFolder))
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    CMess.dataSourceMiss.ToText(), new[] { CMess.ok.ToText() });
                tbsearchdesc.IsEnabled = false;
                tbsearchlua.IsEnabled = false;
                tbdecs.IsEnabled = false;
                return;
            }

            var cdbFiles = Directory.GetFiles(projectFolder, "*.cdb", SearchOption.AllDirectories);
            if (cdbFiles.Length == 0)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noFileFound.ToText(), new[] { CMess.ok.ToText() });
                tbsearchdesc.IsEnabled = false;
                tbsearchlua.IsEnabled = false;
                tbdecs.IsEnabled = false;
                return;
            }

            // Khởi tạo SemaphoreSlim với số lượng tối đa task đồng thời là 15
            var semaphore = new SemaphoreSlim(15);

            // Tạo danh sách các Task, mỗi task xử lý một file cdb
            var tasks = cdbFiles.Select(async cdbFile =>
            {
                await semaphore.WaitAsync(); // Đợi cho đến khi có không gian cho một task mới
                try
                {
                    using (var connection = new SQLiteConnection($"Data Source={cdbFile};Version=3;"))
                    {
                        try
                        {
                            await connection.OpenAsync();

                            string checkTableAndColumnsQuery = @"SELECT COUNT(*)
                                FROM sqlite_master WHERE type='table' AND name='texts'
                                AND EXISTS (SELECT 1 FROM pragma_table_info('texts') WHERE name IN ('id', 'name', 'desc'))";
                            using (var checkCommand = new SQLiteCommand(checkTableAndColumnsQuery, connection))
                            {
                                var result = await checkCommand.ExecuteScalarAsync();
                                if (Convert.ToInt32(result) == 0) return; // Nếu không tìm thấy bảng 'texts' hoặc thiếu cột 'id', 'name', 'desc', bỏ qua file này
                                // Nếu file hợp lệ, thêm vào danh sách và tạo chỉ mục
                                _validCdbFiles.Add(cdbFile);
                                await CreateIndexesIfNeededAsync(cdbFile); // Tạo chỉ mục
                            }
                        }
                        catch (SQLiteException ex)
                        {
                            LogError($"Error connecting to file {cdbFile}: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    semaphore.Release(); // Giải phóng để cho phép một task khác chạy
                }
            }).ToList(); // Chuyển đổi các Task thành danh sách
            // Chờ tất cả các Task hoàn thành
            await Task.WhenAll(tasks);
        }
        private async Task CreateIndexesIfNeededAsync(string cdbFilePath)
        {
            using (var connection = new SQLiteConnection($"Data Source={cdbFilePath};Version=3;"))
            {
                await connection.OpenAsync();
                // Tạo chỉ mục cho cột "id" nếu chưa tồn tại
                string createIdIndex = "CREATE INDEX IF NOT EXISTS idx_texts_id ON texts(id);";
                using (var command = new SQLiteCommand(createIdIndex, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                // Tạo chỉ mục cho cột "name" nếu chưa tồn tại
                string createNameIndex = "CREATE INDEX IF NOT EXISTS idx_texts_name ON texts(name);";
                using (var command = new SQLiteCommand(createNameIndex, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                // Tạo chỉ mục cho cột "desc" nếu chưa tồn tại
                string createDescIndex = "CREATE INDEX IF NOT EXISTS idx_texts_desc ON texts(desc);";
                using (var command = new SQLiteCommand(createDescIndex, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        #endregion

        #region Load KeyWord
        private async Task LoadTermiKeywordsAsync()
        {
            string language = ConfigViewModel.Instance.userSetting.Language;
            keywordsdesc = new List<string>();
            // Đọc thuật ngữ từ terminology.txt
            string terminologyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"data\CardData\Language\{language}\scriptinfo", "terminology.txt");
            if (File.Exists(terminologyFilePath))
            {
                // Sử dụng File Read Lines
                var terminologyKeywords = await Task.Run(() => File.ReadLines(terminologyFilePath).ToList());
                keywordsdesc.AddRange(terminologyKeywords);
            }
        }

        private async Task LoadScrapiKeywordsAsync()
        {
            keywordsscript = new List<string>();
            var keywordsscriptBag = new ConcurrentBag<string>();

            // Đọc tên các file .yml từ thư mục scrapiyard và các thư mục con
            string scrapiyardFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "scrapiyard");

            if (Directory.Exists(scrapiyardFolderPath))
            {
                // Giới hạn số tiến trình đồng thời, ví dụ: chỉ cho phép 5 tiến trình chạy đồng thời
                int maxDegreeOfParallelism = 5;
                var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

                var tasks = new List<Task>();

                // Sử dụng Task.WhenAll để xử lý đồng thời
                foreach (var file in Directory.EnumerateFiles(scrapiyardFolderPath, "*.yml", SearchOption.AllDirectories))
                {
                    var task = ProcessFileAsync(file, semaphore, keywordsscriptBag);
                    tasks.Add(task);
                }
                // Đợi cho tất cả các task hoàn thành
                await Task.WhenAll(tasks);
            }
            keywordsscript = keywordsscriptBag.ToList();
        }
        private async Task ProcessFileAsync(string file, SemaphoreSlim semaphore, ConcurrentBag<string> keywordsscriptBag)
        {
            // Đợi để có thể bắt đầu tiến trình nếu số lượng tiến trình đồng thời đạt giới hạn
            await semaphore.WaitAsync();
            try
            {
                // Lấy tên file không có phần mở rộng
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                if (!string.IsNullOrWhiteSpace(fileNameWithoutExtension))
                {
                    keywordsscriptBag.Add(fileNameWithoutExtension);
                }
            }
            finally
            {
                // Giải phóng semaphore khi hoàn thành
                semaphore.Release();
            }
        }

        #endregion

        #region Load List Information
        private async Task LoadListInfo()
        {
            var tasks = new[]
            {
                FindIInfoService.LoadCardInfo("rule", true),
                FindIInfoService.LoadCardInfo("setname", true),
                FindIInfoService.LoadCardInfo("type", false),
                FindIInfoService.LoadCardInfo("linkmarker", false),
                FindIInfoService.LoadCardInfo("race", false),
                FindIInfoService.LoadCardInfo("character", false),
                FindIInfoService.LoadCardInfo("attribute", false)
            };
            var results = await Task.WhenAll(tasks);
            listrule = results[0].Data;
            listsetname = results[1].Data;
            listtype = results[2].Data;
            listlinkmarker = results[3].Data;
            listrace = results[4].Data;
            listchar = results[5].Data;
            listattri = results[6].Data;

            var errorList = results.Where(r => !r.Success).Select(r => r.ErrorMessage).Distinct().ToList();
            if (errorList.Any())
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, string.Join("\n", errorList), new[] { CMess.ok.ToText() });
            }
        }
        private void LoadInfoLabel()
        {
            tblLiRating.Text = $"{CMess.LinkRat.ToText()}: ";
            tblLinkArr.Text = $"{CMess.LinkArr.ToText()}: ";
            tllPenlabel.Text = $"{CMess.penScaleLabel.ToText()}: ";
            tblatk.Text = $"{CMess.cardatk.ToText()}: ";
            tbldef.Text = $"{CMess.carddef.ToText()}: ";
        }
        #endregion

        #endregion

        #region Menu

        #region Menu Item
        private void CommandNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (isAllowNew)
            {
                NewLua(true);
            }
            else
            {
                CustomMessageBox.Show("BURH", "LOL, LMAO", themeColor);
            }
        }
        private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenLua();
        }
        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (isAllowSave)
            {
                SaveLua();
            }
            else
            {
                CustomMessageBox.Show("BURH", "LOL, LMAO", themeColor);
            }
        }
        private void CommandSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (isAllowSave)
            {
                SaveAs();
            }
            else
            {
                CustomMessageBox.Show("BURH", "LOL, LMAO", themeColor);
            }
        }
        private void Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void menuitemExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void menuitemConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ConfigEditor configEditor = new ConfigEditor();
            configEditor.ConfigChanged += ConfigEditor_ConfigChanged;
            configEditor.ShowDialog();
            configEditor.ShowInTaskbar = false;
        }
        private void menuItemScrapiBook_Click(object sender, RoutedEventArgs e)
        {
            ScrapiBook book = new ScrapiBook();
            book.Show();
            book.ShowInTaskbar = true;
        }
        private async void chkupdate_Click(object sender, RoutedEventArgs e)
        {
            #region scrapiyard
            //// Đọc URL của các kho Git scrapiyard từ app.config
            //string scrapiyardURL = ConfigurationManager.AppSettings["scrapiyardURL"];
            //string scrapiyardPath = System.IO.Path.Combine(dataFilePath, "scrapiyard");

            //try
            //{
            //    this.Cursor = Cursors.Wait;
            //    bool hasUpdatescrapiyard = await GitHubService.CheckForUpdatesAsync(scrapiyardPath, scrapiyardURL);
            //    if (hasUpdatescrapiyard)
            //    {
            //        CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, $"Scrapiyard {CMess.updateCompe.ToText()}", new[] { CMess.ok.ToText() });
            //    }
            //    else
            //    {
            //        CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, CMess.noUpdate.ToText(), new[] { CMess.ok.ToText() });
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            //}
            //finally
            //{
            //    this.Cursor = null;
            //}

            ////// Kiểm tra kho Git 'scrapiyard'
            ////if (!GitHubService.IsGitRepository(scrapiyardPath))
            ////{
            ////    MessageBox.Show("Scrapiyard folder is not a valid Git repository. Cloning repository now.", "Not a Git Repository", MessageBoxButton.OK, MessageBoxImage.Information);
            ////    GitHubService.CloneRepository(scrapiyardURL, scrapiyardPath);
            ////}
            ////else
            ////{
            ////    if (GitHubService.CheckForUpdates(scrapiyardPath))
            ////    {
            ////        var result = MessageBox.Show("Updated version is available for Scrapiyard, download it?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
            ////        if (result == MessageBoxResult.Yes)
            ////        {
            ////            GitHubService.DownloadAndUpdate(scrapiyardPath);
            ////        }
            ////    }
            ////    else
            ////    {
            ////        MessageBox.Show("No updates found for Scrapiyard.", "No Updates", MessageBoxButton.OK, MessageBoxImage.Information);
            ////    }
            ////}
            #endregion

            #region scrapibook
            //// Đọc URL của các kho Git scrapibook từ app.config
            //string scrapibookURL = ConfigurationManager.AppSettings["scrapibookURL"];
            //string scrapibookPath = System.IO.Path.Combine(dataFilePath, "scrapibook");

            //try
            //{
            //    this.Cursor = Cursors.Wait;
            //    bool hasUpdatescrapibook = await GitHubService.CheckForUpdatesAsync(scrapibookPath, scrapibookURL);
            //    if (hasUpdatescrapibook)
            //    {
            //        CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, $"Scrapibook {CMess.updateCompe.ToText()}", new[] { CMess.ok.ToText() });
            //    }
            //    else
            //    {
            //        CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, CMess.noUpdate.ToText(), new[] { CMess.ok.ToText() });
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            //}
            //finally
            //{
            //    this.Cursor = null;
            //}

            //// Kiểm tra kho Git 'scrapibook'
            ////if (!GitHubService.IsGitRepository(scrapibookPath))
            ////{
            ////    MessageBox.Show("Scrapibook folder is not a valid Git repository. Cloning repository now.", "Not a Git Repository", MessageBoxButton.OK, MessageBoxImage.Information);
            ////    GitHubService.CloneRepository(scrapibookURL, scrapibookPath);
            ////}
            ////else
            ////{
            ////    if (GitHubService.CheckForUpdates(scrapibookPath))
            ////    {
            ////        var result = MessageBox.Show("Updated version is available for Scrapibook, download it?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
            ////        if (result == MessageBoxResult.Yes)
            ////        {
            ////            GitHubService.DownloadAndUpdate(scrapibookPath);
            ////        }
            ////    }
            ////    else
            ////    {
            ////        MessageBox.Show("No updates found for Scrapibook.", "No Updates", MessageBoxButton.OK, MessageBoxImage.Information);
            ////    }
            ////}
            #endregion

            #region yamlyugi
            //bool isHasUpdate = false;
            //string yamlyugiURL = ConfigurationManager.AppSettings["yamlyugiURL"];
            //string yamlyugiPath = System.IO.Path.Combine(dataFilePath, "yaml-yugi");

            //if (!GitHubService.IsGitRepository(yamlyugiPath))
            //{
            //    MessageBox.Show("yaml-yugi folder is not a valid Git repository. Cloning repository now.", "Not a Git Repository", MessageBoxButton.OK, MessageBoxImage.Information);
            //    // CloneRepository(scrapiyardURL, scrapiyardPath);
            //    bool isCloneSuccessful = GitHubService.CloneRepository(yamlyugiURL, yamlyugiPath);
            //    if (isCloneSuccessful) isHasUpdate = true;
            //}
            //else
            //{
            //    // bool hasUpdates = CheckForUpdates(scrapiyardPath);
            //    if (GitHubService.CheckForUpdates(yamlyugiPath))
            //    {
            //        var result = MessageBox.Show("Updated version is available for yaml-yugi, download it?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
            //        if (result == MessageBoxResult.Yes)
            //        {
            //            // DownloadAndUpdate(scrapiyardPath);
            //            bool isDownloadSuccessful = GitHubService.DownloadAndUpdate(yamlyugiPath);
            //            if (isDownloadSuccessful) isHasUpdate = true;
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("No updates found for yaml-yugi.", "No Updates", MessageBoxButton.OK, MessageBoxImage.Information);
            //    }
            //}

            //if (isHasUpdate)
            //{
            //    var result = MessageBox.Show($"Latest version of yaml-yugi is ready, extract Konami_ID database? (This may take a long time)\nThis feature is not recommended for regular users.", "Extract Konami_ID", MessageBoxButton.YesNo, MessageBoxImage.Information);
            //    if (result == MessageBoxResult.Yes)
            //        GetIDService.GetID();
            //}
            #endregion

            #region CardData
            //string CardDataURL = ConfigurationManager.AppSettings["CardDataURL"];
            //string CardDataPath = System.IO.Path.Combine(dataFilePath, "CardData");

            //try
            //{
            //    this.Cursor = Cursors.Wait;
            //    bool hasUpdateCardData = await GitHubService.CheckForUpdatesAsync(CardDataPath, CardDataURL);

            //    if (hasUpdateCardData)
            //    {
            //        CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, $"CardData {CMess.updateCompe.ToText()}", new[] { CMess.ok.ToText() });
            //    }
            //    else
            //    {
            //        CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, CMess.noUpdate.ToText(), new[] { CMess.ok.ToText() });
            //    }
            //}
            //catch (Exception ex)
            //{
            //    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            //}
            //finally
            //{
            //    this.Cursor = null;
            //}

            ////if (!GitHubService.IsGitRepository(CardDataPath))
            ////{
            ////    MessageBox.Show("CardData folder is not a valid Git repository. Cloning repository now.", "Not a Git Repository", MessageBoxButton.OK, MessageBoxImage.Information);
            ////    // CloneRepository(scrapiyardURL, scrapiyardPath);
            ////    GitHubService.CloneRepository(CardDataURL, CardDataPath);
            ////}
            ////else
            ////{
            ////    // bool hasUpdates = CheckForUpdates(scrapiyardPath);
            ////    if (GitHubService.CheckForUpdates(CardDataPath))
            ////    {
            ////        var result = MessageBox.Show("Updated version is available for CardData, download it?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
            ////        if (result == MessageBoxResult.Yes)
            ////        {
            ////            // DownloadAndUpdate(scrapiyardPath);
            ////            GitHubService.DownloadAndUpdate(CardDataPath);
            ////        }
            ////    }
            ////    else
            ////    {
            ////        MessageBox.Show("No updates found for CardData.", "No Updates", MessageBoxButton.OK, MessageBoxImage.Information);
            ////    }
            ////}
            #endregion
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
            about.ShowInTaskbar = false;
        }
        private void btntheme_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    if (isDarkTheme == false)
            //    {
            //        Properties.Settings.Default.darkmode = true;
            //    }
            //    else
            //    {
            //        Properties.Settings.Default.darkmode = false;
            //    }
            //    Properties.Settings.Default.Save();
            //}
            //catch (Exception ex)
            //{
            //    LogError($"An error occurred while updating settings: {ex.Message}");
            //}
        }

        #endregion

        private void NewLua(bool iscreate)
        {
            // Tạo đối tượng SaveFileDialog
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = $"{CMess.CardScript.ToText()}(*.lua)|*.lua|{CMess.txtfile.ToText()}(*.txt)|*.txt|{CMess.mdfile.ToText()}(*.md)|*.md|{CMess.logfile.ToText()}(*.log)|*.log|{CMess.deckfile.ToText()}(*.ydk)|*.ydk|{CMess.yamlfile.ToText()}(*.yml)|*.yml|{CMess.configfile.ToText()}(*.conf)|*.conf|{CMess.allfile.ToText()}(*.*)|*.*",
                DefaultExt = ".lua", // Phần mở rộng mặc định
                AddExtension = true,
                OverwritePrompt = true
            };
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                // grleft.Width = new GridLength(50);
                // grdecs.Height = new GridLength(50);
                tablist.SelectedIndex = 3;
                // Lấy đường dẫn file mà người dùng đã chọn
                string filePath = saveFileDialog.FileName;
                // Danh sách các phần mở rộng hợp lệ
                // var validExtensions = new[] { ".txt", ".csv", ".md", ".ini", ".json", ".xml", ".log", ".lua" };
                // Kiểm tra nếu file chưa có phần mở rộng hợp lệ
                // bool isValidExtension = validExtensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
                // Nếu không có phần mở rộng hợp lệ, thêm phần mở rộng ".lua"
                //if (!isValidExtension)
                //{
                //    MessageBoxResult resultExtension = MessageBox.Show("This file does not have a valid extension. Do you want to automatically add the '.lua' extension?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                //    if (resultExtension == MessageBoxResult.No)
                //    {
                //        return;
                //    }
                //    filePath += ".lua";
                //}
                try
                {
                    // Kiểm tra nếu file đã tồn tại
                    //if (File.Exists(filePath))
                    //{
                    //    MessageBoxResult overwriteResult = MessageBox.Show($"File {filePath} already exists. Do you want to overwrite this file?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    //    if (overwriteResult == MessageBoxResult.No)
                    //    {
                    //        MessageBox.Show("Cancel file creation.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                    //        return; // Nếu người dùng không muốn ghi đè, hủy thao tác
                    //    }
                    //}
                    //else
                    //{
                        File.Create(filePath).Close(); // Tạo file trống
                    //}
                    // Thêm tên file vào libnametext
                    // Tạo ListBoxItem mới với Content là tên file, và Tag là tuple chứa đường dẫn file
                    string fileName = Path.GetFileName(filePath); // Lấy tên file (bao gồm phần mở rộng, không có đường dẫn)
                    var listBoxItem = new ListBoxItem
                    {
                        Content = fileName,
                        Tag = filePath
                    };
                    libnamenew.Items.Add(listBoxItem);
                    currentFilePath = filePath;

                    if (iscreate)
                    {
                        string luaCode = string.Empty;
                        if (filePath.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
                        {
                            luaCode =
                                "-- add OCG name\n" +
                                "-- add TCG name\n" +
                                "local s,id,o=GetID()\n" +
                                "function s.initial_effect(c)\n" +
                                "\t\n" +
                                "end";
                            File.WriteAllText(filePath, luaCode);
                        }

                        var resultopen = CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                            string.Format(CMess.creaSuc.ToText(), 1, CMess.File.ToText()), new[] { CMess.yes.ToText(), CMess.no.ToText() });
                        if (resultopen == 0)
                        {
                            textEditor.Text = luaCode;
                            textEditor.Focus();
                            int tabPosition = luaCode.IndexOf('\t');
                            if (tabPosition != -1)
                            {
                                textEditor.CaretOffset = tabPosition + 1; // Đặt con trỏ ngay sau tab
                            }
                        }
                        else
                        {
                            textEditor.Focus();
                        }
                    }
                    // MessageBox.Show($"File {filePath} has been created.", "File created successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (UnauthorizedAccessException ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
                catch (IOException ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
        }
        private void OpenLua()
        {
            // Tạo đối tượng OpenFileDialog
            openFileDialog = new OpenFileDialog
            {
                Filter = $"{CMess.CardScript.ToText()}(*.lua)|*.lua|{CMess.txtfile.ToText()}(*.txt)|*.txt|{CMess.mdfile.ToText()}(*.md)|*.md|{CMess.logfile.ToText()}(*.log)|*.log|{CMess.deckfile.ToText()}(*.ydk)|*.ydk|{CMess.yamlfile.ToText()}(*.yml)|*.yml|{CMess.configfile.ToText()}(*.conf)|*.conf|{CMess.allfile.ToText()}(*.*)|*.*",
                Multiselect = true
            };
            // Mở hộp thoại chọn tệp
            if (openFileDialog.ShowDialog() == true)
            {
                tablist.SelectedIndex = 2;

                // Clear current ListBox before adding new files
                libnametext.Items.Clear();

                // Duyệt qua tất cả các file đã chọn và thêm chỉ tên file vào ListBox
                foreach (var filePath in openFileDialog.FileNames)
                {
                    var listBoxItem = new ListBoxItem
                    {
                        Content = Path.GetFileName(filePath), // Hiển thị tên file (bao gồm phần mở rộng)
                        Tag = (filePath)
                    };
                    libnametext.Items.Add(listBoxItem);  // Thêm vào ListBox
                    // libnametext.Items.Add(Path.GetFileName(filePath));  // Thêm chỉ tên file vào ListBox
                }

                // Nếu chỉ có một tệp được chọn, hiển thị nội dung vào TextEditor
                if (openFileDialog.FileNames.Length == 1)
                {
                    currentFilePath = openFileDialog.FileNames[0];
                    libnametext.SelectedIndex = 0;
                    string fileContent = File.ReadAllText(currentFilePath);
                    textEditor.Text = fileContent;
                }
                else
                {
                    textEditor.Text = string.Empty;  // Nếu có nhiều tệp, không hiển thị nội dung nào
                }

            }
        }
        private void SaveLua()
        {
            // Kiểm tra đường dẫn file
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                if (string.IsNullOrWhiteSpace(textEditor.Text))
                {
                    var resultsave = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, CMess.confirmSaveBlank.ToText(), new[] { CMess.yes.ToText(), CMess.no.ToText() });
                    if (resultsave != 0) return;
                }
                // Lưu nội dung từ TextEditor vào file hiện tại
                try
                {
                    File.WriteAllText(currentFilePath, textEditor.Text);
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.saveSuc.ToText(), 1, CMess.File.ToText()), new[] { CMess.ok.ToText() });
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorSaveScript.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                NewLua(false);
                SaveLua();
            }
        }
        private void SaveAs()
        {
            // Kiểm tra xem nội dung tệp có trống không
            if (string.IsNullOrWhiteSpace(textEditor.Text))
            {
                var resultsave = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, CMess.confirmSaveBlank.ToText(), new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (resultsave != 0) return;
            }
            // Hiển thị hộp thoại SaveFileDialog để chọn vị trí và tên tệp mới
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = $"{CMess.CardScript.ToText()}(*.lua)|*.lua|{CMess.txtfile.ToText()}(*.txt)|*.txt|{CMess.mdfile.ToText()}(*.md)|*.md|{CMess.logfile.ToText()}(*.log)|*.log|{CMess.deckfile.ToText()}(*.ydk)|*.ydk|{CMess.yamlfile.ToText()}(*.yml)|*.yml|{CMess.configfile.ToText()}(*.conf)|*.conf|{CMess.allfile.ToText()}(*.*)|*.*", // Đặt bộ lọc tệp .lua
                DefaultExt = ".lua", // Phần mở rộng mặc định
                AddExtension = true,
                OverwritePrompt = true
            };
            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                string newFilePath = saveFileDialog.FileName;
                // Kiểm tra nếu tệp đã tồn tại
                //if (File.Exists(newFilePath))
                //{
                //    MessageBoxResult overwriteResult = MessageBox.Show("Tệp này đã tồn tại. Bạn có muốn ghi đè lên tệp này?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                //    if (overwriteResult == MessageBoxResult.No)
                //    {
                //        MessageBox.Show("Hủy lưu file.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                //        return;
                //    }
                //}
                try
                {
                    // Lưu nội dung vào tệp mới đã chọn
                    File.WriteAllText(newFilePath, textEditor.Text);
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.saveSuc.ToText(), 1, CMess.File.ToText()), new[] { CMess.ok.ToText() });
                    currentFilePath = newFilePath; // Cập nhật đường dẫn tệp hiện tại
                }
                catch (UnauthorizedAccessException ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, 
                        $"{CMess.errorSaveScript.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
                catch (IOException ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, 
                        $"{CMess.errorSaveScript.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorSaveScript.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                // MessageBox.Show("Hủy lưu file.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void bordernavigate_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        #endregion

        #region Change
        private async void ConfigEditor_ConfigChanged()
        {
            LoadFont();
            LoadConfig();
            await Task.WhenAll(LoadTermiKeywordsAsync(), LoadScrapiKeywordsAsync());
            await Task.WhenAll(LoadListInfo());
            LoadInfoLabel();

        }
        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!suggestionDescList.IsMouseOver)
            {
                suggestionDescList.Visibility = Visibility.Collapsed;
            }
            if (!suggestionScriptList.IsMouseOver)
            {
                suggestionScriptList.Visibility = Visibility.Collapsed;
            }
            if (!suggestionscrapiList.IsMouseOver)
            {
                suggestionscrapiList.Visibility = Visibility.Collapsed;
            }
            if (!suggestionscrapiListdesc.IsMouseOver)
            {
                suggestionscrapiListdesc.Visibility = Visibility.Collapsed;
            }

            if (!konamipopup.IsMouseOver && !iconpopup.IsMouseOver)
            {
                konamipopup.IsOpen = false;
            }

        }
        private void Instance_ThemeChanged(object sender, string newTheme)
        {
            var oldTheme = this.Resources.MergedDictionaries
                   .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("MaterialDesignColor"));
            if (oldTheme != null)
            {
                // this.Resources.MergedDictionaries.Remove(oldTheme);
            }
            Uri themeUri = new Uri($"pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.{newTheme}.xaml", UriKind.Absolute);
            ResourceDictionary newResource = new ResourceDictionary { Source = themeUri };
            this.Resources.MergedDictionaries.Add(newResource);
        }

        #region Window Change
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (isShuttingDown)
            {
                e.Cancel = true;
                return;
            }
            if (CheckIfChildWindow())
            {
                return;
            }
            if (this.WindowStyle == WindowStyle.SingleBorderWindow || this.WindowStyle == WindowStyle.ThreeDBorderWindow)
            {
                Properties.Settings.Default.MainWindow = $"{this.Left},{this.Top},{this.Width},{this.Height}";
                Properties.Settings.Default.controlsize = $"{grleft.Width},{grcard.Height},{grdecs.Height}";
                Properties.Settings.Default.Save();
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            SaveWindowsSettings();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveWindowsSettings();
        }
        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                int fontsize = (int)ConfigViewModel.Instance.displaySetting.FontSize;
                if (e.Delta > 0)
                {
                    Properties.Settings.Default.FontSize = fontsize + 1;
                }
                else
                {
                    Properties.Settings.Default.FontSize = fontsize - 1;
                }
                Properties.Settings.Default.Save();
                LoadFont();
                e.Handled = true;
            }
        }
        private void SaveWindowsSettings()
        {
            Properties.Settings.Default.MainWindow = $"{this.Left},{this.Top},{this.Width},{this.Height}";
            Properties.Settings.Default.Save();
        }

        private void UpdateConfig(string key, string value)
        {
            try
            {
                if (Properties.Settings.Default.Properties[key] != null)
                {
                    Properties.Settings.Default[key] = value;
                }
                else
                {
                    SettingsProperty newProperty = new SettingsProperty(key)
                    {
                        PropertyType = typeof(string),
                        DefaultValue = value
                    };
                    Properties.Settings.Default.Properties.Add(newProperty);
                }
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                LogError($"An error occurred while updating settings: {ex.Message}");
            }
        }
        #endregion

        #region Controls Change

        #region Separator Grleft
        private void separatorGrleft_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                var mousePosition = Mouse.GetPosition(this);
                double deltaY = mousePosition.Y - MousePosition.Y;
                // Cập nhật chiều cao của grcard
                double newHeightCardBox = HeightCard + deltaY;
                double newHeightScrapiBox = HeightScrapi - deltaY;
                // Đảm bảo không vượt quá chiều cao tối thiểu
                if (newHeightCardBox > 0 && newHeightScrapiBox > 0)
                {
                    int newHeight = (int)newHeightCardBox;
                    grcard.Height = new GridLength(newHeight);
                }
                // SaveControlSetting();
            }
        }
        private void separatorGrleft_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isResizing = true;
                HeightCard = grcard.ActualHeight;
                HeightScrapi = grscrapi.ActualHeight;
                MousePosition = Mouse.GetPosition(this);
                Mouse.Capture(separatorGrleft);
            }
        }
        #endregion

        #region Separator Center
        private void separatorCenter_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                var mousePosition = Mouse.GetPosition(this);
                double deltaX = mousePosition.X - MousePosition.X;
                // Cập nhật bề rộng của cột trái và phải
                double newWidthLeft = WidthLeft + deltaX;
                double newWidrgRight = WidthRight - deltaX;
                // Đảm bảo không vượt quá bề rộng tối thiểu
                if (newWidthLeft > 0 && newWidrgRight > 0)
                {
                    int newWidth = (int)newWidthLeft;
                    grleft.Width = new GridLength(newWidth);
                    cardinfo.Width = newWidth;
                    UpdateTabHeaderLeft(newWidth);
                }
                // SaveControlSetting();
            }
        }
        private void separatorCenter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isResizing = true;
                WidthLeft = grleft.ActualWidth;
                WidthRight = grright.ActualWidth;
                MousePosition = Mouse.GetPosition(this);
                Mouse.Capture(separatorCenter);
            }
        }
        #endregion

        #region Separator Grright
        private void separatorGrRight_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                var mousePosition = Mouse.GetPosition(this);
                double deltaY = mousePosition.Y - MousePosition.Y;
                // Cập nhật chiều cao của RichTextBox và TextEditor
                double newHeightGrdecs = HeightGrdecs + deltaY;
                double newHeightTextEditor = HeightTextEditor - deltaY;
                // Đảm bảo không vượt quá chiều cao tối thiểu
                if (newHeightGrdecs > 0 && newHeightTextEditor > 0)
                {
                    int newHeight = (int)newHeightGrdecs;
                    grdecs.Height = new GridLength(newHeight);
                    //string key = "controlsize";
                    //string value = $"{grleft.Width},{newHeight.ToString()}";
                    //UpdateConfig(key, value);
                    UpdateTabHeaderHeights(newHeight);
                }
                // SaveControlSetting();
            }
        }
        private void separatorGrRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isResizing = true;
                HeightGrdecs = grdecs.ActualHeight;
                HeightTextEditor = grtextedit.ActualHeight;
                MousePosition = Mouse.GetPosition(this);
                Mouse.Capture(separatorGrRight);
            }
        }
        #endregion

        private void separator_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isResizing)
            {
                isResizing = false;
                Mouse.Capture(null);
            }
        }
        private void SaveControlSetting()
        {
            Properties.Settings.Default.controlsize = $"{grleft.Width},{grcard.Height},{grdecs.Height}";
            Properties.Settings.Default.Save();
        }

        #endregion

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl != null)
            {
                foreach (TabItem tabItem in tabControl.Items)
                {
                    if (tabItem.IsSelected)
                    {
                        tabItem.Background = themeColor;
                    }
                    else
                    {
                        tabItem.Background = Brushes.Transparent;
                    }
                }
            }

        }
        private void UpdateTabHeaderLeft(double newWidth)
        {
            // Kiểm tra xem Tab 3 có bị ẩn không
            var tabnew = tablist.Items[3] as TabItem;
            bool isTabNewVisible = tabnew.Visibility != Visibility.Collapsed;
            double headerWidthCard = isTabNewVisible ? (newWidth - 4) / 4 : (newWidth - 3) / 3;
            // Áp dụng bề rộng cho tất cả các TabItem headers
            for (int i = 0; i < tablist.Items.Count; i++)
            {
                var tabItem = tablist.Items[i] as TabItem;
                if (tabItem != null)
                {
                    tabItem.Width = headerWidthCard;
                }
            }
            tablist.InvalidateMeasure();
            tablist.UpdateLayout();

            double headerWidthScrapi = (newWidth - 2) / 2;
            // Áp dụng bề rộng cho tất cả các TabItem headers
            for (int i = 0; i < tablistScrapi.Items.Count; i++)
            {
                var tabItem = tablistScrapi.Items[i] as TabItem;
                if (tabItem != null)
                {
                    tabItem.Width = headerWidthScrapi;
                }
            }
            tablistScrapi.InvalidateMeasure();
            tablistScrapi.UpdateLayout();
        }
        private void UpdateTabHeaderHeights(double newHeight)
        {
            double headerHeight = (newHeight - 2) / 2;
            for (int i = 0; i < tabcresult.Items.Count; i++)
            {
                var tabItem = tabcresult.Items[i] as TabItem;
                if (tabItem != null)
                {
                    tabItem.Height = headerHeight;
                }
            }
        }

        #endregion

        #region RichTextBox

        #region Preview Key Down (Change Suggestion + Command Search Data)
        private async void tbsearchdesc_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (suggestionDescList.Visibility == Visibility.Visible)
            {
                if (suggestionDescList.Items.Count == 0) return;

                if (e.Key == Key.Up)
                {
                    if (suggestionDescList.SelectedIndex > 0)
                    {
                        suggestionDescList.SelectedIndex -= 1;
                        suggestionDescList.ScrollIntoView(suggestionDescList.SelectedItem);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    if (suggestionDescList.SelectedIndex < suggestionDescList.Items.Count - 1)
                    {
                        suggestionDescList.SelectedIndex += 1;
                        suggestionDescList.ScrollIntoView(suggestionDescList.SelectedItem);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    suggestionDescList.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
                else if (e.Key == Key.Tab)
                {
                    e.Handled = true;
                    if (suggestionDescList.SelectedIndex >= 0 && suggestionDescList.SelectedIndex < suggestionDescList.Items.Count)
                    {
                        var textRande = new TextRange(tbsearchdesc.Document.ContentStart, tbsearchdesc.Document.ContentEnd);
                        string original = textRande.Text.Trim();
                        var selectedItem = suggestionDescList.SelectedItem?.ToString();
                        string newtext = ReplaceCurrentText(original, selectedItem);

                        tbsearchdesc.Document.Blocks.Clear();
                        tbsearchdesc.AppendText(newtext);

                        suggestionDescList.Visibility = Visibility.Collapsed;

                        TextPointer caretPosition = tbsearchdesc.Document.ContentEnd;
                        tbsearchdesc.Selection.Select(caretPosition, caretPosition);
                    }
                }

            }

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                tablist.SelectedIndex = 0;

                // Hủy tác vụ trước đó
                _tbsearchdescCancellationTokenSource?.Cancel();
                // Tạo mới CancellationTokenSource cho lần tìm kiếm này
                _tbsearchdescCancellationTokenSource = new CancellationTokenSource();

                string searchText = new TextRange(tbsearchdesc.Document.ContentStart, tbsearchdesc.Document.ContentEnd).Text.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    suggestionDescList.Visibility = Visibility.Collapsed;
                    libnamedesc.SelectedIndex = -1;
                    await SearchCdbFilesAsync(searchText, _tbsearchdescCancellationTokenSource.Token);
                }
            }
        }
        private async void tbsearchlua_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (suggestionScriptList.Visibility == Visibility.Visible)
            {
                if (suggestionScriptList.Items.Count == 0) return;

                if (e.Key == Key.Up)
                {
                    if (suggestionScriptList.SelectedIndex > 0)
                    {
                        suggestionScriptList.SelectedIndex -= 1;
                        suggestionScriptList.ScrollIntoView(suggestionScriptList.SelectedItem);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    if (suggestionScriptList.SelectedIndex < suggestionScriptList.Items.Count - 1)
                    {
                        suggestionScriptList.SelectedIndex += 1;
                        suggestionScriptList.ScrollIntoView(suggestionScriptList.SelectedItem);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    suggestionScriptList.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
                else if (e.Key == Key.Tab)
                {
                    e.Handled = true;
                    if (suggestionScriptList.SelectedIndex >= 0 && suggestionScriptList.SelectedIndex < suggestionScriptList.Items.Count)
                    {
                        var textRange = new TextRange(tbsearchlua.Document.ContentStart, tbsearchlua.Document.ContentEnd);
                        string original = textRange.Text.Trim();
                        var selectedItem = suggestionScriptList.SelectedItem?.ToString();
                        string newtext = ReplaceCurrentText(original, selectedItem);

                        tbsearchlua.Document.Blocks.Clear();
                        tbsearchlua.AppendText(newtext);

                        suggestionScriptList.Visibility = Visibility.Collapsed;
                        TextPointer caretPosition = tbsearchlua.Document.ContentEnd;
                        tbsearchlua.Selection.Select(caretPosition, caretPosition);
                    }
                }

            }

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                tablist.SelectedIndex = 1;
                // Hủy tác vụ trước đó
                _tbsearchluaCancellationTokenSource?.Cancel();
                // Tạo mới CancellationTokenSource cho lần tìm kiếm này
                _tbsearchluaCancellationTokenSource = new CancellationTokenSource();

                string searchString = new TextRange(tbsearchlua.Document.ContentStart, tbsearchlua.Document.ContentEnd).Text.Trim();
                if (!string.IsNullOrEmpty(searchString))
                {
                    suggestionScriptList.Visibility = Visibility.Collapsed;
                    libnamelua.SelectedIndex = -1;
                    await SearchLuaFilesAsync(searchString, _tbsearchluaCancellationTokenSource.Token);
                }
            }
        }
        private async void tbsearch_scrapiyard_name_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (suggestionscrapiList.Visibility == Visibility.Visible)
            {
                if (suggestionscrapiList.Items.Count == 0) return;
                if (e.Key == Key.Up)
                {
                    if (suggestionscrapiList.SelectedIndex > 0)
                    {
                        suggestionscrapiList.SelectedIndex -= 1;
                        suggestionscrapiList.ScrollIntoView(suggestionscrapiList.SelectedItem);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    if (suggestionscrapiList.SelectedIndex < suggestionscrapiList.Items.Count - 1)
                    {
                        suggestionscrapiList.SelectedIndex += 1;
                        suggestionscrapiList.ScrollIntoView(suggestionscrapiList.SelectedItem);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    suggestionscrapiList.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
                else if (e.Key == Key.Tab)
                {
                    e.Handled = true;
                    if (suggestionscrapiList.SelectedIndex >= 0 && suggestionscrapiList.SelectedIndex < suggestionscrapiList.Items.Count)
                    {
                        var textRange = new TextRange(tbsearch_scrapiyard_name.Document.ContentStart, tbsearch_scrapiyard_name.Document.ContentEnd);
                        string original = textRange.Text.Trim();
                        var selecteditem = suggestionscrapiList.SelectedItem?.ToString();
                        string newtext = ReplaceCurrentText(original, selecteditem);

                        tbsearch_scrapiyard_name.Document.Blocks.Clear();
                        tbsearch_scrapiyard_name.AppendText(newtext);

                        suggestionscrapiList.Visibility = Visibility.Collapsed;

                        TextPointer caretPosition = tbsearch_scrapiyard_name.Document.ContentEnd;
                        tbsearch_scrapiyard_name.Selection.Select(caretPosition, caretPosition);
                    }
                }

            }

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                tablistScrapi.SelectedIndex = 0;
                suggestionscrapiList.Visibility = Visibility.Collapsed;
                string searchQuery = new TextRange(tbsearch_scrapiyard_name.Document.ContentStart, tbsearch_scrapiyard_name.Document.ContentEnd).Text.Trim();
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    // Hủy tác vụ trước đó
                    _tbsearch_scrapiyard_nameCancellationTokenSource?.Cancel();
                    // Tạo mới CancellationTokenSource cho lần tìm kiếm này
                    _tbsearch_scrapiyard_nameCancellationTokenSource = new CancellationTokenSource();
                    string scrapiyardPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "scrapiyard");

                    ScrapiNameItem.Clear();
                    // libscrapiyard_name.Items.Clear();

                    try
                    {
                        // Tìm file bất đồng bộ
                        var files = await Task.Run(() => SearchFiles(scrapiyardPath, searchQuery, _tbsearch_scrapiyard_nameCancellationTokenSource.Token));
                        // Cập nhật giao diện sau khi tìm kiếm xong
                        if (files.Any())
                        {
                            foreach (var file in files)
                            {
                                ScrapiNameItem.Add(new ScrapiName { ScrapiFileName = Path.GetFileNameWithoutExtension(file), ScrapiFilePath = file });
                                // libscrapiyard_name.Items.Add(new ListBoxItem{Content = Path.GetFileNameWithoutExtension(file), Tag = file});
                            }
                        }
                        else
                        {
                            ScrapiNameItem.Add(new ScrapiName { ScrapiFileName = "No results found.", ScrapiFilePath = "" });
                            // libscrapiyard_name.Items.Add("No results found.");
                        }
                        libscrapiyard_name.SelectedIndex = -1;
                    }
                    catch (OperationCanceledException)
                    {
                        // Bắt khi tác vụ bị hủy
                        // libscrapiyard_name.Items.Add("Search was canceled.");
                    }
                }
                // SelectionChangedEvent
                if (ScrapiNameItem.Count == 1)
                {
                    ScrapiName firstItem = ScrapiNameItem[0];
                    if (firstItem.ScrapiFileName != "No results found.")
                    {
                        if (libscrapiyard_name.SelectedIndex != 0)
                        {
                            libscrapiyard_name.SelectedIndex = 0;
                        }
                    }
                }
            }
        }
        private async void tbsearch_scrapiyard_desc_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (suggestionscrapiListdesc.Visibility == Visibility.Visible)
            {
                if (suggestionscrapiListdesc.Items.Count == 0) return;

                if (e.Key == Key.Up)
                {
                    if (suggestionscrapiListdesc.SelectedIndex > 0)
                    {
                        suggestionscrapiListdesc.SelectedIndex -= 1;
                        suggestionscrapiListdesc.ScrollIntoView(suggestionscrapiListdesc.SelectedItem);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    if (suggestionscrapiListdesc.SelectedIndex < suggestionscrapiListdesc.Items.Count - 1)
                    {
                        suggestionscrapiListdesc.SelectedIndex += 1;
                        suggestionscrapiListdesc.ScrollIntoView(suggestionscrapiListdesc.SelectedItem);
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    suggestionscrapiListdesc.Visibility = Visibility.Collapsed;
                    e.Handled = true;
                }
                else if (e.Key == Key.Tab)
                {
                    e.Handled = true;
                    if (suggestionscrapiListdesc.SelectedIndex >= 0 && suggestionscrapiListdesc.SelectedIndex < suggestionscrapiListdesc.Items.Count)
                    {
                        var textRange = new TextRange(tbsearch_scrapiyard_desc.Document.ContentStart, tbsearch_scrapiyard_desc.Document.ContentEnd);
                        string original = textRange.Text.Trim();
                        var selectedItem = suggestionscrapiListdesc.SelectedItem?.ToString();
                        string newtext = ReplaceCurrentText(original, selectedItem);

                        tbsearch_scrapiyard_desc.Document.Blocks.Clear();
                        tbsearch_scrapiyard_desc.AppendText(newtext);

                        suggestionscrapiListdesc.Visibility = Visibility.Collapsed;

                        TextPointer caretPosition = tbsearch_scrapiyard_desc.Document.ContentEnd;
                        tbsearch_scrapiyard_desc.Selection.Select(caretPosition, caretPosition);
                    }
                }
            }

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                tablistScrapi.SelectedIndex = 1;
                suggestionscrapiListdesc.Visibility = Visibility.Collapsed;

                // Hủy tác vụ trước đó
                _tbsearch_scrapiyard_descCancellationTokenSource?.Cancel();
                // Tạo mới CancellationTokenSource cho lần tìm kiếm này
                _tbsearch_scrapiyard_descCancellationTokenSource = new CancellationTokenSource();
                string searchQuery = new TextRange(tbsearch_scrapiyard_desc.Document.ContentStart, tbsearch_scrapiyard_desc.Document.ContentEnd).Text.Trim();
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    libscrapiyard_desc.SelectedIndex = -1;
                    await SearchScrapiFilesByContentAsync(searchQuery, _tbsearch_scrapiyard_descCancellationTokenSource.Token);
                }
            }
        }
        #endregion

        #region Search Data

        #region Search Card
        private async Task SearchCdbFilesAsync(string searchText, CancellationToken cancellationToken)
        {
            // Nếu không có file hợp lệ, không cần thực hiện tìm kiếm
            if (!_validCdbFiles.Any() || cancellationToken.IsCancellationRequested) return;

            Dispatcher.Invoke(() => DescItem.Clear()
            // GC.Collect();
            // GC.WaitForPendingFinalizers();
            );
            itemCountDesc = 0;
            _taskCompletionDesc = new TaskCompletionSource<bool>();

            // var semaphore = new SemaphoreSlim(4);
            var tasks = _validCdbFiles.Select(async cdbFile =>
            {
                // await semaphore.WaitAsync();
                // Kiểm tra trạng thái hủy ngay khi bắt đầu mỗi tác vụ
                if (cancellationToken.IsCancellationRequested) return;

                using (var connection = new SQLiteConnection($"Data Source={cdbFile};Version=3;"))
                {
                    try
                    {
                        // Kiểm tra trạng thái hủy trước khi thực hiện truy vấn SQL
                        if (cancellationToken.IsCancellationRequested) return;

                        await connection.OpenAsync();

                        // Thực hiện truy vấn chính
                        string query = @"SELECT id, name FROM texts WHERE id = @idSearch OR name LIKE @nameSearch OR desc LIKE @descSearch";
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            // Thêm các tham số với logic tìm kiếm phù hợp
                            command.Parameters.AddWithValue("@idSearch", searchText);
                            command.Parameters.AddWithValue("@nameSearch", "%" + searchText + "%");
                            command.Parameters.AddWithValue("@descSearch", "%" + searchText + "%");

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    // Kiểm tra xem có yêu cầu hủy không trước khi cập nhật kết quả
                                    if (cancellationToken.IsCancellationRequested) return;


                                    if (cancellationToken.IsCancellationRequested) return;

                                    var id = reader["id"].ToString();
                                    var name = reader["name"].ToString();

                                    Dispatcher.Invoke(() =>
                                    {
                                        // libnamedesc.Items.Add(new ListBoxItem { Content = name, Tag = (id, cdbFile) });
                                        DescItem.Add(new DescInfo { Name = name, id = id, CdbFile = cdbFile });
                                    });

                                    if (Interlocked.Increment(ref itemCountDesc) >= MaxItemsPerBatch)
                                    {
                                        await PauseSearchDescAsync(); // Tạm dừng nếu đạt giới hạn
                                    }
                                }
                            }
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        LogError($"{CMess.errorConDB.ToText()} {ex.Message}");
                    }
                }
            }).ToList(); // Chuyển đổi thành List<Task>
            // Kiểm tra hủy sau khi tất cả các tác vụ bắt đầu
            if (cancellationToken.IsCancellationRequested) return;
            // Chờ tất cả các tác vụ hoàn tất
            await Task.WhenAll(tasks);
        }
        private async Task PauseSearchDescAsync()
        {
            await _taskCompletionDesc.Task;
            _taskCompletionDesc = new TaskCompletionSource<bool>();
        }
        public void ResumeSearchDesc()
        {
            itemCountDesc = 0;
            Thread.Sleep(200);
            _taskCompletionDesc?.TrySetResult(true);
        }
        private void ScrollViewerDesc_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                double currentVerticalOffset = scrollViewer.VerticalOffset;
                double maxVerticalOffset = scrollViewer.ScrollableHeight;
                if (currentVerticalOffset / maxVerticalOffset > 0.8)
                {
                    ResumeSearchDesc();
                }
            }
        }

        #endregion

        #region Search Lua
        private async Task SearchLuaFilesAsync(string searchString, CancellationToken cancellationToken)
        {
            // Xóa danh sách kết quả của lần tìm kiếm trước
            Dispatcher.Invoke(() => LuaItems.Clear());
            itemCountLua = 0;
            _taskCompletionLua = new TaskCompletionSource<bool>();

            await Task.Run(async () =>
            {
                var files = Directory.GetFiles(dataSourceFolder, "*.lua", SearchOption.AllDirectories);
                // Tìm kiếm bất đồng bộ cho từng tệp
                foreach (var file in files)
                {
                    try
                    {
                        using (StreamReader reader = new StreamReader(file))
                        {
                            string line;
                            int lineNumber = 0;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                // Kiểm tra xem có yêu cầu hủy không trước khi cập nhật kết quả
                                if (cancellationToken.IsCancellationRequested) return;

                                lineNumber++;
                                if (line.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    // LuaItems.Add(new LuaInfo { FilePath = file, LineNumber = lineNumber });
                                    Dispatcher.Invoke(() =>
                                    {
                                        LuaItems.Add(new LuaInfo { FileName = Path.GetFileNameWithoutExtension(file), FilePath = Path.GetFullPath(file), LineNumber = lineNumber });
                                    });

                                    if (Interlocked.Increment(ref itemCountLua) > MaxItemsPerBatch)
                                    {
                                        await PauseSearchLuaAsync(); // Tạm dừng nếu đạt giới hạn
                                    }
                                    break;
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"{CMess.errorRead.ToText()} {ex.Message}");
                    }
                }
            }, cancellationToken);
        }
        private async Task PauseSearchLuaAsync()
        {
            await _taskCompletionLua.Task;
            _taskCompletionLua = new TaskCompletionSource<bool>();
        }
        public void ResumeSearchLua()
        {
            itemCountLua = 0;
            Thread.Sleep(200);
            _taskCompletionLua?.TrySetResult(true);
        }
        private void ScrollViewerLua_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                double currentVerticalOffset = scrollViewer.VerticalOffset;
                double maxVerticalOffset = scrollViewer.ScrollableHeight;
                if (currentVerticalOffset / maxVerticalOffset > 0.8)
                {
                    ResumeSearchLua();
                }
            }
        }
        #endregion

        // Search Scrapi Name
        private IEnumerable<string> SearchFiles(string directory, string searchQuery, CancellationToken cancellationToken)
        {
            // Lấy danh sách file *.yml và lọc theo tên
            var files = Directory.GetFiles(directory, "*.yml", SearchOption.AllDirectories)
                .Where(f => Path.GetFileNameWithoutExtension(f)
                .IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0);

            // Kiểm tra token hủy và thoát nếu cần
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            return files;
        }
        // Search Scrapi Desc
        private async Task SearchScrapiFilesByContentAsync(string searchString, CancellationToken cancellationToken)
        {
            Dispatcher.Invoke(() => ScrapiDescItem.Clear());

            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "scrapiyard");
            var files = Directory.GetFiles(directoryPath, "*.yml", SearchOption.AllDirectories);
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            var block = new ActionBlock<string>(async (file) =>
            {
                try
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (cancellationToken.IsCancellationRequested) return;

                            if (line.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                // string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                                // string fullFilePath = Path.GetFullPath(file);

                                Dispatcher.Invoke(() =>
                                {
                                    ScrapiDescItem.Add(new ScrapiDesc { ScrapiDescName = Path.GetFileNameWithoutExtension(file), ScrapiDescPath = Path.GetFullPath(file) });
                                    // libscrapiyard_desc.Items.Add(new ListBoxItem { Content = fileNameWithoutExtension, Tag = fullFilePath });
                                });
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError($"{CMess.errorRead.ToText()} {file} {ex.Message}");
                }
            }, options);

            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested) break;
                await block.SendAsync(file);
            }
            block.Complete();
            await block.Completion;
        }

        #endregion

        #region Suggestion

        #region Text Changed (Search Suggestion)
        private void tbsearchdesc_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange textRande = new TextRange(tbsearchdesc.Document.ContentStart, tbsearchdesc.Document.ContentEnd);
            string text = textRande.Text.Trim();
            string lastWord = GetLastWord(text);

            if (string.IsNullOrEmpty(lastWord))
            {
                suggestionDescList.Visibility = Visibility.Collapsed;
                return;
            }

            var suggestions = keywordsdesc.Where(k => k.StartsWith(lastWord, StringComparison.OrdinalIgnoreCase)).ToList();
            if (suggestions.Count > 0)
            {
                suggestionDescList.ItemsSource = suggestions;
                suggestionDescList.Visibility = Visibility.Visible;
                suggestionDescList.SelectedIndex = 0;
            }
            else
            {
                suggestionDescList.Visibility = Visibility.Collapsed;
            }
        }
        private void tbsearchlua_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange textRange = new TextRange(tbsearchlua.Document.ContentStart, tbsearchlua.Document.ContentEnd);
            string text = textRange.Text.Trim();
            string lastWord = GetLastWord(text);
            if (string.IsNullOrEmpty(lastWord))
            {
                suggestionScriptList.Visibility = Visibility.Collapsed;
                return;
            }

            var suggestions = keywordsscript.Where(k => k.StartsWith(lastWord, StringComparison.OrdinalIgnoreCase)).ToList();
            if (suggestions.Count > 0)
            {
                suggestionScriptList.ItemsSource = suggestions;
                suggestionScriptList.Visibility = Visibility.Visible;
                suggestionScriptList.SelectedIndex = 0;
            }
            else
            {
                suggestionScriptList.Visibility = Visibility.Collapsed;
            }
        }
        private void tbsearch_scrapiyard_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange textRange = new TextRange(tbsearch_scrapiyard_name.Document.ContentStart, tbsearch_scrapiyard_name.Document.ContentEnd);
            string text = textRange.Text.Trim();
            string lastWord = GetLastWord(text);
            if (string.IsNullOrEmpty(lastWord))
            {
                suggestionscrapiList.Visibility = Visibility.Collapsed;
                return;
            }

            var suggestions = keywordsscript.Where(k => k.StartsWith(lastWord, StringComparison.OrdinalIgnoreCase)).ToList();
            if (suggestions.Count > 0)
            {
                suggestionscrapiList.ItemsSource = suggestions;
                suggestionscrapiList.Visibility = Visibility.Visible;
                suggestionscrapiList.SelectedIndex = 0;
            }
            else
            {
                suggestionscrapiList.Visibility = Visibility.Collapsed;
            }
        }
        private void tbsearch_scrapiyard_desc_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange textRange = new TextRange(tbsearch_scrapiyard_desc.Document.ContentStart, tbsearch_scrapiyard_desc.Document.ContentEnd);
            string text = textRange.Text.Trim();
            //string lastword = ReplaceCurrentText(text, "").Item2;
            string lastword = GetLastWord(text);

            if (string.IsNullOrEmpty(lastword))
            {
                suggestionscrapiListdesc.Visibility = Visibility.Collapsed;
                return;
            }

            var suggestions = keywordsdesc.Where(k => k.StartsWith(lastword, StringComparison.OrdinalIgnoreCase)).ToList();
            if (suggestions.Count > 0)
            {
                suggestionscrapiListdesc.ItemsSource = suggestions;
                suggestionscrapiListdesc.Visibility = Visibility.Visible;
                suggestionscrapiListdesc.SelectedIndex = 0;
            }
            else
            {
                suggestionscrapiListdesc.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Lost Focus (Suggestion Visibility)
        private void tbsearchdesc_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!suggestionDescList.IsMouseOver)
            {
                suggestionDescList.Visibility = Visibility.Collapsed;
            }
        }
        private void tbsearchlua_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!suggestionScriptList.IsMouseOver)
            {
                suggestionScriptList.Visibility = Visibility.Collapsed;
            }
        }
        private void tbsearch_scrapiyard_name_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!suggestionscrapiList.IsMouseOver)
            {
                suggestionscrapiList.Visibility = Visibility.Collapsed;
            }
        }
        private void tbsearch_scrapiyard_desc_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!suggestionscrapiListdesc.IsMouseOver)
            {
                suggestionscrapiListdesc.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Mouse Left Button Up (Choose Suggestion)
        private void suggestionDescList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = suggestionDescList.SelectedItem?.ToString();
            if (listBoxItem == null)
            {
                suggestionDescList.Visibility = Visibility.Collapsed;
                return;
            }

            var textRange = new TextRange(tbsearchdesc.Document.ContentStart, tbsearchdesc.Document.ContentEnd);
            string original = textRange.Text.Trim();
            string newtext = ReplaceCurrentText(original, listBoxItem);

            tbsearchdesc.Document.Blocks.Clear();
            tbsearchdesc.AppendText(newtext);

            suggestionDescList.Visibility = Visibility.Collapsed;

            tbsearchdesc.Focus();
            TextPointer caretPosition = tbsearchdesc.Document.ContentEnd;
            tbsearchdesc.Selection.Select(caretPosition, caretPosition);
        }
        private void suggestionScriptList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = suggestionScriptList.SelectedItem?.ToString();
            if (listBoxItem == null)
            {
                suggestionScriptList.Visibility = Visibility.Collapsed;
                return;
            }

            var textRange = new TextRange(tbsearchlua.Document.ContentStart, tbsearchlua.Document.ContentEnd);
            string original = textRange.Text.Trim();
            string newtext = ReplaceCurrentText(original, listBoxItem);

            tbsearchlua.Document.Blocks.Clear();
            tbsearchlua.AppendText(newtext);

            suggestionScriptList.Visibility = Visibility.Collapsed;

            tbsearchlua.Focus();
            TextPointer caretPosition = tbsearchlua.Document.ContentEnd;
            tbsearchlua.Selection.Select(caretPosition, caretPosition);
        }
        private void suggestionscrapiList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = suggestionscrapiList.SelectedItem?.ToString();
            if (listBoxItem == null)
            {
                suggestionscrapiList.Visibility = Visibility.Collapsed;
                return;
            }

            var textRange = new TextRange(tbsearch_scrapiyard_name.Document.ContentStart, tbsearch_scrapiyard_name.Document.ContentEnd);
            string original = textRange.Text.Trim();
            string newtext = ReplaceCurrentText(original, listBoxItem);

            tbsearch_scrapiyard_name.Document.Blocks.Clear();
            tbsearch_scrapiyard_name.AppendText(newtext);

            suggestionscrapiList.Visibility = Visibility.Collapsed;

            tbsearch_scrapiyard_name.Focus();
            TextPointer caretPosition = tbsearch_scrapiyard_name.Document.ContentEnd;
            tbsearch_scrapiyard_name.Selection.Select(caretPosition, caretPosition);
        }
        private void suggestionscrapiListdesc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = suggestionscrapiListdesc.SelectedItem?.ToString();
            if (listBoxItem == null)
            {
                suggestionscrapiListdesc.Visibility = Visibility.Collapsed;
                return;
            }

            var textRange = new TextRange(tbsearch_scrapiyard_desc.Document.ContentStart, tbsearch_scrapiyard_desc.Document.ContentEnd);
            string original = textRange.Text.Trim();
            string newtext = ReplaceCurrentText(original, listBoxItem);

            tbsearch_scrapiyard_desc.Document.Blocks.Clear();
            tbsearch_scrapiyard_desc.AppendText(newtext);

            suggestionscrapiListdesc.Visibility = Visibility.Collapsed;

            tbsearch_scrapiyard_desc.Focus();
            TextPointer caretPosition = tbsearch_scrapiyard_desc.Document.ContentEnd;
            tbsearch_scrapiyard_desc.Selection.Select(caretPosition, caretPosition);
        }
        #endregion

        #region Replace
        private string GetLastWord(string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                return string.Empty;
            }
            MatchCollection matches = regex.Matches(original);
            int lastSpecialCharIndex;
            if (matches.Count <= 0)
            {
                return original;
            }
            else
            {
                lastSpecialCharIndex = matches[matches.Count - 1].Index;
            }
            return original.Substring(lastSpecialCharIndex + 1);
        }
        private string ReplaceCurrentText(string original, string suggestion)
        {
            if (string.IsNullOrEmpty(original))
            {
                if (string.IsNullOrEmpty(suggestion))
                {
                    return string.Empty;
                }
                else
                {
                    return suggestion;
                }
            }
            else
            {
                MatchCollection matches = regex.Matches(original);
                int lastSpecialCharIndex;
                if (matches.Count <= 0)
                {
                    return suggestion;
                }
                else
                {
                    lastSpecialCharIndex = matches[matches.Count - 1].Index;
                }
                string firstPart = original.Substring(0, lastSpecialCharIndex + 1);
                return firstPart + suggestion;
            }
        }

        // return item1: chuỗi replace, item2: chuỗi lastword
        // Cái này lag quá nên bỏ
        private Tuple<string, string> ReplaceAndGetLastWord(string original, string suggestion)
        {
            if (string.IsNullOrEmpty(original))
            {
                if (string.IsNullOrEmpty(suggestion))
                {
                    return Tuple.Create("", "");
                }
                else
                {
                    return Tuple.Create(suggestion, "");
                }
            }
            else
            {
                MatchCollection matches = regex.Matches(original);
                int lastSpecialCharIndex;
                if (matches.Count <= 0)
                {
                    return Tuple.Create(suggestion, original);
                }
                else
                {
                    lastSpecialCharIndex = matches[matches.Count - 1].Index;
                }
                string firstPart = original.Substring(0, lastSpecialCharIndex + 1);
                string secondPart = original.Substring(lastSpecialCharIndex + 1);

                return Tuple.Create(firstPart + suggestion, secondPart);
            }
        }

        #endregion

        #endregion

        #endregion

        #region List Box

        #region Selection Changed
        private async void libnamedesc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (libnamedesc.SelectedItem is DescInfo selectedDescInfo)
            {
                tabcresult.SelectedIndex = 0;
                currentFilePath = null;
                currentPassword = null;
                currentName = null;
                _lineHighlighter.ClearHighlight();
                string name = selectedDescInfo.Name;
                string selectedid = selectedDescInfo.id;
                string cdbfile = selectedDescInfo.CdbFile;
                await Task.WhenAll(LoadLuaFileAsyncfromdesc(selectedid), LoadDescIdAsyncfromdesc(selectedid, cdbfile), SetImageSource());
            }
        }
        private async void libnamelua_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (libnamelua.SelectedItem is LuaInfo selectedLuaInfo)
            {
                tabcresult.SelectedIndex = 0;
                currentFilePath = null;
                currentPassword = null;
                currentName = null;
                string filename = selectedLuaInfo.FileName;
                string filePath = selectedLuaInfo.FilePath;
                int lineNumber = selectedLuaInfo.LineNumber;
                await Task.WhenAll(LoadLuaFileAsyncfromlua(filePath, lineNumber), LoadDescIdAsyncfromlua(filename), SetImageSource());
            }
        }
        private async void libnametext_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentFilePath = null;
            currentPassword = null;
            currentName = null;
            if (libnametext.SelectedItem is ListBoxItem selectedItem)
            {
                tabcresult.SelectedIndex = 0;
                _lineHighlighter.ClearHighlight();
                // Lấy đường dẫn của file từ Tag của ListBoxItem
                string selectedFilePath = selectedItem.Tag as string;
                string fileContent;

                if (!string.IsNullOrEmpty(selectedFilePath) && File.Exists(selectedFilePath))
                {
                    currentFilePath = selectedFilePath;
                    // Đọc nội dung file và hiển thị vào TextEditor
                    fileContent = File.ReadAllText(currentFilePath);
                    await Task.WhenAll(LoadDescIdAsyncfromlua(Path.GetFileNameWithoutExtension(selectedFilePath)), SetImageSource());
                }
                else
                {
                    fileContent = $"File not found {selectedItem.Content}";
                }
                textEditor.Text = fileContent;
            }
        }
        private async void libnamenew_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentFilePath = null;
            currentPassword = null;
            currentName = null;
            if (libnamenew.SelectedItem is ListBoxItem selectedItem)
            {
                // Lấy đường dẫn tuyệt đối từ Tag của ListBoxItem
                string selectedFilePath = selectedItem.Tag as string;
                _lineHighlighter.ClearHighlight();
                if (!string.IsNullOrEmpty(selectedFilePath) && File.Exists(selectedFilePath))
                {
                    try
                    {
                        // Gán đường dẫn vào currentFilePath
                        currentFilePath = selectedFilePath;
                        // Đọc nội dung file và hiển thị vào AvalonEdit (textEditor)
                        string fileContent = File.ReadAllText(currentFilePath);
                        await Task.WhenAll(LoadDescIdAsyncfromlua(Path.GetFileNameWithoutExtension(selectedFilePath)), SetImageSource());
                        textEditor.Text = fileContent; // Hiển thị nội dung vào textEditor
                    }
                    catch (Exception ex)
                    {
                        LogError($"{CMess.errorRead.ToText()} {ex.Message}");
                    }
                }
                else
                {
                    LogError($"{CMess.invaFilePath.ToText()}");
                }
            }
        }

        private async void libscrapiyard_name_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // if (libscrapiyard_name.SelectedItem is ListBoxItem selectedItem && selectedItem.Tag is string filePath && File.Exists(filePath))
            if (libscrapiyard_name.SelectedItem is ScrapiName selectedScrapiName)
            {
                tabcresult.SelectedIndex = 1;

                string fullFilePath = selectedScrapiName.ScrapiFilePath;
                // tbscrapiyard.Text = File.ReadAllText(fullFilePath);
                tbscrapiyard.Text = ScrapiyardData.GetScrapiyardText(fullFilePath);

                if (isAutoSearch)
                {
                    // Hủy tác vụ trước đó
                    _tbsearchluaCancellationTokenSource?.Cancel();
                    // Tạo mới CancellationTokenSource cho lần tìm kiếm này
                    _tbsearchluaCancellationTokenSource = new CancellationTokenSource();

                    tablist.SelectedIndex = 1;
                    string selectedfilename = selectedScrapiName.ScrapiFileName;
                    await Task.Run(() => SearchLuaFilesAsync(selectedfilename, _tbsearchluaCancellationTokenSource.Token));
                }
            }
        }
        private async void libscrapiyard_desc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (libscrapiyard_desc.SelectedItem is ScrapiDesc selectedScrapiDesc)
            // if (libscrapiyard_desc.SelectedItem is ListBoxItem selectedItem && selectedItem.Tag is string fullFilePath && File.Exists(fullFilePath))
            {
                tabcresult.SelectedIndex = 1;
                string fullFilePath = selectedScrapiDesc.ScrapiDescPath;
                // tbscrapiyard.Text = File.ReadAllText(fullFilePath);
                tbscrapiyard.Text = ScrapiyardData.GetScrapiyardText(fullFilePath);

                if (isAutoSearch)
                {
                    // Hủy tác vụ trước đó
                    _tbsearchluaCancellationTokenSource?.Cancel();
                    // Tạo mới CancellationTokenSource cho lần tìm kiếm này
                    _tbsearchluaCancellationTokenSource = new CancellationTokenSource();

                    tablist.SelectedIndex = 1;
                    // string selectedfilename = selectedItem.Content.ToString();
                    string selectedfilename = selectedScrapiDesc.ScrapiDescName;
                    await Task.Run(() => SearchLuaFilesAsync(selectedfilename, _tbsearchluaCancellationTokenSource.Token));
                }
            }
        }
        #endregion

        #region Load Data
        private async Task LoadLuaFileAsyncfromdesc(string id)
        {
            try
            {
                string projectFolder = dataSourceFolder;
                string luaFilePath = await Task.Run(() => Directory.GetFiles(projectFolder, $"c{id}.lua", SearchOption.AllDirectories).FirstOrDefault());
                string luaContent = null;

                if (string.IsNullOrEmpty(luaFilePath))
                {
                    luaContent = string.Format(CMess.luaFileNotFou.ToText(), id.ToString());
                }
                else if (File.Exists(luaFilePath))
                {
                    currentFilePath = luaFilePath; // Lưu đường dẫn tuyệt đối của tệp

                    using (var reader = new StreamReader(currentFilePath))
                    {
                        luaContent = await reader.ReadToEndAsync();
                    }
                    if (string.IsNullOrWhiteSpace(luaContent))
                    {
                        luaContent = string.Format(CMess.luaFileEmp.ToText(), id.ToString());
                    }
                    //luaContent = await Task.Run(() => File.ReadAllText(currentFilePath));
                    //if (string.IsNullOrWhiteSpace(luaContent))
                    //{
                    //    luaContent = $"Lua file for ID {id} is empty.";
                    //}
                }
                Dispatcher.Invoke(() => textEditor.Text = luaContent);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    LogError($"{CMess.errorOcc.ToText()} {ex.Message}");
                });
            }
        }
        private async Task LoadDescIdAsyncfromdesc(string id, string cdbFilePath)
        {
            // Trước khi bắt đầu tìm kiếm
            Dispatcher.Invoke(() =>
            {
                tbdecs.Document.Blocks.Clear();
                tbdecs.Document.Blocks.Add(new Paragraph(new Run("Loading...")));
            });
            bool found = false; //Cờ kết quả
            string connectionString = $"Data Source={cdbFilePath};Version=3;Pooling=True;Max Pool Size=100;";
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT name, desc FROM texts WHERE id = @id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string nameValue = reader["name"]?.ToString();
                                string descValue = reader["desc"]?.ToString();

                                Paragraph para1 = new Paragraph();
                                Run runId = new Run(id);
                                runId.Foreground = themeColor;
                                para1.Inlines.Add(runId);
                                para1.Inlines.Add(new Run(" | " + nameValue));
                                Paragraph para2 = new Paragraph(new Run(descValue));

                                Dispatcher.Invoke(() =>
                                {
                                    tbdecs.Document.Blocks.Clear();
                                    tbdecs.Document.Blocks.Add(para1);
                                    tbdecs.Document.Blocks.Add(para2);
                                });
                                currentPassword = id;
                                currentName = nameValue;
                                found = true;
                            }
                        }
                    }

                    string querydata = "SELECT ot, alias, setcode, type, atk, def, level, race, attribute FROM datas WHERE id = @id";
                    using (var command = new SQLiteCommand(querydata, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                ulong ot = reader["ot"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["ot"])) : 0UL;
                                ulong alias = reader["alias"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["alias"])) : 0UL;
                                ulong setcode = reader["setcode"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["setcode"])) : 0UL;
                                ulong type = reader["type"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["type"])) : 0UL;
                                long atk = reader["atk"] != DBNull.Value ? Convert.ToInt64(reader["atk"]) : 0L;
                                long def = reader["def"] != DBNull.Value ? Convert.ToInt64(reader["def"]) : 0L;
                                ulong level = reader["level"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["level"])) : 0UL;
                                ulong race = reader["race"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["race"])) : 0UL;
                                ulong attribute = reader["attribute"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["attribute"])) : 0UL;
                                await GetCardInfo(id, ot, alias, setcode, type, atk, def, level, race, attribute);
                            }
                        }
                    }
                }
                // Nếu không tìm thấy kết quả nào, hiển thị thông báo
                if (!found)
                {
                    Dispatcher.Invoke(() =>
                    {
                        tbdecs.Document.Blocks.Clear();
                        tbdecs.Document.Blocks.Add(new Paragraph(new Run(string.Format(CMess.descNotFou.ToText(), id.ToString()))));
                    });
                }
            }
            catch (SQLiteException sqlEx)
            {
                LogError($"SQL {CMess.error.ToText()}: {sqlEx.Message}\n{sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                LogError($"{CMess.errorOcc.ToText()} {ex.Message}");
            }
        }

        private async Task LoadLuaFileAsyncfromlua(string filePath, int linenumber)
        {
            try
            {
                string luaContent = null;
                if (string.IsNullOrEmpty(filePath))
                {
                    luaContent = string.Format(CMess.luaFileNotFou.ToText(), Path.GetFileNameWithoutExtension(filePath));
                }
                else if (File.Exists(filePath))
                {
                    currentFilePath = filePath; // Lưu đường dẫn tuyệt đối của tệp
                    using (var reader = new StreamReader(currentFilePath))
                    {
                        luaContent = await reader.ReadToEndAsync();
                    }
                    // luaContent = await Task.Run(() => File.ReadAllText(currentFilePath));
                    if (string.IsNullOrWhiteSpace(luaContent))
                    {
                        luaContent = string.Format(CMess.luaFileEmp.ToText(), Path.GetFileNameWithoutExtension(filePath));
                    }
                }
                else
                {
                    luaContent = string.Format(CMess.luaFileNotFou.ToText(), Path.GetFileNameWithoutExtension(filePath));
                }
                Dispatcher.Invoke(() => textEditor.Text = luaContent);

                // Highlight the specified line
                HighlightLine(linenumber);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    LogError($"{CMess.errorOcc.ToText()} {ex.Message}");
                });
            }
        }
        private void HighlightLine(int linenumber)
        {
            _lineHighlighter.HighlightLine(linenumber);
            var line = textEditor.Document.GetLineByNumber(linenumber);
            textEditor.ScrollTo(linenumber, 0);
            textEditor.Select(line.Offset, 0);
        }
        private async Task LoadDescIdAsyncfromlua(string fileName)
        {
            // Kiểm tra định dạng của filename
            if (!fileName.StartsWith("c") || !long.TryParse(fileName.Substring(1), out _))
            {
                Dispatcher.Invoke(() =>
                {
                    tbdecs.Document.Blocks.Clear();
                    tbdecs.Document.Blocks.Add(new Paragraph(new Run(string.Format(CMess.descNotFou.ToText(), fileName))));
                });
                return;
            }
            // Trước khi bắt đầu tìm kiếm
            Dispatcher.Invoke(() =>
            {
                tbdecs.Document.Blocks.Clear();
                tbdecs.Document.Blocks.Add(new Paragraph(new Run("Loading...")));
            });
            // Tách id từ filename
            string id = fileName.Substring(1); // Lấy phần id (bỏ "c")
            bool found = false; //Cờ kết quả
            try
            {
                // Lặp qua từng file trong danh sách _validCdbFiles
                foreach (var cdbFilePath in _validCdbFiles)
                {
                    string connectionString = $"Data Source={cdbFilePath};Version=3;Pooling=True;Max Pool Size=100;";
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        string query = "SELECT name, desc FROM texts WHERE id = @id";
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    string nameValue = reader["name"]?.ToString();
                                    string descValue = reader["desc"]?.ToString();

                                    Paragraph para1 = new Paragraph();
                                    Run runId = new Run(id);
                                    runId.Foreground = themeColor;
                                    para1.Inlines.Add(runId);
                                    para1.Inlines.Add(new Run(" | " + nameValue));
                                    Paragraph para2 = new Paragraph(new Run(descValue));

                                    Dispatcher.Invoke(() =>
                                    {
                                        tbdecs.Document.Blocks.Clear();
                                        tbdecs.Document.Blocks.Add(para1);
                                        tbdecs.Document.Blocks.Add(para2);
                                    });
                                    currentPassword = id;
                                    currentName = nameValue;
                                    found = true;
                                }
                            }
                        }
                        if (found)
                        {
                            string querydata = "SELECT ot, alias, setcode, type, atk, def, level, race, attribute FROM datas WHERE id = @id";
                            using (var command = new SQLiteCommand(querydata, connection))
                            {
                                command.Parameters.AddWithValue("@id", id);
                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    if (await reader.ReadAsync())
                                    {
                                        ulong ot = reader["ot"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["ot"])) : 0UL;
                                        ulong alias = reader["alias"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["alias"])) : 0UL;
                                        ulong setcode = reader["setcode"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["setcode"])) : 0UL;
                                        ulong type = reader["type"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["type"])) : 0UL;
                                        long atk = reader["atk"] != DBNull.Value ? Convert.ToInt64(reader["atk"]) : 0L;
                                        long def = reader["def"] != DBNull.Value ? Convert.ToInt64(reader["def"]) : 0L;
                                        ulong level = reader["level"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["level"])) : 0UL;
                                        ulong race = reader["race"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["race"])) : 0UL;
                                        ulong attribute = reader["attribute"] != DBNull.Value ? unchecked((ulong)Convert.ToUInt64(reader["attribute"])) : 0UL;
                                        await GetCardInfo(id, ot, alias, setcode, type, atk, def, level, race, attribute);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                // Nếu không tìm thấy kết quả nào, hiển thị thông báo
                if (!found)
                {
                    Dispatcher.Invoke(() =>
                    {
                        tbdecs.Document.Blocks.Clear();
                        tbdecs.Document.Blocks.Add(new Paragraph(new Run(string.Format(CMess.descNotFou.ToText(), fileName))));
                    });
                }
            }
            catch (SQLiteException sqlEx)
            {
                LogError($"SQL {CMess.error.ToText()}: {sqlEx.Message}\n{sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                LogError($"{CMess.errorOcc.ToText()} {ex.Message}");
            }
        }

        #endregion

        #region Scroll
        private void libname_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            int numberOfItemsToMove = 1;
            var listBox = (ListBox)sender;

            // Di chuyển lên hoặc xuống dựa vào Delta
            if (e.Delta > 0)
            {
                for (int i = 0; i < numberOfItemsToMove; i++)
                {
                    if (listBox.SelectedIndex > 0)
                        listBox.SelectedIndex--;
                }
            }
            else
            {
                for (int i = 0; i < numberOfItemsToMove; i++)
                {
                    if (listBox.SelectedIndex < listBox.Items.Count - 1)
                        listBox.SelectedIndex++;
                }
            }
            listBox.ScrollIntoView(listBox.SelectedItem);
            e.Handled = true;
        }
        private void libname_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Ép kiểu sender thành ListBox
            var listBox = sender as ListBox;
            if (listBox == null)
            {
                return;
            }
            // Tắt chế độ autoscroll nếu đang hoạt động
            if (_isScrolling)
            {
                _isScrolling = false;
                Mouse.Capture(null);
                Mouse.OverrideCursor = null; // Khôi phục con trỏ
                _autoscrollTimer.Stop();
                _basePosition = new Point(0, 0); // Xóa vị trí gốc
                e.Handled = true; // Ngăn không cho sự kiện đi tiếp
                return; // Thoát khỏi phương thức
            }
            // Nếu không đang cuộn và nhấn chuột giữa
            if (e.ChangedButton == MouseButton.Middle)
            {
                // Bật chế độ autoscroll
                _isScrolling = true;
                Mouse.Capture(listBox); // Dùng listBox thay vì libnamedesc
                Mouse.OverrideCursor = Cursors.ScrollNS; // Thay đổi con trỏ
                _basePosition = e.GetPosition(listBox); // Lưu vị trí gốc
                _autoscrollTimer.Start();
                e.Handled = true; // Ngăn không cho sự kiện đi tiếp
            }
        }
        private void libname_MouseMove(object sender, MouseEventArgs e)
        {
            var listBox = sender as ListBox; // Lấy ListBox từ sender
            if (listBox == null || !_isScrolling)
            {
                return; // Nếu không phải ListBox hoặc không đang cuộn, không làm gì cả
            }
            var mousePos = e.GetPosition(listBox); // Lấy vị trí chuột trong ListBox
            double deltaY = mousePos.Y - _basePosition.Y; // Tính sự thay đổi vị trí Y

            if (deltaY < -10) // Nếu con trỏ di chuyển lên trên
            {
                // Cuộn lên
                AutoScrollList(listBox, -2); // Cuộn lên với tốc độ
            }
            else if (deltaY > 10) // Nếu con trỏ di chuyển xuống dưới
            {
                // Cuộn xuống
                AutoScrollList(listBox, 2); // Cuộn xuống với tốc độ
            }
        }
        private void AutoScrollList(ListBox listBox, double offset)
        {
            // Lấy ScrollViewer từ ListBox
            var scrollViewer = GetDescendantByType<ListBox, ScrollViewer>(listBox);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset);
            }
        }
        private T GetDescendantByType<TParent, T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is T)
                    {
                        return (T)child;
                    }
                    var childOfChild = GetDescendantByType<TParent, T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
        #endregion

        #endregion

        #region Card Information
        private void tbtncardinfo_Checked(object sender, RoutedEventArgs e)
        {
            drhcardinfo.IsLeftDrawerOpen = true;
        }
        private void tbtncardinfo_Unchecked(object sender, RoutedEventArgs e)
        {
            drhcardinfo.IsLeftDrawerOpen = false;
        }

        private async Task SetImageSource()
        {
            await Task.Run(() =>
            {
                string imageSource = FindIInfoService.FindImagePath(dataSourceFolder, currentPassword);

                if (!string.IsNullOrWhiteSpace(imageSource))
                {
                    Dispatcher.Invoke(() =>
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(imageSource, UriKind.RelativeOrAbsolute);
                        bitmapImage.DecodePixelWidth = 500;
                        bitmapImage.EndInit();
                        imagecard.Source = bitmapImage;
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        imagecard.Source = new BitmapImage(new Uri(nullImagePath, UriKind.Absolute));
                    });
                }
            });

        }
        private async Task GetCardInfo(string id, ulong ot, ulong alias, ulong setcode, ulong type, long atk, long def, ulong level, ulong race, ulong attribute)
        {
            
        }
        #endregion

        #region Web
        private void imgpopup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            konamipopup.IsOpen = true;
        }

        private void linkkonamidb_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? konamiID;
                string URL = string.Empty;
                if (int.TryParse(currentPassword, out int password))
                {
                    if (password < 1000 || password >= 600000000)
                    {
                        throw new Exception($"{CMess.cardIDNotExist.ToText()}");
                    }
                    if (password < 100000000)
                    {
                        konamiID = GetIDService.GetKonamiOfficialID(currentPassword);
                        URL = $"https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&request_locale=ja&cid={konamiID}";
                    }
                    else if (password >= 160000000 && password < 300000000)
                    {
                        konamiID = GetIDService.GetKonamiRushID(currentPassword);
                        URL = $"https://www.db.yugioh-card.com/rushdb/card_search.action?ope=2&request_locale=ja&cid={konamiID}";
                    }
                    else
                    {
                        throw new Exception($"{CMess.konamiIDnotFou.ToText()}");
                    }
                    GetBrowserPath.NavigateBrowser(URL);
                }
                else
                {
                    throw new Exception($"{CMess.invaCardID.ToText()}");
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                // MessageBox.Show($"{ex.Message}");
            }
            e.Handled = true;
        }
        private void linkyugipedia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(currentPassword, out int password))
                {
                    if (password >= 600000000)
                    {
                        throw new Exception($"{CMess.cardIDNotExist.ToText()}");
                    }
                    if (string.IsNullOrWhiteSpace(currentName))
                    {
                        throw new Exception($"{CMess.yugiPedianotFou.ToText()}");
                    }
                    string URL = $"https://yugipedia.com/wiki/{NameReplace.ProcessName(currentName, password)}";
                    GetBrowserPath.NavigateBrowser(URL);
                }
                else
                {
                    throw new Exception($"{CMess.invaCardID.ToText()}");
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                // MessageBox.Show($"{ex.Message}.");
            }
            e.Handled = true;
        }
        private void linkYGOResources_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? konamiID;
                if (int.TryParse(currentPassword, out int password))
                {
                    if (password < 1000 || password >= 600000000)
                    {
                        throw new Exception($"{CMess.cardIDNotExist.ToText()}");
                    }
                    if (password < 100000000)
                    {
                        konamiID = GetIDService.GetKonamiOfficialID(currentPassword);
                    }
                    else if (password >= 160000000 && password < 300000000)
                    {
                        konamiID = GetIDService.GetKonamiRushID(currentPassword);
                    }
                    else
                    {
                        throw new Exception($"{CMess.konamiIDnotFou.ToText()}");
                    }

                    string URL = $"https://db.ygoresources.com/card#{konamiID}";
                    GetBrowserPath.NavigateBrowser(URL);
                }
                else
                {
                    throw new Exception($"{CMess.konamiIDnotFou.ToText()}");
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                // MessageBox.Show($"{ex.Message}");
            }
            e.Handled = true;
        }
        #endregion

        #region Error
        private void LogError(string message)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        #endregion

    }
}
