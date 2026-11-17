using System.IO;
using System.Windows;
using System.Windows.Controls;
using ScriptSupport.Models;
using ScriptSupport.States;
using ScriptSupport.Stores;
using ScriptSupport.Manager;
using ScriptSupport.Factorys;
using ScriptSupport.Interfaces;
using ScriptSupport.ViewModels;
using ScriptSupport.Localization;
using CMess = ScriptSupport.Localization.Language;

namespace ScriptSupport.UserControls
{
    /// <summary>
    /// Interaction logic for MainUserControl.xaml
    /// </summary>
    public partial class MainUserControl : UserControl
    {
        public UIConfigState UIConfig { get; }
        private readonly ConfigStore _config;
        private readonly ICardElelemtInterface _cardElelemtInterface;
        private readonly ICardInterface _cardInterface;
        private readonly IScriptInterface _scriptInterface;
        private readonly IImageCardInterface _imageCardInterface;
        private readonly IKonamiIDInterface _konamiIDInterface;
        private readonly IGenesysInterface _genesysInterface;
        private readonly IScrapiInterface _scrapiInterface;
        private readonly IScrapiyardInterface _scrapiyardInterface;
        private readonly ISpecialCharInterface _specialCharInterface;
        private readonly IDataFolderInterface _dataFolderInterface;
        private readonly IFloatingPanelInterface _panelService;
        private readonly IDialogInterface _dialogInterface;
        private FloatManager? _floatManager;
        private readonly PanelFactory _panelPactory;

