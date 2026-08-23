using System;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using CardEditor.Commands;
using CardEditor.Services;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.ViewModels
{
    public class ScriptDescriptionViewModel : INotifyPropertyChanged, IDisposable
    {
        private TextDocument _document;
        public TextDocument Document
        {
            get => _document;
            private set
            {
                if (_document == value) return;
                _document = value;
                OnPropertyChanged();
            }
        }

        public ICommand LinkClickedCommand { get; }
        public ScriptDescriptionViewModel()
        {
            LinkClickedCommand = new RelayCommand(url => OpenLink(url as string));
        }

        public void SetDocument(TextDocument document)
        {
            Document = document;
        }

        public void Clear()
        {
            Document = null;
        }
        public void Dispose()
        {

        }

        private static void OpenLink(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl)) return;

            string url = rawUrl.Trim();

            if (url.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("/"))
            {
                url = "https://github.com/ProjectIgnis/scrapiyard/blob/master"
                      + url + ".yml";
            }
            else if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                     (uri.Scheme != Uri.UriSchemeHttp &&
                      uri.Scheme != Uri.UriSchemeHttps))
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                   $"{CMess.errorOcc.ToText()}: Unsupported link type.", new[] { CMess.ok.ToText() });
                return;
            }

            var (success, errorMessage) = BrowserURL.NavigateBrowser(url);

            if (!success)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    errorMessage, new[] { CMess.ok.ToText() });
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
