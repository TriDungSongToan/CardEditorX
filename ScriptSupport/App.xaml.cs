using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ScriptSupport.Views;
using ScriptSupport.States;
using ScriptSupport.Stores;
using ScriptSupport.Manager;
using ScriptSupport.Factorys;
using ScriptSupport.Services;
using ScriptSupport.ViewModels;
using ScriptSupport.Interfaces;
using ScriptSupport.Environment;
using ScriptSupport.Localization;
using ScriptSupport.UserControls;
using ScriptSupport.Editor.Analysis;

namespace ScriptSupport
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;
        public IServiceProvider Services => _host?.Services
            ?? throw new InvalidOperationException("Host has not been initialized.");

        public App()
        {

        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                var builder = Host.CreateApplicationBuilder(e.Args);
                var isStandalone = !e.Args.Contains("--embedded");

                builder.Services.AddSingleton<AppEnvironment>();
                builder.Services.AddSingleton(new AppRuntimeConfig());
                builder.Services.AddSingleton<IApplicationInitializer, ApplicationInitializer>();
                builder.Services.AddSingleton<IApplicationInterface, ApplicationService>();

                builder.Services.AddSingleton<ImagePathResolver>();
                builder.Services.AddSingleton<IImageAppInterface, ImageAppService>();
                builder.Services.AddSingleton<IDataFolderInterface, DataFolderService>();
                builder.Services.AddSingleton<IDialogInterface, DialogService>();
                builder.Services.AddSingleton<IHighlightInterface, HighLightService>();

                builder.Services.AddSingleton<EditorCommandsService>();
                builder.Services.AddSingleton<UIConfigState>();
                builder.Services.AddSingleton<FilterConfigState>();
                builder.Services.AddSingleton<FilterCardState>();
                builder.Services.AddSingleton<ResultState>();
                builder.Services.AddSingleton<HighlightState>();

                builder.Services.AddSingleton<IUIConfigInterface, UIConfigService>();


                builder.Services.AddSingleton<IFilterConfigInterface, FilterConfigService>();
                builder.Services.AddSingleton<IConfigInterface, ConfigService>();
                builder.Services.AddSingleton<ICardElelemtInterface, CardElelemtService>();
                builder.Services.AddSingleton<ICardInterface, CardService>();
                builder.Services.AddSingleton<IGenesysInterface, GenesysService>();
                builder.Services.AddSingleton<IScriptInterface, ScriptService>();
                builder.Services.AddSingleton<IScrapiInterface, ScrapiService>();
                builder.Services.AddSingleton<IScrapiyardInterface, ScrapiyardService>();
                builder.Services.AddSingleton<IImageCardInterface, ImageCardService>();
                builder.Services.AddSingleton<IKonamiIDInterface, KonamiIDService>();
                builder.Services.AddSingleton<IHelpInterface, HelpService>();
                builder.Services.AddSingleton<ISpecialCharInterface, SpecialCharService>();
                builder.Services.AddSingleton<ILauncherInterface, LauncherService>();
                builder.Services.AddSingleton<IItemsSourceInterface, ItemsSourceService>();
                builder.Services.AddSingleton<IStringInterface, StringService>();
                builder.Services.AddSingleton<IEditorServiceFactory, EditorServiceFactory>();

                builder.Services.AddSingleton<IResultInterface, ResultService>();

                builder.Services.AddSingleton<ConfigStore>();
                builder.Services.AddSingleton<CardElementStore>();
                builder.Services.AddSingleton<CardStore>();
                builder.Services.AddSingleton<ScriptStore>();
                builder.Services.AddSingleton<ScrapiStore>();
                builder.Services.AddSingleton<ScrapiyardStore>();
                builder.Services.AddSingleton<ImageCardStore>();
                builder.Services.AddSingleton<KonamiIDStore>();
                builder.Services.AddSingleton<SpecialCharStore>();
                builder.Services.AddSingleton<DocumentFactory>();
                builder.Services.AddSingleton<PanelFactory>();
                builder.Services.AddSingleton<IFloatingPanelInterface, FloatingPanelService>();
                builder.Services.AddSingleton<ILanguageInterface, LanguageService>();
                builder.Services.AddSingleton<LocalizationProxy>();

                builder.Services.AddSingleton<LinkMarkerViewModel>();
                builder.Services.AddSingleton<CardTextFilterViewModel>();
                builder.Services.AddSingleton<CardDataFilterViewModel>();
                builder.Services.AddSingleton<CardInfoViewModel>();
                builder.Services.AddSingleton<CardFilterViewModel>();
                builder.Services.AddSingleton<ResultViewModel>();

                builder.Services.AddTransient<ScriptDescViewModel>();
                builder.Services.AddTransient<Func<ScriptDescViewModel>>(sp => () => sp.GetRequiredService<ScriptDescViewModel>());
                builder.Services.AddTransient<ISymbolDescriptionPresenter, ScriptDescriptionPresenter>();
                builder.Services.AddTransient<DocumentViewModel>();
                builder.Services.AddSingleton<DocumentManager>();

                builder.Services.AddSingleton<CodeEditConfigService>();
                builder.Services.AddSingleton<ICodeEditConfigInterface>(sp => sp.GetRequiredService<CodeEditConfigService>());

                builder.Services.AddSingleton<SpecialCharViewModel>();
                builder.Services.AddSingleton<SpecialCharacter>();

                builder.Services.AddSingleton<MainViewModel>();

                builder.Services.AddTransient<LinkMarkerControl>();
                builder.Services.AddTransient<CardFilterText>();
                builder.Services.AddTransient<CardFilterData>();
                builder.Services.AddTransient<CardInformation>();
                builder.Services.AddTransient<CardFilter>();
                builder.Services.AddTransient<ResultView>();
                builder.Services.AddTransient<MainUserControl>();

                builder.Services.AddSingleton<IViewLocator, ViewLocator>();

                if (isStandalone) builder.Services.AddSingleton<MainWindowStandalone>();
                else builder.Services.AddSingleton<MainWindowEmbedded>();


                #region Window
                builder.Services.AddTransient<SettingViewModel>();
                builder.Services.AddTransient<ConfigEditor>();
                builder.Services.AddTransient<AboutViewModel>();
                builder.Services.AddTransient<About>();

                DialogService.Register<SettingViewModel, ConfigEditor>();
                DialogService.Register<AboutViewModel, About>();
                #endregion

                _host = builder.Build();
                await _host.StartAsync();
                base.OnStartup(e);

                CMSG.Initialize(Services.GetRequiredService<UIConfigState>());



                var initializer = Services.GetRequiredService<IApplicationInitializer>();
                await initializer.InitializeAsync(e.Args);

                var uiConfigService = Services.GetRequiredService<IUIConfigInterface>();

                var filterConfigService = Services.GetRequiredService<IFilterConfigInterface>();
                var (success, message) = await uiConfigService.LoadAsync();
                var (filterSuccess, filterMessage) = await filterConfigService.LoadAsync();
                if (!success) MessageBox.Show($"Load config failed: {message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                Resources["Lang"] = Services.GetRequiredService<LocalizationProxy>();

                Window mainWindow = isStandalone
                    ? _host.Services.GetRequiredService<MainWindowStandalone>()
                    : _host.Services.GetRequiredService<MainWindowEmbedded>();

                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Startup failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host is not null)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
                _host.Dispose();
            }
            base.OnExit(e);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }
    }
}