        public MainUserControl(UIConfigState _uIConfig, ConfigStore config,
            MainViewModel vm, CardFilter cardFilter, ResultView resultView,
            IUIConfigInterface uIConfigInterface, ICardElelemtInterface cardElement,
            ICardInterface cardInterface, IScriptInterface scriptInterface, IImageCardInterface imageCardInterface,
            IKonamiIDInterface konamiIDInterface, IGenesysInterface genesysInterface,
            IScrapiInterface scrapiInterface, IScrapiyardInterface scrapiyardInterface,
            ISpecialCharInterface specialCharInterface,
            IDataFolderInterface dataFolderInterface, IFloatingPanelInterface panelService,
            PanelFactory panelPactory,
            IDialogInterface dialogInterface)
        {
            UIConfig = _uIConfig;
            _config = config;
            _cardElelemtInterface = cardElement;
            _cardInterface = cardInterface;
            _scriptInterface = scriptInterface;
            _imageCardInterface = imageCardInterface;
            _konamiIDInterface = konamiIDInterface;
            _genesysInterface = genesysInterface;
            _scrapiInterface = scrapiInterface;
            _scrapiyardInterface = scrapiyardInterface;
            _specialCharInterface = specialCharInterface;
            _dataFolderInterface = dataFolderInterface;
            _panelService = panelService;
            _panelPactory = panelPactory;
            _dialogInterface = dialogInterface;

            // _panelService.ShowRequested += OnShowRequested;
            InitializeComponent();
            DataContext = vm;
            CardFilterContent.Content = cardFilter;
            ResultContent.Content = resultView;
        }
        private void OnShowRequested(Type vmType)
        {
            Dispatcher.Invoke(() =>
            {
                _floatManager?.Show(vmType);
            });
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _floatManager = new FloatManager(UIConfig, OverlayCanvas, _panelPactory);
            _panelService.ShowRequested += OnShowRequested;

            await _cardElelemtInterface.LoadAllCardElement();

            var (SuccessKonamiID, MessageKonamiID) = await _konamiIDInterface.LoadKonamiIDAsync();
            var (SuccessSpecialChar, MessageSpecialChar) = await _specialCharInterface.LoadChar();
            if (!SuccessKonamiID)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = MessageBoxIconType.Warning,
                    Message = MessageKonamiID,
                    Buttons = new[] { CMess.ok.ToText() },
                    DefaultButtonIndex = 0,
                    ResponseSource = new TaskCompletionSource<int>()
                };
                await _dialogInterface.ShowMessage(request);
            }
            if (!SuccessSpecialChar)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = MessageBoxIconType.Warning,
                    Message = MessageSpecialChar,
                    Buttons = new[] { CMess.ok.ToText() },
                    DefaultButtonIndex = 0,
                    ResponseSource = new TaskCompletionSource<int>()
                };
                await _dialogInterface.ShowMessage(request);
            }

            string dataSourcePath = _config.UserSetting.DataSource;
            if (!string.IsNullOrEmpty(dataSourcePath) && Directory.Exists(dataSourcePath))
            {
                var (SuccessDB, MessageDB) = await _cardInterface.LoadCardDBAsync();
                var (SuccessSc, MessageSc) = await _scriptInterface.LoadScriptsAsync();
                var (SuccessGP, MessageGP) = await _genesysInterface.LoadGenesysDB();
                var (SuccessImg, MessageImg) = await _imageCardInterface.LoadCardImagesAsync();
                if (!SuccessDB)
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.warning.ToText(),
                        IconType = MessageBoxIconType.Warning,
                        Message = MessageDB,
                        Buttons = new[] { CMess.ok.ToText() },
                        DefaultButtonIndex = 0,
                        ResponseSource = new TaskCompletionSource<int>()
                    };
                    await _dialogInterface.ShowMessage(request);
                }
                if (!SuccessSc)
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.warning.ToText(),
                        IconType = MessageBoxIconType.Warning,
                        Message = MessageSc,
                        Buttons = new[] { CMess.ok.ToText() },
                        DefaultButtonIndex = 0,
                        ResponseSource = new TaskCompletionSource<int>()
                    };
                    await _dialogInterface.ShowMessage(request);
                }
                if (!SuccessGP)
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.warning.ToText(),
                        IconType = MessageBoxIconType.Warning,
                        Message = MessageGP,
                        Buttons = new[] { CMess.ok.ToText() },
                        DefaultButtonIndex = 0,
                        ResponseSource = new TaskCompletionSource<int>()
                    };
                    await _dialogInterface.ShowMessage(request);
                }
                if (!SuccessImg)
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.warning.ToText(),
                        IconType = MessageBoxIconType.Warning,
                        Message = MessageImg,
                        Buttons = new[] { CMess.ok.ToText() },
                        DefaultButtonIndex = 0,
                        ResponseSource = new TaskCompletionSource<int>()
                    };
                    await _dialogInterface.ShowMessage(request);
                }
            }
            else
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = MessageBoxIconType.Warning,
                    Message = CMess.dataSourceMiss.ToText(),
                    Buttons = new[] { CMess.ok.ToText() },
                    DefaultButtonIndex = 0,
                    ResponseSource = new TaskCompletionSource<int>()
                };
                await _dialogInterface.ShowMessage(request);
            }

            #region Scrapiyard
            var (SuccessScrapiyard, MessageScrapiyard) = await _scrapiyardInterface.LoadSymbols();
            if (!SuccessScrapiyard)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = MessageBoxIconType.Warning,
                    Message = MessageScrapiyard,
                    Buttons = new[] { CMess.ok.ToText() },
                    DefaultButtonIndex = 0,
                    ResponseSource = new TaskCompletionSource<int>()
                };
                await _dialogInterface.ShowMessage(request);
            }

            //var (successScrapi, MessageScrapi) = await _dataFolderInterface.CheckScrapiyardFolder();
            //if (!successScrapi)
            //{
            //    var request = new MessageBoxRequest
            //    {
            //        Title = CMess.warning.ToText(),
            //        IconType = MessageBoxIconType.Warning,
            //        Message = MessageScrapi,
            //        Buttons = new[] { CMess.ok.ToText() },
            //        DefaultButtonIndex = 0,
            //        ResponseSource = new TaskCompletionSource<int>()
            //    };
            //    await _dialogInterface.ShowMessage(request);
            //}
            //else
            //{
            //    var (successScrapiLoad, MessageScrapiLoad) = await _scrapiInterface.LoadScrapisAsync();
            //    if (!successScrapiLoad)
            //    {
            //        var request = new MessageBoxRequest
            //        {
            //            Title = CMess.warning.ToText(),
            //            IconType = MessageBoxIconType.Warning,
            //            Message = MessageScrapiLoad,
            //            Buttons = new[] { CMess.ok.ToText() },
            //            DefaultButtonIndex = 0,
            //            ResponseSource = new TaskCompletionSource<int>()
            //        };
            //        await _dialogInterface.ShowMessage(request);
            //    }
            //}
            #endregion
        }
    }
}
