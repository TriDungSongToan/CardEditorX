using System.ComponentModel;

namespace CardEditor.Models
{
    public class SortItem : INotifyPropertyChanged
    {
        public SortType Sort { get; set; }
        public string Name { get; set; }
        private string _displayName = string.Empty;
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

    public class SelectedSortItem : INotifyPropertyChanged
    {
        private SortItem _selectedItem;
        public SortItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }
        private bool _orderByAsc;
        public bool OrderByAsc
        {
            get => _orderByAsc;
            set
            {
                if (_orderByAsc != value)
                {
                    _orderByAsc = value;
                    OnPropertyChanged(nameof(OrderByAsc));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class SortItemData
    {
        public int Sort { get; set; }
        public bool OrderByAsc { get; set; }
    }
}
