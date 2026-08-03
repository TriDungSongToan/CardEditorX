using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.ComponentModel;

namespace CardEditor.Localization
{
    public class LocalizeExtension : MarkupExtension
    {
        private readonly Language _key;
        public LocalizeExtension()
        {
            _key = new Language();
        }
        public LocalizeExtension(Language key)
        {
            _key = key;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding(nameof(LocalizeBindingSource.Value))
            {
                Source = new LocalizeBindingSource(_key),
                Mode = BindingMode.OneWay
            };

            return binding.ProvideValue(serviceProvider);
        }

        public class LocalizeBindingSource : INotifyPropertyChanged
        {
            private readonly Language _key;

            public LocalizeBindingSource(Language key)
            {
                _key = key;
                WeakEventManager<LanguageManager, EventArgs>
                    .AddHandler(LanguageManager.Instance, nameof(LanguageManager.LanguageChanged), OnLanguageChanged);
            }

            public string Value => LanguageManager.Instance.GetTranslation(_key);

            private void OnLanguageChanged(object sender, EventArgs e)
            {
                PropertyChanged?.Invoke(this,
                    new PropertyChangedEventArgs(nameof(Value)));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }

    }
    public static class LanguageExtensions
    {
        // Phương thức mở rộng thông thường thay vì toán tử chuyển đổi
        public static string ToText(this Language language)
        {
            return LanguageManager.GetText((uint)language);
        }
    }
    public static class UIntExtensions
    {
        public static string ToLanguageString(this uint code)
        {
            return LanguageManager.GetText(code);
        }
    }
}
