using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Enums;
using CardEditor.Collections;

namespace CardEditor.Models
{
    // Mỗi file translation.conf sẽ tạo một Object TranslationConf
    public class TranslationConf
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName => System.IO.Path.GetFileName(FilePath);
        // Một file translation.conf gồm nhiều Section
        public BulkObservableCollection<TranslationSection> Sections { get; set; } = new();
        public List<string> Warnings { get; } = new();
    }
    // Mỗi một Section trong file là một Object TranslationSection
    public class TranslationSection
    {
        public string SectionName { get; set; } = string.Empty;
        // Một Section gồm nhiều Line
        public BulkObservableCollection<TranslationSectionContent> Content { get; set; } = new();
    }
    // Nội dung của một Line trong Section
    public class TranslationSectionContent
    {
        /// <summary>
        /// Mỗi line có cấu trúc: [SourceContent]->[TranslationContent]
        /// </summary>
        public string SourceContent { get; set; } = string.Empty;
        public string TranslationContent { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sau khi đọc 2 file translation.conf
    /// - File EN-US mới
    /// - File VI-VN cũ
    /// Sẽ tạo được 2 Object TranslationConf
    /// Tiến hành gộp 2 file thành 1 Object TranslationCombine để so sánh
    /// </summary>

    public class TranslationCombine : INotifyPropertyChanged
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
        public BulkObservableCollection<TranslationSectionCombine> Sections { get; set; } = new();
        public int RemovedCount { get; set; }
        public List<string> RemovedKeys { get; } = new List<string>();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public class TranslationSectionCombine : INotifyPropertyChanged
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
        public BulkObservableCollection<TranslationSectionContentCombine> Content { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class TranslationSectionContentCombine : INotifyPropertyChanged
    {
        public bool IsInitialized { get; set; }
        public string Source { get; set; } = string.Empty; // Vế trái của file EN-US mới
        public string SourceContent { get; set; } = string.Empty; // Vế phải của file EN-US mới
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

        //public List<string> RemovedKeys { get; } = new List<string>();
        //public int RemovedCount => RemovedKeys.Count;

        private StatusConf _status; // Trạng thái dịch
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
    public class TranslationRowsDragData
    {
        public TranslationSectionCombine From { get; set; }
        public List<TranslationSectionContentCombine> Items { get; set; }
    }

}
