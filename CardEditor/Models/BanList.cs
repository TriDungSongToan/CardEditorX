using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Services;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Models
{
    public class CardBanList
    {
        public ulong Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public int LimitedCount { get; set; } = 3;

        public void UpdateFrom(CardBanList src, params Action<CardBanList, CardBanList>[] updaters)
        {
            if (src == null || updaters == null) return;

            foreach (var u in updaters)
                u(this, src);
        }
        public void UpdateFrom(CardBanList src, IEnumerable<Action<CardBanList, CardBanList>> updaters)
        {
            if (src == null || updaters == null) return;
            foreach (var u in updaters)
                u(this, src);
        }
    }
    public class BanList : INotifyPropertyChanged
    {
        private string _name = string.Empty;
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
        public string FileName { get; set; } = string.Empty; // File Name With Extension 
        public string FilePath { get; set; } = string.Empty; // Absolute File Path
        public string archiveFilePath;
        public string archiveEntryName;

        public Dictionary<ulong, CardBanList> CardList { get; set; }
        public bool WhiteList { get; set; } = false;
        public Guid ID { get; set; } = Guid.NewGuid();
        public override string ToString()
        {
            return Name;
        }

        public async Task<(bool, string)> SaveToArchive(string targetSourcePath)
        {
            if (string.IsNullOrEmpty(archiveFilePath) || !System.IO.File.Exists(archiveFilePath) || string.IsNullOrEmpty(archiveEntryName))
                return (true, string.Empty);

            if (string.IsNullOrEmpty(targetSourcePath) || !System.IO.File.Exists(targetSourcePath))
                return (false, CMess.fileNotExit.ToText());

            return await LoadDataServices.SaveEntryToZip(archiveFilePath, archiveEntryName, targetSourcePath);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
