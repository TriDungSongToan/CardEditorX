using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Enums;
using CardEditor.Collections;

namespace CardEditor.Models
{
    // Mỗi một file strings.conf sẽ tạo một Object StringConf
    public class StringConf
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName => System.IO.Path.GetFileName(FilePath);
        // Một file strings.conf gồm nhiều Section
        public BulkObservableCollection<StringSection> Sections { get; } = new();
        public List<string> Warnings { get; } = new List<string>();
    }
    // Mỗi một Section trong file là một Object StringSection
    public class StringSection
    {
        public string SectionName { get; set; } = string.Empty;
        // Một Section gồm nhiều Line
        public BulkObservableCollection<StringSectionContent> Content { get; set; } = new();
    }
    // Nội dung của một Line trong Section
    public class StringSectionContent
    {
        /// <summary>
        /// Mỗi line có cấu trúc: ![Token] [ID] [Content]
        /// </summary>
        public string Token { get; set; } = string.Empty;
        public string ID { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsDisabled { get; set; }
        public bool LooseDisabled { get; set; }
    }

    /// <summary>
    /// Sau khi đọc 2 file string.conf
    /// - File EN-US mới
    /// - File VI-VN cũ
    /// Sẽ tạo được 2 Object StringConf
    /// Tiến hành gộp 2 file thành 1 Object StringCombine để so sánh
    /// </summary>

    public class StringCombine : INotifyPropertyChanged
    {
        private string _combineName = string.Empty;
        public string CombineName
        {
            get => _combineName;
            set
            {
                if (_combineName != value)
                {
                    _combineName = value;
                    OnPropertyChanged(nameof(CombineName));
                }
            }
        }
        public BulkObservableCollection<StringSectionCombine> Sections { get; set; } = new();
        public int RemovedCount { get; set; }
        public List<string> RemovedKeys { get; } = new List<string>();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class StringSectionCombine : INotifyPropertyChanged
    {
        private string _sectionName = string.Empty;
        public string SectionName
        {
            get => _sectionName;
            set
            {
                if (_sectionName != value)
                {
                    _sectionName = value;
                    OnPropertyChanged(nameof(SectionName));
                }
            }
        }
        public BulkObservableCollection<StringSectionContentCombine> Content { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class StringSectionContentCombine : INotifyPropertyChanged
    {
        public bool IsInitialized { get; set; }
        public string Token { get; set; } = string.Empty;
        public string ID { get; set; } = string.Empty; // Phần ID của file EN-US mới
        public long IdValue { get; set; }
        public string SourceContent { get; set; } = string.Empty; // Phần nội dung của file EN-US mới
        public string OldSource { get; set; } = null;
        private string _translationContent = string.Empty;
        public string TranslationContent
        {
            get => _translationContent;
            set
            {
                if (_translationContent == value) return;
                _translationContent = value;
                OnPropertyChanged(nameof(TranslationContent));

                if (IsInitialized)
                    Status = value == SourceContent ? StatusConf.Untranslated : StatusConf.Translated;
            }
        }

        private bool _isDisabled;
        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                if (_isDisabled != value)
                {
                    _isDisabled = value;
                    OnPropertyChanged(nameof(IsDisabled));
                }
            }
        }

        //public List<string> RemovedKeys { get; } = new List<string>();
        //public int RemovedCount => RemovedKeys.Count;

        private StatusConf _status;
        public StatusConf Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class StringRowsDragData
    {
        public StringSectionCombine From { get; set; }
        public List<StringSectionContentCombine> Items { get; set; }
    }

}
