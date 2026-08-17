using System;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.ComponentModel;
using System.Collections.Generic;
using CardEditor.Models.MasterDuel;
using CardEditor.Helpers;
using CardEditor.Manager;
using CardEditor.Commands;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Interfaces;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using CardAppContext = CardEditor.Models.AppContext;


namespace CardEditor.Views
{
    /// <summary>
    /// Interaction logic for DEVWindow.xaml
    /// </summary>
    public partial class DEVWindow : Window, INotifyPropertyChanged
    {
        private readonly IMasterDuelAPIInterface _masterDuelApi;
        private readonly IFileInterface _fileService;

        #region Propertys

        #region Archetype
        private string _edoProArchetypeSetcodeFilePath = string.Empty;
        public string EDooProArchetypeSetcodeFilePath
        {
            get => _edoProArchetypeSetcodeFilePath;
            set
            {
                if (_edoProArchetypeSetcodeFilePath != value)
                {
                    _edoProArchetypeSetcodeFilePath = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _mdproStringConfigFilePath = string.Empty;
        public string MDproStringConfigFilePath
        {
            get => _mdproStringConfigFilePath;
            set
            {
                if (_mdproStringConfigFilePath != value)
                {
                    _mdproStringConfigFilePath = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Scrapiyard
        private string _scrapiyardFolderPath = string.Empty;
        public string ScrapiyardFolderPath
        {
            get => _scrapiyardFolderPath;
            set
            {
                if (_scrapiyardFolderPath != value)
                {
                    _scrapiyardFolderPath = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Yaml Yugi
        private string _officialYamlYugiFolderPath = string.Empty;
        public string OfficialYamlYugiFolderPath
        {
            get => _officialYamlYugiFolderPath;
            set
            {
                if (_officialYamlYugiFolderPath != value)
                {
                    _officialYamlYugiFolderPath = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _rushYamlYugiFolderPath = string.Empty;
        public string RushYamlYugiFolderPath
        {
            get => _rushYamlYugiFolderPath;
            set
            {
                if (_rushYamlYugiFolderPath != value)
                {
                    _rushYamlYugiFolderPath = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Genesys
        private string _genesysFilepath = string.Empty;
        public string GenesysFilepath
        {
            get => _genesysFilepath;
            set
            {
                if (_genesysFilepath != value)
                {
                    _genesysFilepath = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Terminilogy
        private string _terminologyFolderPath = string.Empty;
        public string TerminologyFolderPath
        {
            get => _terminologyFolderPath;
            set
            {
                if (_terminologyFolderPath != value)
                {
                    _terminologyFolderPath = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _specialCharFolderPath = string.Empty;
        public string SpecialCharFolderPath
        {
            get => _specialCharFolderPath;
            set
            {
                if (_specialCharFolderPath != value)
                {
                    _specialCharFolderPath = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region MD API
        private string _masterDuelAPIURL = string.Empty;
        public string MasterDuelAPIURL
        {
            get => _masterDuelAPIURL;
            set
            {
                if (_masterDuelAPIURL != value)
                {
                    _masterDuelAPIURL = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _jsonMDAPIFilePath = string.Empty;
        public string JsonMDAPIFilePath
        {
            get => _jsonMDAPIFilePath;
            set
            {
                if (_jsonMDAPIFilePath != value)
                {
                    _jsonMDAPIFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _statusText = string.Empty;
        public string StatusText
        {
            get => _statusText;
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }
        private int? _currentPage;
        public int? CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                }
            }
        }
        private int? _totalCardsLoaded;
        public int? TotalCardsLoaded
        {
            get => _totalCardsLoaded;
            set
            {
                if (_totalCardsLoaded != value)
                {
                    _totalCardsLoaded = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }
        public BulkObservableCollection<CardMasterDuel> CardsMasterDuel { get; } = new();
        #endregion

        #endregion

        #region Commands

        #region Archetype
        public RelayCommand BrowseEDOProArchetypeFilePathCommand { get; set; }
        public RelayCommand BrowseMDProStringConfigFilePathCommand { get; set; }
        public RelayCommand ExportArchetypeEDOProCommand { get; set; }
        public RelayCommand ExportArchetypeMDProCommand { get; set; }
        #endregion

        #region Scrapiyard
        public RelayCommand CheckUpdateScrapiyardCommand { get; set; }
        public RelayCommand BrowseScrapiyardFolderPathCommand { get; set; }
        public RelayCommand ExportScrapiyardConstantCommand { get; set; }
        public RelayCommand ExportScrapiyardEnumCommand { get; set; }
        public RelayCommand ExportScrapiyardFunctionCommand { get; set; }
        public RelayCommand ExportScrapiyardNameSpaceCommand { get; set; }
        public RelayCommand ExportScrapiyardTagCommand { get; set; }
        public RelayCommand ExportScrapiyardTypeCommand { get; set; }
        #endregion

        #region Yaml Yugi
        public RelayCommand CheckUpdateYamlYugiCommand { get; set; }
        public RelayCommand CreateKonamiIDDBCommand { get; set; }
        public RelayCommand BrowseOfficialYamlYugiFolderPathCommand { get; set; }
        public RelayCommand BrowseRushYamlYugiFolderPathCommand { get; set; }
        public RelayCommand GetOfficialCardKonamiIDCommand { get; set; }
        public RelayCommand GetRushCardKonamiIDCommand { get; set; }
        #endregion

        #region Genesys
        public RelayCommand BrowseGenesysFilePathCommand { get; set; }
        public RelayCommand ExportGenesysCommand { get; set; }
        #endregion

        #region Terminilogy
        public RelayCommand BrowseTerminologyFolderPathCommand { get; set; }
        public RelayCommand ExportTerminologyCommand { get; set; }
        public RelayCommand BrowseSpecialCharFolderPathCommand { get; set; }
        public RelayCommand ExportSpecialCharactersCommand { get; set; }
        #endregion

        #region MD API
        public RelayCommand CheckCountMDAPICommand { get; set; }
        public RelayCommand LoadCardTypeMDAPICommand { get; set; }
        public RelayCommand LoadMonsterTypeMDAPICommand { get; set; }
        public RelayCommand LoadCardRaceMDAPICommand { get; set; }
        public RelayCommand LoadMonsterAttributeMDAPICommand { get; set; }
        public RelayCommand LoadLinkMarkerMDAPICommand { get; set; }
        public RelayCommand LoadCardRarityMDAPICommand { get; set; }
        public RelayCommand CheckATKDEFMDAPICommand { get; set; }

        public RelayCommand FetchJSONMDAPICommand { get; set; }
        public RelayCommand CancelLoadJSONMDAPICommand { get; set; }
        public RelayCommand BrowseJSONMDAPIFilePathCommand { get; set; }
        public RelayCommand LoadJSONFileMDAPICommand { get; set; }
        public RelayCommand ExportCardRarityListCommand { get; set; }
        #endregion

        public RelayCommand CancelCommand { get; set; }

        #endregion

        #region Constructor
        public DEVWindow()
        {
            InitializeComponent();
            InitializeCommands();

            _masterDuelApi = new MasterDuelAPIService();
            _fileService = new FileService();
            this.DataContext = this;
        }
        private void InitializeCommands()
        {
            #region Archetype
            BrowseEDOProArchetypeFilePathCommand = new RelayCommand(_ => BrowseEDOProArchetypeFilePath());
            BrowseMDProStringConfigFilePathCommand = new RelayCommand(_ => BrowseMDProStringConfigFilePath());
            ExportArchetypeEDOProCommand = new RelayCommand(_ => ExportArchetypeEDOPro());
            ExportArchetypeMDProCommand = new RelayCommand(_ => ExportArchetypeMDPro());
            #endregion

            #region Scrapiyard
            CheckUpdateScrapiyardCommand = new RelayCommand(async _ => await CheckUpdateScrapiyard());
            BrowseScrapiyardFolderPathCommand = new RelayCommand(_ => BrowseScrapiyardFolderPath());
            ExportScrapiyardConstantCommand = new RelayCommand(async _ => await ExportScrapiyardConstant());
            ExportScrapiyardEnumCommand = new RelayCommand(async _=> await ExportScrapiyardEnum());
            ExportScrapiyardFunctionCommand = new RelayCommand(async _ => await ExportScrapiyardFunction());
            ExportScrapiyardNameSpaceCommand = new RelayCommand(async _ => await ExportScrapiyardNameSpace());
            ExportScrapiyardTagCommand = new RelayCommand(async _ => await ExportScrapiyardTag());
            ExportScrapiyardTypeCommand = new RelayCommand(async _ => await ExportScrapiyardType());
            #endregion

            #region Yaml Yugi
            CheckUpdateYamlYugiCommand = new RelayCommand(async _ => await CheckUpdateYamlYugi());
            CreateKonamiIDDBCommand = new RelayCommand(async _ => await CreateKonamiIDDB());

            BrowseOfficialYamlYugiFolderPathCommand = new RelayCommand(_ => BrowseOfficialYamlYugiFolderPath());
            BrowseRushYamlYugiFolderPathCommand = new RelayCommand(_ => BrowseRushYamlYugiFolderPath());

            GetOfficialCardKonamiIDCommand = new RelayCommand(async _ => await GetOfficialCardKonamiID());
            GetRushCardKonamiIDCommand = new RelayCommand(async _ => await GetRushCardKonamiID());
            #endregion

            #region Genesys
            BrowseGenesysFilePathCommand = new RelayCommand(_ => BrowseGenesysFilePath());
            ExportGenesysCommand = new RelayCommand(_ => ExportGenesys());
            #endregion

            #region Terminilogy
            BrowseTerminologyFolderPathCommand = new RelayCommand(_ => BrowseTerminologyFolderPath());
            ExportTerminologyCommand = new RelayCommand(async _ => await ExportTerminology());
            BrowseSpecialCharFolderPathCommand = new RelayCommand(_ => BrowseSpecialCharFolderPath());
            ExportSpecialCharactersCommand = new RelayCommand(async _ => await ExportSpecialCharacters());
            #endregion

            #region MD API
            CheckCountMDAPICommand = new RelayCommand(async _ => await CheckApiCount());
            LoadCardTypeMDAPICommand = new RelayCommand(async _ => await LoadTypeList());
            LoadMonsterTypeMDAPICommand = new RelayCommand(async _ => await LoadMonsterTypeList());
            LoadCardRaceMDAPICommand = new RelayCommand(async _ => await LoadRaceList());
            LoadMonsterAttributeMDAPICommand = new RelayCommand(async _ => await LoadAttributeList());
            LoadLinkMarkerMDAPICommand = new RelayCommand(async _ => await LoadLinkArrowList());
            LoadCardRarityMDAPICommand = new RelayCommand(async _ => await LoadRarityList());
            CheckATKDEFMDAPICommand = new RelayCommand(async _ => await CheckAtkDefValue());

            FetchJSONMDAPICommand = new RelayCommand(async _ => await FetchAndSaveAsync());
            CancelLoadJSONMDAPICommand = new RelayCommand(_ => CancelFetch());
            BrowseJSONMDAPIFilePathCommand = new RelayCommand(_ => BrowseJSONMDAPIFilePath());
            LoadJSONFileMDAPICommand = new RelayCommand(async _ => await LoadJSONFileMDAPI());
            ExportCardRarityListCommand = new RelayCommand(async _ => await ExportCardRarityList());
            #endregion

            CancelCommand = new RelayCommand(_ => this.Close());
        }
        #endregion

        #region Command Methods

        #region Archetype
        private void BrowseEDOProArchetypeFilePath()
        {
            string filePath = FileDiaLogHelper.OpenLua();
            if (!string.IsNullOrEmpty(filePath)) EDooProArchetypeSetcodeFilePath = filePath;
        }
        private void BrowseMDProStringConfigFilePath()
        {
            string filePath = FileDiaLogHelper.OpenConf("MDPro Archetype");
            if (!string.IsNullOrEmpty(filePath)) MDproStringConfigFilePath = filePath;
        }

        private void ExportArchetypeEDOPro()
        {
            if (string.IsNullOrWhiteSpace(EDooProArchetypeSetcodeFilePath) ||
                !System.IO.File.Exists(EDooProArchetypeSetcodeFilePath)) return;

            try
            {
                var (resultExport, messageExport) = Archetype.GetEDOArchetypeList(EDooProArchetypeSetcodeFilePath);
                if (resultExport)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        "Export Archetype Successfully!", new[] { CMess.ok.ToText() });

                    if (!string.IsNullOrWhiteSpace(messageExport) && System.IO.File.Exists(messageExport))
                    {
                        System.Diagnostics.Process.Start("explorer.exe",
                            $"/select,\"{messageExport}\"");
                    }
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {messageExport}", new[] { CMess.ok.ToText() });
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private void ExportArchetypeMDPro()
        {
            if (string.IsNullOrWhiteSpace(MDproStringConfigFilePath) ||
                !System.IO.File.Exists(MDproStringConfigFilePath)) return;

            try
            {
                var (resultExport, messageExport) = Archetype.GetMDProArchetypeList(MDproStringConfigFilePath);
                if (resultExport)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        "Export Archetype Successfully!", new[] { CMess.ok.ToText() });

                    if (!string.IsNullOrWhiteSpace(messageExport) && System.IO.File.Exists(messageExport))
                    {
                        System.Diagnostics.Process.Start("explorer.exe",
                            $"/select,\"{messageExport}\"");
                    }
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {messageExport}", new[] { CMess.ok.ToText() });
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region Scrapiyard
        private async Task CheckUpdateScrapiyard()
        {
            // Đọc URL của các kho Git scrapiyard từ app.config
            string scrapiyardURL = ConfigurationManager.AppSettings["scrapiyardURL"];
            string scrapiyardPath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "scrapiyard");

            // Kiểm tra cấu hình
            if (string.IsNullOrWhiteSpace(scrapiyardURL))
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    "Scrapiyard URL is not configured.", new[] { CMess.ok.ToText() });
                return;
            }

            // Kiểm tra / clone / update repository
            var (result, message) = await GitHubService.CheckForUpdatesAsync(scrapiyardPath, scrapiyardURL);

            // Có lỗi
            if (!result && !string.IsNullOrEmpty(message))
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"Failed to update Scrapiyard: {message}", new[] { CMess.ok.ToText() });
                return;
            }

            // result == true:
            // - Folder rỗng -> đã clone
            // - Có update -> đã update
            //
            // result == false + message rỗng:
            // - Repository đã là version mới nhất
            if (result)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    "Scrapiyard has been updated successfully.", new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    "No updates found for Scrapiyard.", new[] { CMess.ok.ToText() });
            }
        }
        private void BrowseScrapiyardFolderPath()
        {
            string folderPath = FileDiaLogHelper.OpenFolder("Scrapiyard Folder");
            if (!string.IsNullOrWhiteSpace(folderPath)) ScrapiyardFolderPath = folderPath;
        }

        private async Task ExportScrapiyardConstant()
        {
            var (result, message) = await Task.Run(() => ScriptData.ExportConstantsToJson(ScrapiyardFolderPath));
            if (result)
            {
                CMSG.Show(CMess.infoma.ToText(), CMSG.MessageBoxIconType.Information,
                    $"Extract Constants Suc\n{message}", new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task ExportScrapiyardEnum()
        {
            var (result, message) = await Task.Run(() => ScriptData.ExportEnumsToJson(ScrapiyardFolderPath));
            if (result)
            {
                CMSG.Show(CMess.infoma.ToText(), CMSG.MessageBoxIconType.Information,
                    $"Extract Constants Suc\n{message}", new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task ExportScrapiyardFunction()
        {
            var (result, message) = await Task.Run(() => ScriptData.ExportFunctionsToJson(ScrapiyardFolderPath));
            if (result)
            {
                CMSG.Show(CMess.infoma.ToText(), CMSG.MessageBoxIconType.Information,
                    $"Extract Constants Suc\n{message}", new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task ExportScrapiyardNameSpace()
        {
            var (result, message) = await Task.Run(() => ScriptData.ExportNameSpacesToJson(ScrapiyardFolderPath));
            if (result)
            {
                CMSG.Show(CMess.infoma.ToText(), CMSG.MessageBoxIconType.Information,
                    $"Extract Constants Suc\n{message}", new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task ExportScrapiyardTag()
        {
            var (result, message) = await Task.Run(() => ScriptData.ExportTagToJson(ScrapiyardFolderPath));
            if (result)
            {
                CMSG.Show(CMess.infoma.ToText(), CMSG.MessageBoxIconType.Information,
                    $"Extract Constants Suc\n{message}", new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task ExportScrapiyardType()
        {
            var (result, message) = await Task.Run(() => ScriptData.ExportTypesToJson(ScrapiyardFolderPath));
            if (result)
            {
                CMSG.Show(CMess.infoma.ToText(), CMSG.MessageBoxIconType.Information,
                    $"Extract Constants Suc\n{message}", new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region Yaml Yugi
        private async Task CheckUpdateYamlYugi()
        {
            // Đọc URL của các kho Git scrapiyard từ app.config
            string yamlyugiURL = ConfigurationManager.AppSettings["yamlyugiURL"];
            string yamlyugiPath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "yaml-yugi");

            // Kiểm tra cấu hình
            if (string.IsNullOrWhiteSpace(yamlyugiURL))
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    "yamlyugi URL is not configured.", new[] { CMess.ok.ToText() });
                return;
            }

            // Kiểm tra / clone / update repository
            var (result, message) = await GitHubService.CheckForUpdatesAsync(yamlyugiPath, yamlyugiURL);

            // Có lỗi
            if (!result && !string.IsNullOrEmpty(message))
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"Failed to update yaml-yugi: {message}", new[] { CMess.ok.ToText() });
                return;
            }

            // result == true:
            // - Folder rỗng -> đã clone
            // - Có update -> đã update
            //
            // result == false + message rỗng:
            // - Repository đã là version mới nhất
            if (result)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    "yaml-yugi has been updated successfully.", new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    "No updates found for yaml-yugi.", new[] { CMess.ok.ToText() });
            }
        }
        private async Task CreateKonamiIDDB()
        {
            var (result, message) = await GetKonamiIDService.CreateDatabase();
            if (result) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                "Database created successfully.", new[] { CMess.ok.ToText() });
            else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
        }

        private void BrowseOfficialYamlYugiFolderPath()
        {
            string folderPath = FileDiaLogHelper.OpenFolder("Official YamlYugi Folder");
            if (!string.IsNullOrWhiteSpace(folderPath)) OfficialYamlYugiFolderPath = folderPath;
        }
        private void BrowseRushYamlYugiFolderPath()
        {
            string folderPath = FileDiaLogHelper.OpenFolder("Rush YamlYugi Folder");
            if (!string.IsNullOrWhiteSpace(folderPath)) RushYamlYugiFolderPath = folderPath;
        }

        private async Task GetOfficialCardKonamiID()
        {
            var (result, message) = await GetKonamiIDService.GetOfficialKonamiID(OfficialYamlYugiFolderPath);
            CMSG.Show(result ? CMess.notifi.ToText() : CMess.error.ToText(),
                result ? CMSG.MessageBoxIconType.Notification : CMSG.MessageBoxIconType.Error,
                message, new[] { CMess.ok.ToText() });
        }
        private async Task GetRushCardKonamiID()
        {
            var (result, message) = await GetKonamiIDService.GetRushKonamiID(RushYamlYugiFolderPath);
            CMSG.Show(result ? CMess.notifi.ToText() : CMess.error.ToText(),
                result ? CMSG.MessageBoxIconType.Notification : CMSG.MessageBoxIconType.Error,
                message, new[] { CMess.ok.ToText() });
        }
        #endregion

        #region Genesys
        private void BrowseGenesysFilePath()
        {
            string filePath = FileDiaLogHelper.OpenText();
            if (!string.IsNullOrWhiteSpace(filePath)) GenesysFilepath = filePath;
        }
        private async Task ExportGenesys()
        {
            var (result, filePath, message) = await GenesysID.ProcessCardDataAsync(GenesysFilepath);
            if (result)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    message, new[] { CMess.ok.ToText() });

                if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("explorer.exe",
                        $"/select,\"{filePath}\"");
                }
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    message, new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region Terminilogy
        private void BrowseTerminologyFolderPath()
        {
            string selectFolder = FileDiaLogHelper.OpenFolder("Terminology Folder");
            if (!string.IsNullOrWhiteSpace(selectFolder)) TerminologyFolderPath = selectFolder;
        }
        private async Task ExportTerminology()
        {
            var (result, message, filePath) = await Terminology.ExtractTerminology(TerminologyFolderPath, CardAppContext.Instance.DataFolderPath);

            if (result)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    message, new[] { CMess.ok.ToText() });

                if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("explorer.exe",
                        $"/select,\"{filePath}\"");
                }
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    message, new[] { CMess.ok.ToText() });
            }
        }
        private void BrowseSpecialCharFolderPath()
        {
            string selectFolder = FileDiaLogHelper.OpenFolder("Data source Folder (for Special Character)");
            if (!string.IsNullOrWhiteSpace(selectFolder)) SpecialCharFolderPath = selectFolder;
        }
        private async Task ExportSpecialCharacters()
        {
            var (result, message, filePath) = await Terminology.ExtractSpecialCharacters(SpecialCharFolderPath, CardAppContext.Instance.DataFolderPath);

            if (result)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    message, new[] { CMess.ok.ToText() });

                if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start("explorer.exe",
                        $"/select,\"{filePath}\"");
                }
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    message, new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region MD API

        private CancellationTokenSource _cardMasterDuelcts;

        #region MD API Scanner
        private async Task CheckApiCount()
        {
            using HttpClient client = new HttpClient();
            int page = 1;
            int totalItems = 0;

            while (true)
            {
                string url = $"{MasterDuelAPIURL}?page={page}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Page không tồn tại
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        StatusText = "Page not found, Break Loop";
                        break;
                    }

                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();
                    using JsonDocument document = JsonDocument.Parse(json);
                    int count = document.RootElement.GetArrayLength();

                    if (count == 0)
                    {
                        StatusText = "Page no data, Break Loop";
                        break;
                    }

                    totalItems += count;

                    StatusText = $"Result: Page {page}: {count} items | Total: {totalItems:N0}";
                    page++;
                }
                catch (HttpRequestException ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");
                }
            }
            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"API Count Result:\nTotal pages: {page - 1}, Total items: {totalItems:N0}", new[] { CMess.ok.ToText() });
        }
        private async Task LoadTypeList()
        {
            try
            {
                HashSet<string> ResultType = new(StringComparer.Ordinal);
                using HttpClient client = new HttpClient();

                int page = 1;

                while (true)
                {
                    string url = $"{MasterDuelAPIURL}?page={page}";

                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(url);

                        // Page không tồn tại
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            StatusText = "Page not found, Break Loop";

                            break;
                        }

                        response.EnsureSuccessStatusCode();

                        string json = await response.Content.ReadAsStringAsync();

                        using JsonDocument document = JsonDocument.Parse(json);

                        int count = document.RootElement.GetArrayLength();

                        // Page không có data
                        if (count == 0)
                        {
                            StatusText = "Page no data, Break Loop";

                            break;
                        }

                        int newItems = 0;

                        // Scan từng Card
                        foreach (JsonElement card in document.RootElement.EnumerateArray())
                        {
                            // Lấy type
                            if (!card.TryGetProperty("type", out JsonElement type))
                                continue;

                            if (type.ValueKind != JsonValueKind.String)
                                continue;

                            string? value = type.GetString();

                            if (string.IsNullOrWhiteSpace(value))
                                continue;

                            // HashSet tự động bỏ qua duplicate
                            if (ResultType.Add(value))
                            {
                                newItems++;
                            }
                        }

                        string resultContent =
                            $"Page {page}: {count} cards | " +
                            $"New Types: {newItems} | " +
                            $"Total Types: {ResultType.Count}";

                        StatusText = $"Result: {resultContent}";

                        page++;
                    }
                    catch (HttpRequestException ex)
                    {
                        LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                        break;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                        break;
                    }
                }

                string TypeListFile = string.Join(Environment.NewLine, ResultType.OrderBy(x => x));
                string TypeListUI = string.Join(", ", ResultType.OrderBy(x => x));

                string filePath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "MasterDuelAPI", "CardTypes.txt");
                _fileService.WriteToFile(TypeListFile, filePath);
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    $"Type Scan Result\n{TypeListUI}",
                    new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog( $"LoadTypeList Error: {ex}");

                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Error,
                    $"Load Type List failed.\n\n{ex.Message}",
                    new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadMonsterTypeList()
        {
            HashSet<string> ResultMonsterType = new(StringComparer.Ordinal);
            using HttpClient client = new HttpClient();

            int page = 1;

            while (true)
            {
                string url = $"{MasterDuelAPIURL}?page={page}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Page không tồn tại
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        StatusText = "Page not found, Break Loop";

                        break;
                    }

                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    using JsonDocument document = JsonDocument.Parse(json);

                    int count = document.RootElement.GetArrayLength();

                    // Page không có data
                    if (count == 0)
                    {
                        StatusText = "Page no data, Break Loop";

                        break;
                    }

                    int newItems = 0;

                    // Scan từng Card
                    foreach (JsonElement card in document.RootElement.EnumerateArray())
                    {
                        // Lấy monsterType
                        if (!card.TryGetProperty("monsterType", out JsonElement monsterType))
                            continue;

                        // monsterType là Array
                        if (monsterType.ValueKind != JsonValueKind.Array)
                            continue;

                        foreach (JsonElement type in monsterType.EnumerateArray())
                        {
                            if (type.ValueKind != JsonValueKind.String)
                                continue;

                            string? value = type.GetString();

                            if (string.IsNullOrWhiteSpace(value))
                                continue;

                            // HashSet tự động bỏ qua duplicate
                            if (ResultMonsterType.Add(value))
                            {
                                newItems++;
                            }
                        }
                    }

                    string resultContent =
                        $"Page {page}: {count} cards | " +
                        $"New Monster Types: {newItems} | " +
                        $"Total Monster Types: {ResultMonsterType.Count}";

                    StatusText = $"Result: {resultContent}";

                    page++;
                }

                catch (HttpRequestException ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
            }

            string MonsterTypeListFile = string.Join(Environment.NewLine, ResultMonsterType.OrderBy(x => x));
            string MonsterTypeListUI = string.Join(", ", ResultMonsterType.OrderBy(x => x));
            _fileService.WriteToFile(MonsterTypeListFile, System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "MasterDuelAPI/MonsterTypes.txt"));

            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"Monster Type Scan Result\n{MonsterTypeListUI}", new[] { CMess.ok.ToText() });
        }
        private async Task LoadRaceList()
        {
            HashSet<string> ResultRace = new(StringComparer.Ordinal);
            using HttpClient client = new HttpClient();

            int page = 1;

            while (true)
            {
                string url = $"{MasterDuelAPIURL}?page={page}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Page không tồn tại
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        StatusText = "Page not found, Break Loop";

                        break;
                    }

                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    using JsonDocument document = JsonDocument.Parse(json);

                    int count = document.RootElement.GetArrayLength();

                    // Page không có data
                    if (count == 0)
                    {
                        StatusText = "Page no data, Break Loop";

                        break;
                    }

                    int newItems = 0;

                    // Scan từng Card
                    foreach (JsonElement card in document.RootElement.EnumerateArray())
                    {
                        // Lấy race
                        if (!card.TryGetProperty("race", out JsonElement race))
                            continue;

                        if (race.ValueKind != JsonValueKind.String)
                            continue;

                        string? value = race.GetString();

                        if (string.IsNullOrWhiteSpace(value))
                            continue;

                        // HashSet tự động bỏ qua duplicate
                        if (ResultRace.Add(value))
                        {
                            newItems++;
                        }
                    }

                    string resultContent =
                        $"Page {page}: {count} cards | " +
                        $"New Races: {newItems} | " +
                        $"Total Races: {ResultRace.Count}";

                    StatusText = $"Result: {resultContent}";

                    page++;
                }
                catch (HttpRequestException ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
            }

            string RaceListFile = string.Join(Environment.NewLine, ResultRace.OrderBy(x => x));
            string RaceListUI = string.Join(", ", ResultRace.OrderBy(x => x));
            _fileService.WriteToFile(RaceListFile, System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "MasterDuelAPI/Races.txt"));

            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"Race Scan Result\n{RaceListUI}", new[] { CMess.ok.ToText() });
        }
        private async Task LoadAttributeList()
        {
            HashSet<string> ResultAttribute = new(StringComparer.Ordinal);
            using HttpClient client = new HttpClient();

            int page = 1;

            while (true)
            {
                string url = $"{MasterDuelAPIURL}?page={page}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Page không tồn tại
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        StatusText = "Page not found, Break Loop";

                        break;
                    }

                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    using JsonDocument document = JsonDocument.Parse(json);

                    int count = document.RootElement.GetArrayLength();

                    // Page không có data
                    if (count == 0)
                    {
                        StatusText = "Page no data, Break Loop";

                        break;
                    }

                    int newItems = 0;

                    // Scan từng Card
                    foreach (JsonElement card in document.RootElement.EnumerateArray())
                    {
                        // Lấy attribute
                        if (!card.TryGetProperty("attribute", out JsonElement attribute))
                            continue;

                        if (attribute.ValueKind != JsonValueKind.String)
                            continue;

                        string? value = attribute.GetString();

                        if (string.IsNullOrWhiteSpace(value))
                            continue;

                        // HashSet tự động bỏ qua duplicate
                        if (ResultAttribute.Add(value))
                        {
                            newItems++;
                        }
                    }

                    string resultContent =
                        $"Page {page}: {count} cards | " +
                        $"New Attributes: {newItems} | " +
                        $"Total Attributes: {ResultAttribute.Count}";

                    StatusText = $"Result: {resultContent}";

                    page++;
                }
                catch (HttpRequestException ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
            }

            string AttributeListFile = string.Join(Environment.NewLine, ResultAttribute.OrderBy(x => x));
            string AttributeListUI = string.Join(", ", ResultAttribute.OrderBy(x => x));
            _fileService.WriteToFile(AttributeListFile, System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "MasterDuelAPI/Attributes.txt"));

            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"Attribute Scan Result\n{AttributeListUI}", new[] { CMess.ok.ToText() });
        }
        private async Task CheckAtkDefValue()
        {
            string CardName = null;
            if (string.IsNullOrWhiteSpace(CardName))
            {
                StatusText = "Card name is empty";
                return;
            }
            await CheckCardAtkDef(CardName);
        }
        private async Task CheckCardAtkDef(string cardName)
        {
            using HttpClient client = new HttpClient();

            JsonElement? foundCard = null;
            string? name = null;
            string? atk = null;
            string? def = null;
            int page = 1;

            while (true)
            {
                string url = $"{MasterDuelAPIURL}?page={page}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Page không tồn tại
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        StatusText = "Page not found, Break Loop";

                        break;
                    }

                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    using JsonDocument document = JsonDocument.Parse(json);

                    int count = document.RootElement.GetArrayLength();

                    // Page không có data
                    if (count == 0)
                    {
                        StatusText = "Page no data, Break Loop";

                        break;
                    }

                    foreach (JsonElement card in document.RootElement.EnumerateArray())
                    {
                        if (!card.TryGetProperty("name", out JsonElement nameElement))
                            continue;
                        string? currentName = nameElement.GetString();

                        if (!string.Equals(currentName, cardName, StringComparison.Ordinal))
                            continue;

                        name = currentName;

                        atk = card.TryGetProperty("atk", out JsonElement atkElement)
                        ? atkElement.ToString()
                        : "[Missing]";

                        def = card.TryGetProperty("def", out JsonElement defElement)
                        ? defElement.ToString()
                        : "[Missing]";

                        foundCard = card.Clone();
                        break;
                    }

                    if (name != null) break;

                    page++;
                }
                catch (HttpRequestException ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
            }

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(atk) && !string.IsNullOrEmpty(def))
            {
                StatusText = $"Card Found: Name: {name} | ATK: {atk} | DEF: {def}";
            }
            else
            {
                StatusText = $"Card Not Found: Card with name '{cardName}' not found.";
            }

            string AtkDefValue = !string.IsNullOrEmpty(atk) && !string.IsNullOrEmpty(def) ? $"{atk}/{def}" : "[Not Found]";

            if (foundCard.HasValue)
            {
                string cardJson = JsonSerializer.Serialize(
                    foundCard.Value,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                string fileName = SanitizeStringHelper.SanitizeWindowsFileName(name ?? "UnknownCard");
                _fileService.WriteToFile(cardJson, System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, $"MasterDuelAPICard/{fileName}.txt"));
            }
        }
        private async Task LoadLinkArrowList()
        {
            using HttpClient client = new HttpClient();

            HashSet<string> resultLinkArrows = new();
            int page = 1;

            while (true)
            {
                string url = $"{MasterDuelAPIURL}?page={page}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Page không tồn tại
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        StatusText = "Page not found, Break Loop";

                        break;
                    }

                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    using JsonDocument document = JsonDocument.Parse(json);

                    int count = document.RootElement.GetArrayLength();

                    // Page không có data
                    if (count == 0)
                    {
                        StatusText = "Page no data, Break Loop";

                        break;
                    }

                    foreach (JsonElement card in document.RootElement.EnumerateArray())
                    {
                        if (!card.TryGetProperty("linkArrows", out JsonElement linkArrowsElement))
                            continue;

                        if (linkArrowsElement.ValueKind != JsonValueKind.Array)
                            continue;

                        foreach (JsonElement arrowElement in linkArrowsElement.EnumerateArray())
                        {
                            string? arrow = arrowElement.GetString();

                            if (!string.IsNullOrWhiteSpace(arrow))
                            {
                                resultLinkArrows.Add(arrow);
                            }
                        }
                    }

                    StatusText = $"Result: Page {page}: {count} items | Link Arrows: {resultLinkArrows.Count}";

                    page++;
                }
                catch (HttpRequestException ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
            }

            string LinkArrowListFile = string.Join(Environment.NewLine, resultLinkArrows.OrderBy(x => x));
            string LinkArrowListUI = string.Join(", ", resultLinkArrows.OrderBy(x => x));
            _fileService.WriteToFile(LinkArrowListFile, System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "MasterDuelAPI/LinkArrows.txt"));


            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"Link Arrow Scan Result\n{LinkArrowListUI}", new[] { CMess.ok.ToText() });
        }
        private async Task LoadRarityList()
        {
            HashSet<string> ResultRarity = new(StringComparer.Ordinal);
            using HttpClient client = new HttpClient();

            int page = 1;

            while (true)
            {
                string url = $"{MasterDuelAPIURL}?page={page}";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Page không tồn tại
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        StatusText = "Page not found, Break Loop";

                        break;
                    }

                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    using JsonDocument document = JsonDocument.Parse(json);

                    int count = document.RootElement.GetArrayLength();

                    // Page không có data
                    if (count == 0)
                    {
                        StatusText = "Page no data, Break Loop";

                        break;
                    }

                    int newItems = 0;

                    // Scan từng Card
                    foreach (JsonElement card in document.RootElement.EnumerateArray())
                    {
                        // Lấy rarity
                        if (!card.TryGetProperty("rarity", out JsonElement rarity))
                            continue;

                        if (rarity.ValueKind != JsonValueKind.String)
                            continue;

                        string? value = rarity.GetString();

                        if (string.IsNullOrWhiteSpace(value))
                            continue;

                        // HashSet tự động bỏ qua duplicate
                        if (ResultRarity.Add(value))
                        {
                            newItems++;
                        }
                    }

                    string resultContent =
                        $"Page {page}: {count} cards | " +
                        $"New Rarities: {newItems} | " +
                        $"Total Rarities: {ResultRarity.Count}";

                    StatusText = $"Result: {resultContent}";

                    page++;
                }
                catch (HttpRequestException ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog($"Error: Page: {page}, Error: {ex.Message}");

                    break;
                }
            }

            string CardRarityListFile = string.Join(Environment.NewLine, ResultRarity.OrderBy(x => x));
            string CardRarityListUI = string.Join(", ", ResultRarity.OrderBy(x => x));
            _fileService.WriteToFile(CardRarityListFile, System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "MasterDuelAPI/CardRarities.txt"));

            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                $"Rarity Scan Result\n{CardRarityListUI}", new[] { CMess.ok.ToText() });
        }
        #endregion

        #region API Loaders
        private async Task FetchAndSaveAsync()
        {
            if (string.IsNullOrWhiteSpace(MasterDuelAPIURL)) return;
            string _cachePath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, "MasterDuelAPI/CardsMasterDuel.json");

            IsLoading = true;
            _cardMasterDuelcts = new CancellationTokenSource();
            // Báo tiến trình về UI thread
            var progress = new Progress<(int page, int totalCards)>(p =>
            {
                CurrentPage = p.page;
                TotalCardsLoaded = p.totalCards;
                StatusText = $"Đang tải trang {p.page}... ({p.totalCards} card)";
            });

            try
            {
                var (loadOk, loadMsg, cardsList) = await _masterDuelApi.LoadCardMasterDuelList(
                    MasterDuelAPIURL, progress, _cardMasterDuelcts.Token);

                if (!loadOk)
                {
                    StatusText = $"Lỗi: {loadMsg}";
                    MessageBox.Show(loadMsg, "Lỗi tải dữ liệu",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var (saveOk, saveMsg) = await _masterDuelApi.SaveCardMasterDuelList(cardsList, _cachePath);

                StatusText = saveOk ? saveMsg : $"Tải OK nhưng lưu lỗi: {saveMsg}";

                MessageBox.Show(saveMsg, saveOk ? "Thành công" : "Lỗi",
                    MessageBoxButton.OK,
                    saveOk ? MessageBoxImage.Information : MessageBoxImage.Error);

                if (saveOk)
                {
                    CardsMasterDuel.Clear();
                    CardsMasterDuel.AddRange(cardsList);
                }
            }
            catch (OperationCanceledException)
            {
                StatusText = "Đã hủy.";
            }
            catch (Exception ex)
            {
                StatusText = $"Lỗi: {ex.Message}";
                MessageBox.Show(ex.Message, "Lỗi tải dữ liệu",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                _cardMasterDuelcts.Dispose();
                _cardMasterDuelcts = null;
            }
        }
        private void CancelFetch()
        {
            _cardMasterDuelcts?.Cancel();
        }
        private void BrowseJSONMDAPIFilePath()
        {
            string filePath = FileDiaLogHelper.OpenJSON();
            if (!string.IsNullOrEmpty(filePath)) JsonMDAPIFilePath = filePath;
        }
        private async Task LoadJSONFileMDAPI()
        {
            if (string.IsNullOrWhiteSpace(JsonMDAPIFilePath) || !System.IO.File.Exists(JsonMDAPIFilePath)) return;

            var (resultLoad, messageLoad) = await MasterDuelAPIViewModel.Instance.LoadJSON(JsonMDAPIFilePath);
            if (resultLoad)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Json.ToText(), CMess.load.ToText()),
                    new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageLoad}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task ExportCardRarityList()
        {
            if (!MasterDuelAPIViewModel.Instance.isLoaded)
            {
                if (string.IsNullOrWhiteSpace(JsonMDAPIFilePath) || !System.IO.File.Exists(JsonMDAPIFilePath)) return;
                var (resultLoad, messageLoad) = await MasterDuelAPIViewModel.Instance.LoadJSON(JsonMDAPIFilePath);
                if (!resultLoad)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {messageLoad}", new[] { CMess.ok.ToText() });
                    return;
                }
            }

            var (resultBuild, messageBuild) = MasterDuelAPIViewModel.Instance.BuildRarityMap();
            if (!resultBuild)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageBuild}", new[] { CMess.ok.ToText() });
                return;
            }

            string selectedFilePath = FileDiaLogHelper.SaveJSON();
            if (string.IsNullOrWhiteSpace(selectedFilePath)) return;

            var (resultSave, messageSave) = await MasterDuelAPIViewModel.Instance.SaveRarityMap(selectedFilePath);
            if (resultSave)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                   messageSave, new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                   messageSave, new[] { CMess.ok.ToText() });
            }
        }
        #endregion

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
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        #endregion
    }
}
