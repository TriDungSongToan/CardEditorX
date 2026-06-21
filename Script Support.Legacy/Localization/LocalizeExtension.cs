using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace ScriptSupport.Legacy.Localization
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
}
