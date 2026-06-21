using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CardEditor.Models
{
    public class RareItem : INotifyPropertyChanged
    {
        private int _idRare;
        public int IdRare
        {
            get => _idRare;
            set { _idRare = value; OnPropertyChanged(); }
        }
        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }
        private long _code;
        public long Code
        {
            get => _code;
            set { _code = value; OnPropertyChanged(); }
        }
        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set { _imagePath = value; OnPropertyChanged(); }
        }
        public string DisplayCode
        {
            get
            {
                if (Code <= 0 || (Code & (Code - 1)) != 0)
                {
                    return $"Invalid ({Code})";
                }
                int bit = (int)(Math.Log(Code) / Math.Log(2));
                return $"Bit {bit} (0x{Code:X})";
            }
        }
        public bool IsEnabled { get; set; } = true;
        public override string ToString() => Name;

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
