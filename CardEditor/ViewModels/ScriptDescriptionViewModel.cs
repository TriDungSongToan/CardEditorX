using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ICSharpCode.AvalonEdit.Document;

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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
