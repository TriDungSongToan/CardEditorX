using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CardEditor.Collections;

namespace CardEditor.Models
{
    public class Deck : INotifyPropertyChanged
    {
        public static int DeckCounter = 1;
        private string CustomName;

        private string _path;
        public string Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged(nameof(Path));
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public List<CardInstance> MainDeck { get; set; } = new List<CardInstance>();
        public List<CardInstance> ExtraDeck { get; set; } = new List<CardInstance>();
        public List<CardInstance> SideDeck { get; set; } = new List<CardInstance>();

        // File name with extension
        public string Name
        {
            get
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(CustomName)) return CustomName;
                    if (!string.IsNullOrWhiteSpace(Path) && System.IO.File.Exists(Path))
                        return System.IO.Path.GetFileName(Path);
                }
                catch { }
                return $"Deck_{DeckCounter++}.ydk";
            }
            set
            {
                if (CustomName != value)
                {
                    CustomName = value;
                    OnPropertyChanged(nameof(Path));
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
    public enum DeckType
    {
        Main,
        Extra,
        Side
    }
    public class CardDeck : BulkObservableCollection<CardInstance>
    {
        public DeckType Type { get; }

        public CardDeck(DeckType type)
        {
            Type = type;
        }
    }
}
