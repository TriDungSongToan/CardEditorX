using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ScriptSupport.Legacy.Collections;
using CMess = ScriptSupport.Legacy.Localization.Language;
using ScriptSupport.Legacy.Localization;

namespace ScriptSupport.Legacy.ViewModels
{
    public class CardInfoViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<CardInfoViewModel> _instance = new Lazy<CardInfoViewModel>(() => new CardInfoViewModel());
        public static CardInfoViewModel Instance => _instance.Value;

        private string DataFolderPath => ScriptSupport.Legacy.Models.AppContext.Instance.DataFolderPath;
        public List<(ulong bit, string name)> listrule { get; set; }
        public List<(ulong bit, string name)> listtype { get; set; }
        public List<(ulong bit, string name)> listtypedeck { get; set; }
        public List<(ulong bit, string name)> listrace { get; set; }
        public List<(ulong bit, string name)> listchar { get; set; }
        public List<(ulong bit, string name)> listattr { get; set; }
        public List<(ulong bit, string name)> listsetcode { get; set; }
        public List<(ulong bit, string name)> listlinkarrow { get; set; }
        public HashSet<ulong> darksynchrolist { get; set; }

        private static readonly object _lockObject = new object();

        private CardInfoViewModel()
        {
            listrule = new List<(ulong bit, string name)>();
            listtype = new List<(ulong bit, string name)>();
            listtypedeck = new List<(ulong bit, string name)>();
            listrace = new List<(ulong bit, string name)>();
            listchar = new List<(ulong bit, string name)>();
            listattr = new List<(ulong bit, string name)>();
            listsetcode = new List<(ulong bit, string name)>();
            listlinkarrow = new List<(ulong bit, string name)>();
            darksynchrolist = new HashSet<ulong>();
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
                LoadLinkArrow(LanguageCode),
                LoadDarkSynchro()
                );
        }
        private async Task LoadRule(string LanguageCode, string Game)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\rule{Game}.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listrule.Clear();
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

                        listrule.Add((bitValue, ruleName));
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorRead.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadType(string LanguageCode)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\type.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listtypedeck.Clear();
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
                        listtypedeck.Add((bitValue, typeName));
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorRead.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
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
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorRead.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadRace(string LanguageCode)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\race.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listrace.Clear();
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
                        listrace.Add((bitValue, raceName));
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorRead.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadChar(string LanguageCode)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\character.txt");
            if (!File.Exists(dataPath)) return;
            try
            {
                listchar.Clear();
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
                        listchar.Add((bitValue, charName));
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorRead.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadAttri(string LanguageCode)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\attribute.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listattr.Clear();
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
                        listattr.Add((bitValue, attriName));
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorRead.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadSetCode(string LanguageCode, string Game)
        {
            string dataPath = Path.Combine(DataFolderPath, $@"CardData\Language\{LanguageCode}\cardinfo\setname{Game}.txt");
            if (!File.Exists(dataPath)) return;

            try
            {
                listsetcode.Clear();
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
                        listsetcode.Add((bitValue, setCodeName));
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorRead.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
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
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorRead.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadDarkSynchro()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"data\CardData\Game\DarkSynchro.txt");
            if (!File.Exists(filePath)) return;

            try
            {
                darksynchrolist.Clear();
                const int bufferSize = 4096;

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);
                string line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (ulong.TryParse(line.Trim(), out ulong number))
                        darksynchrolist.Add(number);
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorRead.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }

        public void Dispose()
        {
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

    }
}
