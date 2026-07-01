using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CardEditor.Models;
using CardEditor.Services;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.ViewModels
{
    public class CardDataViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<CardDataViewModel> _instance = new Lazy<CardDataViewModel>(() => new CardDataViewModel());
        public static CardDataViewModel Instance => _instance.Value;

        private string ExeFilePath => CardEditor.Models.AppContext.Instance.ExeFilePath;
        private string DataFolderPath => CardEditor.Models.AppContext.Instance.DataFolderPath;

        public BulkObservableCollection<RuleItem> RuleItems { get; set; }
        public BulkObservableCollection<TypeItem> TypeItems { get; set; }
        public BulkObservableCollection<RaceItem> RaceItems { get; set; }
        public BulkObservableCollection<CharItem> CharItems { get; set; }
        public BulkObservableCollection<AttributeItem> AttributeItems { get; set; }
        public BulkObservableCollection<SetCodeItem> SetCodeItems { get; set; }
        public BulkObservableCollection<CategoryItem> CategoryItems { get; set; }
        public BulkObservableCollection<FlagItem> FlagItems { get; set; }
        public BulkObservableCollection<LevelItem> LevelItems { get; set; }
        public BulkObservableCollection<LinkArrowItem> LinkArrowItems { get; set; }
        public BulkObservableCollection<CharacterItem> SpecialCharacters { get; set; }
        public List<(ulong bit, string name)> listrule { get; set; }
        public List<(ulong bit, string name)> listtype { get; set; }
        public List<(ulong bit, string name)> listtypedeck { get; set; }
        public List<(ulong bit, string name)> listrace { get; set; }
        public List<(ulong bit, string name)> listchar { get; set; }
        public List<(ulong bit, string name)> listattr { get; set; }
        public List<(ulong bit, string name)> listsetcode { get; set; }
        public List<(ulong bit, string name)> listlinkarrow { get; set; }

        private static readonly object _lockObject = new object();

        private CardDataViewModel()
        {
            RuleItems = new BulkObservableCollection<RuleItem>();
            TypeItems = new BulkObservableCollection<TypeItem>();
            RaceItems = new BulkObservableCollection<RaceItem>();
            CharItems = new BulkObservableCollection<CharItem>();
            AttributeItems = new BulkObservableCollection<AttributeItem>();
            SetCodeItems = new BulkObservableCollection<SetCodeItem>();
            CategoryItems = new BulkObservableCollection<CategoryItem>();
            FlagItems = new BulkObservableCollection<FlagItem>();
            LevelItems = new BulkObservableCollection<LevelItem>();
            LinkArrowItems = new BulkObservableCollection<LinkArrowItem>();
            SpecialCharacters = new BulkObservableCollection<CharacterItem>();
            listrule = new List<(ulong bit, string name)>();
            listtype = new List<(ulong bit, string name)>();
            listtypedeck = new List<(ulong bit, string name)>();
            listrace = new List<(ulong bit, string name)>();
            listchar = new List<(ulong bit, string name)>();
            listattr = new List<(ulong bit, string name)>();
            listsetcode = new List<(ulong bit, string name)>();
            listlinkarrow = new List<(ulong bit, string name)>();

            BindingOperations.EnableCollectionSynchronization(RuleItems, _lockObject);
            BindingOperations.EnableCollectionSynchronization(TypeItems, _lockObject);
            BindingOperations.EnableCollectionSynchronization(RaceItems, _lockObject);
            BindingOperations.EnableCollectionSynchronization(CharItems, _lockObject);
            BindingOperations.EnableCollectionSynchronization(AttributeItems, _lockObject);
            BindingOperations.EnableCollectionSynchronization(SetCodeItems, _lockObject);
            BindingOperations.EnableCollectionSynchronization(CategoryItems, _lockObject);
            BindingOperations.EnableCollectionSynchronization(FlagItems, _lockObject);
        }

        public async Task LoadData()
        {
            string LanguageCode = ConfigViewModel.Instance.userSetting.Language;
            string Game = ConfigViewModel.Instance.userSetting.Game;

            await Task.WhenAll(
                LoadRule(LanguageCode, Game),
                LoadType(LanguageCode),
                LoadTypeImage(LanguageCode),
                LoadRace(LanguageCode),
                LoadChar(LanguageCode),
                LoadAttri(LanguageCode),
                LoadSetCode(LanguageCode, Game),
                LoadCategory(LanguageCode, Game),
                LoadLinkArrow(LanguageCode),
                LoadFlag(LanguageCode),
                LoadSpecialCharacters(LanguageCode)
                // LoadImgStamp()
                );
        }
        private async Task LoadRule(string LanguageCode, string Game)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\rule{Game}.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listrule.Clear();
                var tempList = new List<RuleItem>();
                var existedRuleCodes = new HashSet<ulong>();
                const int bufferSize = 4096;

                using var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);
                string line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) continue;
                    var parts = line.Split('\t');
                    if (parts.Length < 2) continue;

                    string hexPart = parts[0].Substring(2).Trim();
                    if (!ulong.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out ulong bitValue))
                        continue;

                    string ruleName = parts[1].Trim();
                    if (existedRuleCodes.Add(bitValue))
                    {
                        tempList.Add(new RuleItem { RuleCode = bitValue, RuleName = ruleName });
                        listrule.Add((bitValue, ruleName));
                    }
                }
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    lock (_lockObject)
                    {
                        RuleItems.Clear();
                        RuleItems.AddRange(tempList);
                    }
                });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadType(string LanguageCode)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\type.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listtypedeck.Clear();
                var tempList = new List<TypeItem>();
                var existedTypeCodes = new HashSet<ulong>();
                const int bufferSize = 4096;

                using var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);
                string line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) continue;
                    var parts = line.Split('\t');
                    if (parts.Length < 2) continue;

                    string hexPart = parts[0].Substring(2).Trim();
                    if (!ulong.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out ulong bitValue))
                        continue;

                    string typeName = parts[1].Trim();
                    if (existedTypeCodes.Add(bitValue))
                    {
                        tempList.Add(new TypeItem { TypeCode = bitValue, TypeName = typeName });
                        listtypedeck.Add((bitValue, typeName));
                    }
                }
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    lock (_lockObject)
                    {
                        TypeItems.Clear();
                        TypeItems.AddRange(tempList);
                    }
                });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadTypeImage(string LanguageCode)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\typeImage.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listtype.Clear();
                var tempList = new List<(ulong bit, string name)>();
                var existedTypeCodes = new HashSet<ulong>();
                const int bufferSize = 4096;

                using var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);
                string line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) continue;
                    var parts = line.Split('\t');
                    if (parts.Length < 2) continue;

                    string hexPart = parts[0].Substring(2).Trim();
                    if (!ulong.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out ulong bitValue))
                        continue;

                    string name = parts[1].Trim();
                    if (existedTypeCodes.Add(bitValue))
                    {
                        tempList.Add((bitValue, name));
                    }
                }
                listtype = tempList.AsEnumerable().Reverse().ToList();
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadRace(string LanguageCode)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\race.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listrace.Clear();
                var tempList = new List<RaceItem>();
                var existedRaceCodes = new HashSet<ulong>();
                const int bufferSize = 4096;

                using var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);
                string line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) continue;
                    var parts = line.Split('\t');
                    if (parts.Length < 2) continue;

                    string hexPart = parts[0].Substring(2).Trim();
                    if (!ulong.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out ulong bitValue))
                        continue;

                    string raceName = parts[1].Trim();
                    if (existedRaceCodes.Add(bitValue))
                    {
                        tempList.Add(new RaceItem { RaceCode = bitValue, RaceName = raceName });
                        listrace.Add((bitValue, raceName));
                    }
                }
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    lock (_lockObject)
                    {
                        RaceItems.Clear();
                        RaceItems.AddRange(tempList);
                    }
                });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadChar(string LanguageCode)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\character.txt");
            if (!File.Exists(dataPath)) return;
            try
            {
                listchar.Clear();
                var tempList = new List<CharItem>();
                var existedCharCodes = new HashSet<ulong>();
                const int bufferSize = 4096;

                using var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);
                string line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) continue;
                    var parts = line.Split('\t');
                    if (parts.Length < 2) continue;

                    string hexPart = parts[0].Substring(2).Trim();
                    if (!ulong.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out ulong bitValue))
                        continue;

                    string charName = parts[1].Trim();
                    if (existedCharCodes.Add(bitValue))
                    {
                        tempList.Add(new CharItem { CharCode = bitValue, CharName = charName });
                        listchar.Add((bitValue, charName));
                    }
                }
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    lock (_lockObject)
                    {
                        CharItems.Clear();
                        CharItems.AddRange(tempList);
                    }
                });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadAttri(string LanguageCode)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\attribute.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listattr.Clear();
                var tempList = new List<AttributeItem>();
                var existedAttriCodes = new HashSet<ulong>();
                const int bufferSize = 4096;

                using var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);
                string line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) continue;
                    var parts = line.Split('\t');
                    if (parts.Length < 2) continue;

                    string hexPart = parts[0].Substring(2).Trim();
                    if (!ulong.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out ulong bitValue))
                        continue;

                    string attriName = parts[1].Trim();
                    if (existedAttriCodes.Add(bitValue))
                    {
                        tempList.Add(new AttributeItem { AttributeCode = bitValue, AttributeName = attriName });
                        listattr.Add((bitValue, attriName));
                    }
                }
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    lock (_lockObject)
                    {
                        AttributeItems.Clear();
                        AttributeItems.AddRange(tempList);
                    }
                });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadSetCode(string LanguageCode, string Game)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\setname{Game}.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listsetcode.Clear();
                var tempList = new List<SetCodeItem>();
                var existedSetCodeCodes = new HashSet<ulong>();
                const int bufferSize = 4096;

                using var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);
                string line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) continue;
                    var parts = line.Split('\t');
                    if (parts.Length < 2) continue;

                    string hexPart = parts[0].Substring(2).Trim();
                    if (!ulong.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out ulong bitValue))
                        continue;

                    string setCodeName = parts[1].Trim();
                    if (existedSetCodeCodes.Add(bitValue))
                    {
                        tempList.Add(new SetCodeItem { SetCode = bitValue, SetName = setCodeName });
                        listsetcode.Add((bitValue, setCodeName));
                    }
                }
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    lock (_lockObject)
                    {
                        SetCodeItems.Clear();
                        SetCodeItems.AddRange(tempList);
                    }
                });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadCategory(string LanguageCode, string Game)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\category{Game}.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                var tempList = new List<CategoryItem>();
                var existedCategoryCodes = new HashSet<ulong>();
                const int bufferSize = 4096;

                using var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);
                string line;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) continue;
                    var parts = line.Split('\t');
                    if (parts.Length < 2) continue;

                    string hexPart = parts[0].Substring(2).Trim();
                    if (!ulong.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out ulong bitValue))
                        continue;

                    string categoryName = parts[1].Trim();
                    if (existedCategoryCodes.Add(bitValue))
                    {
                        tempList.Add(new CategoryItem { CategoryCode = bitValue, CategoryName = categoryName });
                    }
                }
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    lock (_lockObject)
                    {
                        CategoryItems.Clear();
                        CategoryItems.AddRange(tempList);
                    }
                });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadLinkArrow(string LanguageCode)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\linkmarker.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listlinkarrow.Clear();
                var tempList = new List<(ulong bit, string name)>();
                var existedLinkarrow = new HashSet<ulong>();
                const int bufferSize = 4096;

                using var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);
                string line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) continue;
                    var parts = line.Split('\t');
                    if (parts.Length < 2) continue;

                    string hexPart = parts[0].Substring(2).Trim();
                    if (!ulong.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out ulong bitValue))
                        continue;

                    string linkArrowName = parts[1].Trim();
                    if (existedLinkarrow.Add(bitValue))
                    {
                        tempList.Add((bitValue, linkArrowName));
                    }
                }
                listlinkarrow = tempList.AsEnumerable().Reverse().ToList();
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadFlag(string LanguageCode)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\flag.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                var tempList = new List<FlagItem>();
                var existedFlagCodes = new HashSet<ulong>();
                const int bufferSize = 4096;

                using var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);
                string line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (!line.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) continue;
                    var parts = line.Split('\t');
                    if (parts.Length < 2) continue;

                    string hexPart = parts[0].Substring(2).Trim();
                    if (!ulong.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out ulong bitValue))
                        continue;

                    string flagName = parts[1].Trim();
                    if (existedFlagCodes.Add(bitValue))
                    {
                        tempList.Add(new FlagItem { FlagCode = bitValue, FlagName = flagName });
                    }
                }
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    lock (_lockObject)
                    {
                        FlagItems.Clear();
                        FlagItems.AddRange(tempList);
                    }
                });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadSpecialCharacters(string LanguageCode)
        {
            string speCharPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\SpecialCharacters.csv");
            if (!File.Exists(speCharPath)) return;

            try
            {
                SpecialCharacters.Clear();
                var tempList = new List<CharacterItem>();
                using (var stream = new FileStream(speCharPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    bool isFirstLine = true;
                    string line;
                    while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue;
                        }
                        var parts = line.Split(',');
                        if (parts.Length >= 3)
                        {
                            tempList.Add(new CharacterItem
                            {
                                Character = parts[0].Trim(),
                                Category = parts[1].Trim(),
                                Description = parts[2].Trim()
                            });
                        }
                    }
                }
                SpecialCharacters.AddRange(tempList);
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }

        public void Dispose()
        {
            RuleItems.Clear();
            RuleItems = null;
            TypeItems.Clear();
            TypeItems = null;
            RaceItems.Clear();
            RaceItems = null;
            CharItems.Clear();
            CharItems = null;
            AttributeItems.Clear();
            AttributeItems = null;
            SetCodeItems.Clear();
            SetCodeItems = null;
            CategoryItems.Clear();
            CategoryItems = null;
            FlagItems.Clear();
            FlagItems = null;
            LevelItems.Clear();
            LevelItems = null;
            LinkArrowItems.Clear();
            LinkArrowItems = null;
            SpecialCharacters.Clear();
            SpecialCharacters = null;
            listrule.Clear();
            listrule = null;
            listtype.Clear();
            listtype = null;
            listtypedeck.Clear();
            listtypedeck = null;
            listrace.Clear();
            listrace = null;
            listchar.Clear();
            listchar = null;
            listattr.Clear();
            listattr = null;
            listsetcode.Clear();
            listsetcode = null;
            listlinkarrow.Clear();
            listlinkarrow = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddTab(string header, UserControl content)
        {
            // Tabs.Add(new TabContent { Header = header, Content = content });
        }
        public void UpdateTabHeader(UserControl content, string newHeader)
        {
            //var tab = Tabs.FirstOrDefault(t => t.Content == content);
            //if (tab != null)
            //{
            //    tab.Header = newHeader;
            //}
        }

        public void MoveTabToNewWindow(TabContent tab)
        {
            //if (Tabs.Contains(tab))
            //{
            //    Tabs.Remove(tab);
            //    var newWindow = new MainWindow();
            //    newWindow.TabControlMain.ItemsSource = new ObservableCollection<TabContent> { tab };
            //    newWindow.Show();
            //}
        }

    }
}
