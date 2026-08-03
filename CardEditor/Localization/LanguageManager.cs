using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using CardEditor.ViewModels;

namespace CardEditor.Localization
{
    public class LanguageManager
    {
        private static LanguageManager _instance;
        private Dictionary<uint, string> _translations = new Dictionary<uint, string>();
        public event EventHandler LanguageChanged;

        public static LanguageManager Instance
        {
            get
            {
                if (_instance == null) _instance = new LanguageManager();
                return _instance;
            }
        }
        public void LoadLanguage(string languageCode)
        {
            try
            {
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string filePath = Path.Combine(exeDir, $@"data\CardData\Language\{languageCode}\AppLanguage.txt");

                if (File.Exists(filePath))
                {
                    LoadFromFile(filePath);
                }
                else
                {
                    using var stream = GetEmbeddedResourceStream("CardEditor.Localization.AppLanguage.txt");
                    if (stream != null)
                    {
                        MessageBox.Show("Language file not found. Using default language file.\nYou may need to check for updates and restart application.",
                            "Language File Fallback", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadFromStream(stream);
                    }
                    else
                    {
                        MessageBox.Show("No language files found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                OnLanguageChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void Initialize()
        {
            try
            {
                string languageCode = ConfigViewModel.Instance.userSetting.Language;
                Instance.LoadLanguage(languageCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading language file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadFromFile(string filePath)
        {
            _translations.Clear();
            foreach (var line in File.ReadLines(filePath))
                ParseLine(line);
        }
        private void LoadFromStream(Stream stream)
        {
            _translations.Clear();
            using var reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
                ParseLine(line);
        }
        private void ParseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            var parts = line.Split(new[] { '\t' }, 2);
            if (parts.Length != 2) return;

            string codeStr = parts[0].Trim();
            if (codeStr.StartsWith("0x") &&
                uint.TryParse(codeStr.Substring(2), NumberStyles.HexNumber, null, out uint code))
            {
                _translations[code] = parts[1].Trim();
            }
        }

        public string GetTranslation(Language key) =>
            _translations.TryGetValue((uint)key, out var v) ? v : $"[{key}]";
        public string GetTranslation(uint key) =>
            _translations.TryGetValue(key, out var v) ? v : $"[0x{key:X}]";
        public static string GetText(uint code) =>
            Instance._translations.TryGetValue(code, out var v) ? v : $"[Unknown:{code:X}]";
        public static string GetText(Language language) => GetText((uint)language);


        private void RefreshUI()
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                foreach (Window window in Application.Current.Windows)
                {
                    RefreshControls(window);
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(RefreshUI);
            }
        }
        private void RefreshControls(DependencyObject parent)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is MenuItem menuItem)
                {
                    // Cập nhật binding của MenuItem nếu có
                    BindingExpression binding = menuItem.GetBindingExpression(MenuItem.HeaderProperty);
                    binding?.UpdateTarget();
                }
                else if (child is TextBlock textBlock)
                {
                    // Cập nhật binding của TextBlock nếu có
                    BindingExpression binding = textBlock.GetBindingExpression(TextBlock.TextProperty);
                    binding?.UpdateTarget();
                }
                else if (child is Button button)
                {
                    // Cập nhật binding của Button nếu có
                    BindingExpression binding = button.GetBindingExpression(Button.ContentProperty);
                    binding?.UpdateTarget();
                }

                // Tiếp tục tìm kiếm các control con
                RefreshControls(child);
            }
        }
        protected virtual void OnLanguageChanged()
        {
            LanguageChanged?.Invoke(this, EventArgs.Empty);
            RefreshUI();
        }

        public static void ValidateLocalizationEnum()
        {
            var enumType = typeof(Language);

            var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

            var duplicates = fields.Select(f => new { Name = f.Name, Value = (uint)f.GetValue(null) })
                .GroupBy(x => x.Value).Where(g => g.Count() > 1).ToList();

            if (duplicates.Any())
            {
                var message = "Duplicate Language enum values detected:\n\n";

                foreach (var group in duplicates)
                {
                    message += $"Value 0x{group.Key:X} used by:\n";
                    foreach (var item in group)
                        message += $"   - {item.Name}\n";
                    message += "\n";
                }
                MessageBox.Show(message, "Enum Duplicate Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private static Stream GetEmbeddedResourceStream(string resourceName) =>
            Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
    }
}
