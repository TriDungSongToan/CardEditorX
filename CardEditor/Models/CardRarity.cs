using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace CardEditor.Models
{
    public class RareItem : INotifyPropertyChanged
    {
        private int _idRare;
        public int IdRare
        {
            get => _idRare;
            set
            {
                if (_idRare != value)
                {
                    _idRare = value;
                    OnPropertyChanged(nameof(IdRare));
                }
            }
        }
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        private long _code;
        public long Code
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged(nameof(Code));
                    OnPropertyChanged(nameof(DisplayCode));
                }
            }
        }
        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                if (_imagePath != value)
                {
                    _imagePath = value;
                    OnPropertyChanged(nameof(ImagePath));
                }
            }
        }
        public string DisplayCode
        {
            get
            {
                int bit = GetBitIndex(Code);
                return bit < 0
                ? $"Invalid ({Code})"
                : $"Bit {bit} (0x{Code:X})";
            }
        }
        public bool IsEnabled { get; set; } = true;
        public override string ToString() => Name;
        private static int GetBitIndex(long value)
        {
            if (value <= 0 || (value & (value - 1)) != 0)
                return -1;

            int index = 0;

            while (value > 1)
            {
                value >>= 1;
                index++;
            }

            return index;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
    public class RareCard : INotifyPropertyChanged
    {
        private ulong _id;
        public ulong id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }
        private string _name;
        public string name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }
        private long _rare;
        public long rare
        {
            get => _rare;
            set { _rare = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
