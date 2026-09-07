using System.Runtime.CompilerServices;
using System.ComponentModel;
using CardEditor.Enums;

namespace CardEditor.Models.Settings
{
    public class FilterSetting : INotifyPropertyChanged
    {
        private bool _advanced = false;
        public bool Advanced
        {
            get => _advanced;
            set
            {
                if (_advanced != value)
                {
                    _advanced = value;
                    OnPropertyChanged(nameof(Advanced));
                }
            }
        }
        private bool _matchCase = false;
        public bool MatchCase
        {
            get => _matchCase;
            set
            {
                if (_matchCase != value)
                {
                    _matchCase = value;
                    OnPropertyChanged(nameof(MatchCase));
                }
            }
        }
        private bool _wildcards = false;
        public bool Wildcards
        {
            get => _wildcards;
            set
            {
                if (_wildcards != value)
                {
                    _wildcards = value;
                    OnPropertyChanged(nameof(Wildcards));
                }
            }
        }
        private bool _prefix = false;
        public bool Prefix
        {
            get => _prefix;
            set
            {
                if (_prefix != value)
                {
                    _prefix = value;
                    OnPropertyChanged(nameof(Prefix));
                }
            }
        }
        private bool _suffix = false;
        public bool Suffix
        {
            get => _suffix;
            set
            {
                if (_suffix != value)
                {
                    _suffix = value;
                    OnPropertyChanged(nameof(Suffix));
                }
            }
        }
        private bool _matchWhole = false;
        public bool MatchWhole
        {
            get => _matchWhole;
            set
            {
                if (_matchWhole != value)
                {
                    _matchWhole = value;
                    OnPropertyChanged(nameof(MatchWhole));
                }
            }
        }
        private bool _ignpunct = false;
        public bool Ignpunct
        {
            get => _ignpunct;
            set
            {
                if (_ignpunct != value)
                {
                    _ignpunct = value;
                    OnPropertyChanged(nameof(Ignpunct));
                }
            }
        }
        private bool _ignspace = false;
        public bool Ignpspace
        {
            get => _ignspace;
            set
            {
                if (_ignspace != value)
                {
                    _ignspace = value;
                    OnPropertyChanged(nameof(Ignpspace));
                }
            }
        }

        public int GetValue()
        {
            FilterOption value = FilterOption.None;

            if (Advanced)   value |= FilterOption.Advanced;
            if (MatchCase)  value |= FilterOption.MatchCase;
            if (Wildcards)  value |= FilterOption.Wildcards;
            if (Prefix)     value |= FilterOption.Prefix;
            if (Suffix)     value |= FilterOption.Suffix;
            if (MatchWhole) value |= FilterOption.MatchWhole;
            if (Ignpunct)   value |= FilterOption.IgnPunct;
            if (Ignpspace)  value |= FilterOption.IgnSpace;

            return (int)value;
        }
        public void SetValue(int advanced)
        {
            var value = (FilterOption)advanced;

            Advanced    = value.HasFlag(FilterOption.Advanced);
            MatchCase   = value.HasFlag(FilterOption.MatchCase);
            Wildcards   = value.HasFlag(FilterOption.Wildcards);
            Prefix      = value.HasFlag(FilterOption.Prefix);
            Suffix      = value.HasFlag(FilterOption.Suffix);
            MatchWhole  = value.HasFlag(FilterOption.MatchWhole);
            Ignpunct    = value.HasFlag(FilterOption.IgnPunct);
            Ignpspace   = value.HasFlag(FilterOption.IgnSpace);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
