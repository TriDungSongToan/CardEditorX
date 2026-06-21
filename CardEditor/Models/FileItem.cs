using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using CardEditor.Helpers;
using CardEditor.Localization;
using System.Web.Routing;

namespace CardEditor.Models
{
    public class FileItem : INotifyPropertyChanged
    {
        private readonly string _rootFolder1;
        private readonly string _rootFolder2;
        private string _fullPath;
        public string FullPath
        {
            get => _fullPath;
            set
            {
                if (_fullPath != value)
                {
                    _fullPath = value;
                    OnPropertyChanged(nameof(FullPath));
                    UpdateDisplayName();
                }
            }
        }
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            private set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public FileItem(string FullPath, string RootFolder1, string RootFolder2 = null)
        {
            _rootFolder1 = NormalizeRoot(RootFolder1);
            _rootFolder2 = string.IsNullOrWhiteSpace(RootFolder2) ? null : NormalizeRoot(RootFolder2);
            _fullPath = FullPath;
        }
        private void UpdateDisplayName()
        {
            if (string.IsNullOrWhiteSpace(FullPath))
            {
                DisplayName = CardEditor.Localization.Language.allfile.ToText();
                return;
            }
            if (!File.Exists(FullPath))
            {
                DisplayName = CardEditor.Localization.Language.fileNotExit.ToText();
                return;
            }

            string normalizedFull = Path.GetFullPath(FullPath);
            if (normalizedFull.StartsWith(_rootFolder1, StringComparison.OrdinalIgnoreCase))
            {
                DisplayName = FilePathHelper.GetRelativePath(_rootFolder1, normalizedFull);
                return;
            }
            if (_rootFolder2 != null && normalizedFull.StartsWith(_rootFolder2, StringComparison.OrdinalIgnoreCase))
            {
                DisplayName = FilePathHelper.GetRelativePath(_rootFolder2, normalizedFull);
                return;
            }
            DisplayName = CardEditor.Localization.Language.fileNotExit.ToText();
        }
        private static string NormalizeRoot(string root)
        {
            return Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
