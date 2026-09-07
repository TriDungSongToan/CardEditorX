using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MaterialDesignThemes.Wpf;
using CardEditor.Enums;
using CardEditor.Commands;
using CardEditor.Services;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for CustomButtonMessageBox.xaml
    /// </summary>
    public class ButtonInfo : INotifyPropertyChanged
    {
        private string _text;
        private bool _isDefault;
        private int _index;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
        public bool IsDefault
        {
            get => _isDefault;
            set
            {
                _isDefault = value;
                OnPropertyChanged(nameof(IsDefault));
            }
        }
        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged(nameof(Index));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public partial class CMSG : Window, INotifyPropertyChanged
    {
        #region Variable
        private int? _resultIndex;
        public enum MessageBoxIconType
        {
            Error,
            Warning,
            Notification,
            Information,
            Question
        }
        #endregion

        #region Property
        private string _titles;
        public string Titles
        {
            get => _titles;
            set
            {
                _titles = value;
                OnPropertyChanged(nameof(Titles));
            }
        }
        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }
        private Visibility _iconVisibility = Visibility.Collapsed;
        public Visibility IconVisibility
        {
            get => _iconVisibility;
            set
            {
                _iconVisibility = value;
                OnPropertyChanged(nameof(IconVisibility));
            }
        }

        private ObservableCollection<ButtonInfo> _buttons;
        public ObservableCollection<ButtonInfo> Buttons
        {
            get => _buttons;
            set
            {
                _buttons = value;
                OnPropertyChanged(nameof(Buttons));
            }
        }

        private MaterialDesignThemes.Wpf.PackIconKind _copyLogIcon = PackIconKind.ContentCopy;
        public MaterialDesignThemes.Wpf.PackIconKind CopyLogIcon
        {
            get => _copyLogIcon;
            set
            {
                if (_copyLogIcon != value)
                {
                    _copyLogIcon = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Commnad
        public RelayCommand CopyLogCommand { get; set; }
        #endregion

        #region Constructor
        public CMSG()
        {
            InitializeComponent();
            InitializeCommand();
            DataContext = this;
        }
        private void InitializeCommand()
        {
            CopyLogCommand = new RelayCommand(async _ => await CopyLogExecute());
        }
        #endregion

        #region Command Methods
        private async Task CopyLogExecute()
        {
            bool success = CopyLog();
            if (!success) return;

            CopyLogIcon = PackIconKind.Check;

            await Task.Delay(3000);
            CopyLogIcon = PackIconKind.ContentCopy;
        }
        private bool CopyLog()
        {
            try
            {
                string log = $"{Title}\n{Message}";
                Clipboard.SetText(log);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        public static int Show(string title, MessageBoxIconType iconType, string message, string[] buttons, int defaultButtonIndex = 0)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                return Application.Current.Dispatcher.Invoke(() =>
                Show(title, iconType, message, buttons, defaultButtonIndex));
            }

            // Validate defaultButtonIndex
            if (defaultButtonIndex < 0 || defaultButtonIndex >= buttons.Length)
            {
                defaultButtonIndex = 0;
            }

            var msgBox = new CMSG
            {
                Title = title,
                Message = message,
                Buttons = new ObservableCollection<ButtonInfo>(
                    buttons.Select((text, index) => new ButtonInfo
                    {
                        Text = text,
                        IsDefault = index == defaultButtonIndex,
                        Index = index
                    })
                )
            };

            // Set icon based on type
            msgBox.IconVisibility = Visibility.Visible;
            AppImage appImage = iconType switch
            {
                MessageBoxIconType.Error => AppImage.Error,
                MessageBoxIconType.Warning => AppImage.Warning,
                MessageBoxIconType.Notification => AppImage.Notification,
                MessageBoxIconType.Information => AppImage.Information,
                MessageBoxIconType.Question => AppImage.Question,
                _ => AppImage.Information
            };
            msgBox.MessageIcon.Source = ImageCacheService.Instance.Get(appImage);

            msgBox.ShowDialog();
            return msgBox._resultIndex ?? -1;
        }

        #region Event
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var defaultButton = Buttons.FirstOrDefault(b => b.IsDefault);
                if (defaultButton != null)
                {
                    // _result = defaultButton.Text;
                    _resultIndex = defaultButton.Index;
                    DialogResult = true;
                    Close();
                }
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var buttonInfo = Buttons.FirstOrDefault(b => b.Text == button.Content.ToString());
                if (buttonInfo != null)
                {
                    // _result = buttonInfo.Text;
                    _resultIndex = buttonInfo.Index;
                    DialogResult = true;
                    Close();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        #endregion
    }
}
