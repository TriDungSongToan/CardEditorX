using System.ComponentModel;
using CardEditor.Enums;

namespace CardEditor.Models
{
    public class TextAlignmentItem : INotifyPropertyChanged
    {
        public TextAlignment Alignment;
        private string _displayName { get; set; }
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
