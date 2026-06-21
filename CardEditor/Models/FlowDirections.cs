using System.ComponentModel;

namespace CardEditor.Models
{
    public class FlowDirectionItem : INotifyPropertyChanged
    {
        public FlowDirection Direction;
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
