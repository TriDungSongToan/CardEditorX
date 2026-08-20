using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for PasswordRevealBox.xaml
    /// </summary>
    public partial class PasswordRevealBox : UserControl
    {
        private bool _isUpdatingPassword = false;
        public PasswordRevealBox()
        {
            InitializeComponent();

            PART_PasswordBox.PasswordChanged += PART_PasswordBox_PasswordChanged;
            PART_TextBox.TextChanged += PART_TextBox_TextChanged;
            PART_ToggleButton.Click += PART_ToggleButton_Click;

            UpdateVisibilityState();

            this.Loaded += PasswordRevealBox_Loaded;
        }
        private void PasswordRevealBox_Loaded(object sender, RoutedEventArgs e)
        {
            _isUpdatingPassword = true;
            if (IsPasswordVisible)
            {
                PART_TextBox.Text = Password;
            }
            else
            {
                PART_PasswordBox.Password = Password;
            }
            _isUpdatingPassword = false;

            UpdateVisibilityState();
        }

        #region Password
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(nameof(Password), typeof(string),
            typeof(PasswordRevealBox), new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));
        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }
        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PasswordRevealBox)d;
            if (control._isUpdatingPassword) return;

            string newPassword = e.NewValue as string ?? string.Empty;

            control._isUpdatingPassword = true;
            if (control.PART_PasswordBox.Password != newPassword)
                control.PART_PasswordBox.Password = newPassword;

            if (control.PART_TextBox.Text != newPassword)
                control.PART_TextBox.Text = newPassword;
            control._isUpdatingPassword = false;
        }
        private void PART_PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingPassword) return;

            _isUpdatingPassword = true;
            Password = PART_PasswordBox.Password;
            if (PART_TextBox.Text != PART_PasswordBox.Password)
                PART_TextBox.Text = PART_PasswordBox.Password;
            _isUpdatingPassword = false;
        }
        private void PART_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingPassword) return;

            _isUpdatingPassword = true;
            Password = PART_TextBox.Text;
            if (PART_PasswordBox.Password != PART_TextBox.Text)
                PART_PasswordBox.Password = PART_TextBox.Text;
            _isUpdatingPassword = false;
        }
        #endregion

        #region IsPasswordVisible
        public static readonly DependencyProperty IsPasswordVisibleProperty = DependencyProperty.Register(nameof(IsPasswordVisible),
            typeof(bool), typeof(PasswordRevealBox), new PropertyMetadata(false, OnIsPasswordVisibleChanged));
        public bool IsPasswordVisible
        {
            get => (bool)GetValue(IsPasswordVisibleProperty);
            set => SetValue(IsPasswordVisibleProperty, value);
        }
        private static void OnIsPasswordVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PasswordRevealBox)d;
            bool isVisible = (bool)e.NewValue;

            control.UpdateVisibilityState();

            if (control._isUpdatingPassword) return;

            control._isUpdatingPassword = true;
            if (isVisible)
            {
                control.PART_TextBox.Text = control.PART_PasswordBox.Password;
                control.PART_TextBox.CaretIndex = control.PART_TextBox.Text.Length;
            }
            else
            {
                control.PART_PasswordBox.Password = control.PART_TextBox.Text;
            }
            control._isUpdatingPassword = false;
        }

        private void UpdateVisibilityState()
        {
            PART_TextBox.Visibility = IsPasswordVisible ? Visibility.Visible : Visibility.Collapsed;
            PART_PasswordBox.Visibility = IsPasswordVisible ? Visibility.Collapsed : Visibility.Visible;

            // Đồng bộ ngược lại icon của ToggleButton khi IsPasswordVisible bị set từ bên ngoài (code/binding của host)
            if (PART_ToggleButton.IsChecked != IsPasswordVisible)
                PART_ToggleButton.IsChecked = IsPasswordVisible;
        }
        #endregion

        #region Hint
        public static readonly DependencyProperty HintProperty = DependencyProperty.Register(nameof(Hint), typeof(string),
            typeof(PasswordRevealBox), new PropertyMetadata("Password"));
        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }
        #endregion

        #region Event
        private void PART_ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            IsPasswordVisible = PART_ToggleButton.IsChecked == true;
        }

        public static readonly RoutedEvent EnterPressedEvent = EventManager.RegisterRoutedEvent(nameof(EnterPressed),
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PasswordRevealBox));
        public event RoutedEventHandler EnterPressed
        {
            add => AddHandler(EnterPressedEvent, value);
            remove => RemoveHandler(EnterPressedEvent, value);
        }
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RaiseEvent(new RoutedEventArgs(EnterPressedEvent));
                e.Handled = true;
            }
        }
        #endregion

    }
}
