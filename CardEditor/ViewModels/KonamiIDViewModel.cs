using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using System.Data.SQLite;

namespace CardEditor.ViewModels
{
    public class KonamiIDViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<KonamiIDViewModel> _instance = new Lazy<KonamiIDViewModel>(() => new KonamiIDViewModel());
        public static KonamiIDViewModel Instance => _instance.Value;

        private Dictionary<ulong, int> _officialMapID = new();
        private Dictionary<string, int> _rushMapID = new();
        public IReadOnlyDictionary<ulong, int> OfficialMapID => _officialMapID;
        public IReadOnlyDictionary<string, int> RushMapID => _rushMapID;

        public bool IsLoaded = false;

        private KonamiIDViewModel()
        {

        }

        public async Task<(bool, string)> LoadKonamiID()
        {
            if (IsLoaded) return (true, string.Empty);

            string dbFilePath = System.IO.Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, $@"CardData\KonamiID\GetKonamiID.cdb");
            if (!System.IO.File.Exists(dbFilePath)) return (false, $"{CMess.fileNotExit.ToText()} GetKonamiID.cdb");

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={dbFilePath};Version=3;"))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT konami_id, password FROM dataOfficial";
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int konamiID = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                                ulong password = reader.IsDBNull(1) ? 0UL : unchecked((ulong)Convert.ToUInt64(reader.GetValue(1)));
                                _officialMapID[password] = konamiID;
                            }
                        }
                    }
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT konami_id, name FROM dataRush";
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int konamiID = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                                string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                                _rushMapID[name] = konamiID;
                            }
                        }
                    }
                }
                IsLoaded = true;
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public int? GetOfficialKonamiID(ulong id)
        {
            if (!IsLoaded || OfficialMapID == null) return null;
            if (OfficialMapID.TryGetValue(id, out int konamiID)) return konamiID;
            return null;
        }
        public int? GetRushKonamiID(string name)
        {
            if (!IsLoaded || RushMapID == null) return null;
            if (name.EndsWith(" (Rush)")) name = name.Substring(0, name.Length - " (Rush)".Length);
            if (RushMapID.TryGetValue(name, out int konamiID)) return konamiID;
            return null;
        }

        public void Dispose()
        {
            _officialMapID?.Clear();
            _officialMapID = null;
            _rushMapID?.Clear();
            _rushMapID = null;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
