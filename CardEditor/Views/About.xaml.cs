using System;
using System.Windows;
using System.Windows.Input;
using System.Runtime.Versioning;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Reflection;
using System.Configuration;
using System.ComponentModel;
using CardEditor.Helpers;
using CardEditor.Commands;
using CardEditor.Services;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window, INotifyPropertyChanged
    {
        #region Properties
        private string _appName = "CardEditorX";
        public string AppName
        {
            get => _appName;
            set
            {
                if (_appName != value)
                {
                    _appName = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _version = "1.0.0";
        public string Version
        {
            get => _version;
            set
            {
                if (_version != value)
                {
                    _version = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _author = "Trí Dũng Song Toàn";
        public string Author
        {
            get => _author;
            set
            {
                if (_author != value)
                {
                    _author = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _license = "Non-commercial License";
        public string License
        {
            get => _license;
            set
            {
                if (_license != value)
                {
                    _license = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _copyright = "Copyright © 2026 Trí Dũng Song Toàn. All rights reserved.";
        public string Copyright
        {
            get => _copyright;
            set
            {
                if (_copyright != value)
                {
                    _copyright = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _netVersionBuilded = string.Empty;
        public string NetVersionBuild
        {
            get => _netVersionBuilded;
            set
            {
                if (_netVersionBuilded != value)
                {
                    _netVersionBuilded = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _netRunTime = string.Empty;
        public string NetRunTime
        {
            get => _netRunTime;
            set
            {
                if (_netRunTime != value)
                {
                    _netRunTime = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _processor = string.Empty;
        public string Processor
        {
            get => _processor;
            set
            {
                if (_processor != value)
                {
                    _processor = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _memory = string.Empty;
        public string Memory
        {
            get => _memory;
            set
            {
                if (_memory != value)
                {
                    _memory = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _logicalProcessorCount = string.Empty;
        public string LogicalProcessorCount
        {
            get => _logicalProcessorCount;
            set
            {
                if (_logicalProcessorCount != value)
                {
                    _logicalProcessorCount = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _operatingSystem = string.Empty;
        public string OperatingSystem
        {
            get => _operatingSystem;
            set
            {
                if (_operatingSystem != value)
                {
                    _operatingSystem = value;
                    OnPropertyChanged();
                }
            }
        }
        private DateTime _releaseDate = new DateTime();
        public DateTime ReleaseDate
        {
            get => _releaseDate;
            set
            {
                if (_releaseDate != value)
                {
                    _releaseDate = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Commands
        public RelayCommand CreatorWebCommand { get; set; }
        public RelayCommand GithubCommand { get; set; }
        public RelayCommand CopyInfoCommand { get; set; }
        public RelayCommand OKCommand { get; set; }
        #endregion

        #region Constructor
        public About()
        {
            InitializeComponent();
            InitializeCommand();
            this.DataContext = this;

            LoadAbout();
        }
        private void InitializeCommand()
        {
            CreatorWebCommand = new RelayCommand(_ => CreatorWeb());
            GithubCommand = new RelayCommand(_ => Github());
            CopyInfoCommand = new RelayCommand(async _ => await CopyInfo());
            OKCommand = new RelayCommand(_ => OK());
        }
        private void LoadAbout()
        {
            AppName = "CardEditorX";
            Version = "1.0.0";
            Author = "Trí Dũng Song Toàn";
            License = "Non-commercial License";
            Copyright = "Copyright © 2026 Trí Dũng Song Toàn. All rights reserved. Licensed for personal and non-commercial use only.";
            var framework = Assembly.GetExecutingAssembly().GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
            NetVersionBuild = framework?.Replace(".NETCoreApp,Version=v", ".NET ").Replace(".NETFramework,Version=v", ".NET Framework ") ?? "Unknown";
            NetRunTime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
            OperatingSystem = SystemInfoHelper.OperatingSystem;
            Processor = SystemInfoHelper.Processor;
            LogicalProcessorCount = SystemInfoHelper.LogicalProcessorCount.ToString();
            Memory = SystemInfoHelper.Memory;
            ReleaseDate = BuildInfoHelper.ReleaseDateLocal;
        }
        #endregion

        #region Command Methods
        private void CreatorWeb()
        {
            string URL = ConfigurationManager.AppSettings["CreatorURL"];

            var (Success, ErrorMessage) = BrowserURL.NavigateBrowser(URL);
            if (!Success) CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, ErrorMessage, new[] { CMess.ok.ToText() });
        }
        private void Github()
        {
            string URL = ConfigurationManager.AppSettings["CardEditorXURL"];

            var (Success, ErrorMessage) = BrowserURL.NavigateBrowser(URL);
            if (!Success) CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, ErrorMessage, new[] { CMess.ok.ToText() });
        }

        private async Task CopyInfo()
        {
            string text =
                $"{AppName}\n" +
                $"Version: {Version}\n" +
                $"License: {License}\n" +
                $"Target Framework: {NetVersionBuild}\n" +
                $"Release Date: {ReleaseDate:yyyy-MM-dd} (yyyy-MM-dd)\n" +
                $"Runtime Version: {NetRunTime}\n" +
                $"Operating System: {OperatingSystem}\n" +
                $"Processor: {Processor}\n" +
                $"Logical Processors: {LogicalProcessorCount}\n" +
                $"Memory: {Memory}";

            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception ex) 
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private void OK()
        {
            this.Close();
        }

        #endregion

        #region Event
        private void blsetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
