using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CardEditor.Models
{
    public class GenesysCard : INotifyPropertyChanged
    {
        private ulong _id;
        public ulong Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }
        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }
        private int _gPoints;
        public int GPoints
        {
            get => _gPoints;
            set { _gPoints = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
