using System.IO;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Localization;

namespace CardEditor.Models
{
    public class FileItem : INotifyPropertyChanged
    {
        public string FullPath { get; }          // physical path (Temp nếu từ archive)
        public string ArchiveFilePath { get; }    // null nếu là file độc lập
        public string ArchiveEntryName { get; }   // FullName của entry trong archive, null nếu là file độc lập

        public bool IsArchiveEntry => !string.IsNullOrEmpty(ArchiveFilePath);

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
        private string _primaryText = string.Empty;
        public string PrimaryText
        {
            get => _primaryText;
            set
            {
                if (_primaryText != value)
                {
                    _primaryText = value;
                    OnPropertyChanged(nameof(PrimaryText));
                }
            }
        }
        private string _secondaryText = string.Empty;
        public string SecondaryText
        {
            get => _secondaryText;
            set
            {
                if (_secondaryText != value)
                {
                    _secondaryText = value;
                    OnPropertyChanged(nameof(SecondaryText));
                }
            }
        }
        private string _tooltipText = string.Empty;
        public string TooltipText
        {
            get => _tooltipText;
            set
            {
                if (_tooltipText != value)
                {
                    _tooltipText = value;
                    OnPropertyChanged(nameof(TooltipText));
                }
            }
        }

        public FileItem(string fullPath, string archiveFilePath = null, string archiveEntryName = null)
        {
            FullPath = fullPath;
            ArchiveFilePath = archiveFilePath;
            ArchiveEntryName = archiveEntryName;
            Build();
        }

        private void Build()
        {
            if (IsArchiveEntry)
            {
                PrimaryText = string.IsNullOrEmpty(ArchiveEntryName)
                    ? Path.GetFileName(FullPath)
                    : ArchiveEntryName.Replace('/', Path.DirectorySeparatorChar);
                SecondaryText = ArchiveFilePath;
                TooltipText = $"{ArchiveFilePath}  ›  {ArchiveEntryName}";
                DisplayName = PrimaryText;
                return;
            }

            if (string.IsNullOrWhiteSpace(FullPath) || !File.Exists(FullPath))
            {
                PrimaryText = CardEditor.Localization.Language.fileNotExit.ToText();
                SecondaryText = FullPath ?? string.Empty;
                TooltipText = SecondaryText;
                DisplayName = PrimaryText;
                return;
            }

            PrimaryText = Path.GetFileName(FullPath);
            SecondaryText = Path.GetDirectoryName(FullPath) ?? string.Empty;
            TooltipText = FullPath;
            DisplayName = PrimaryText;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class ArchiveFileList
    {
        public List<string> DatabaseEntries { get; } = new();
        public List<string> ScriptEntries { get; } = new();
        public List<string> DeckEntries { get; } = new();
        public List<string> BanlistEntries { get; } = new();
    }

    public class ExtractedEntry
    {
        public string EntryFullName { get; set; }
        public string TempPath { get; set; }
    }
}
