using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using CardAppContext = CardEditor.Models.AppContext;
using CardEditor.Services;

namespace CardEditor.ViewModels
{
    public class GenesysRawDataViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<GenesysRawDataViewModel> _instance = new Lazy<GenesysRawDataViewModel>(() => new GenesysRawDataViewModel());
        public static GenesysRawDataViewModel Instance => _instance.Value;

        #region Raw Data Storage
        public bool IsLoadGenesysCardListFromDB { get; private set; } = false;
        public bool IsSaveGenesysCardListToDB { get; private set; } = true;

        // private bool _isGenesysCardDataLoading = false;

        private Dictionary<ulong, GenesysCard> _genesysCardsData = new Dictionary<ulong, GenesysCard>();
        public IReadOnlyDictionary<ulong, GenesysCard> GenesysCardsData => _genesysCardsData;
        #endregion

        #region Genesys Card
        public async Task CreateGenesysCardDataBase()
        {
            if (!File.Exists(CardAppContext.Instance.GenesysDBPath))
            {
                var (resultCreate, messageCreate) = await Task.Run(() => CreateFileServices.CreateGenesysCardsDatabase(CardAppContext.Instance.GenesysFolderPath, "GenesysCardsDB.cdb"));
                if (!resultCreate)
                {
                    OnErrorOccurred?.Invoke($"{CMess.errorCreaCGNS.ToText()} {messageCreate}");
                    return;
                }
            }
            await LoadGenesysCardDataFromDatabase();
        }
        public async Task LoadGenesysCardDataFromDatabase()
        {
            try
            {
                _genesysCardsData.Clear();
                using (var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.GenesysDBPath};Version=3;"))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT id, name, point FROM GenesysCard";
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var card = new GenesysCard
                                {
                                    Id = reader.IsDBNull(0) ? 0UL : unchecked((ulong)Convert.ToUInt64(reader.GetValue(0))),
                                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                    GPoints = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                                };
                                _genesysCardsData[card.Id] = card;
                            }
                        }
                    }
                }

                IsLoadGenesysCardListFromDB = true;
                IsSaveGenesysCardListToDB = true;
                // OnDataChanged?.Invoke();
            }
            catch(Exception ex)
            {
                OnErrorOccurred?.Invoke($"{CMess.errorLoadDB.ToText()} {ex.Message}");
            }
        }

        public async Task<(bool, string)> ModifyGenesysCardDatabase(GenesysCard genesysCard)
        {
            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.GenesysDBPath};Version=3;");
                await connection.OpenAsync();
                const string upsertQuery = @"
                    INSERT INTO GenesysCard (id, name, point)
                    VALUES (@id, @name, @point) 
                    ON CONFLICT(id) DO UPDATE SET
                        name = excluded.name,
                        point = excluded.point;";
                using var command = new SQLiteCommand(upsertQuery, connection);
                command.Parameters.AddWithValue("@id", genesysCard.Id);
                command.Parameters.AddWithValue("@name", genesysCard.Name);
                command.Parameters.AddWithValue("@point", genesysCard.GPoints);
                await command.ExecuteNonQueryAsync();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{CMess.errorAddCard.ToText()} {ex.Message}");
                return (false, ex.Message);
            }
        }
        public async Task<(bool, string)> SaveAllGenesysCardDatabase()
        {
            SQLiteTransaction transaction = null;
            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.GenesysDBPath};Version=3;");
                await connection.OpenAsync();

                transaction = connection.BeginTransaction();
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = @"
                    INSERT INTO GenesysCard (id, name, point)
                    VALUES (@id, @name, @point) 
                    ON CONFLICT(id) DO UPDATE SET
                        name = excluded.name,
                        point = excluded.point;";

                var paramId = command.CreateParameter();
                paramId.ParameterName = "@id";
                command.Parameters.Add(paramId);

                var paramName = command.CreateParameter();
                paramName.ParameterName = "@name";
                command.Parameters.Add(paramName);

                var paramPoint = command.CreateParameter();
                paramPoint.ParameterName = "@point";
                command.Parameters.Add(paramPoint);

                foreach (var card in _genesysCardsData)
                {
                    paramId.Value = card.Key;
                    paramName.Value = card.Value.Name;
                    paramPoint.Value = card.Value.GPoints;

                    await command.ExecuteNonQueryAsync();
                }
                transaction.Commit();
                IsSaveGenesysCardListToDB = true;
                return (true, _genesysCardsData.Count.ToString());
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch
                    {
                        ///
                    }
                }
                return (false, $"{CMess.errorSaveCard.ToText()} {ex.Message}");
            }

        }
        public async Task<(bool, string)> DeleteGenesysCardDatabase(GenesysCard genesysCard)
        {
            return await DeleteGenesysCardDatabaseInternal(genesysCard.Id);
        }
        public async Task<(bool, string)> DeleteGenesysCardDatabase(ulong cardID)
        {
            return await DeleteGenesysCardDatabaseInternal(cardID);
        }
        public async Task<(bool, string)> DeleteGenesysCardDatabaseInternal(ulong cardID)
        {
            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.GenesysDBPath};Version=3;");
                await connection.OpenAsync();

                using var command = new SQLiteCommand("DELETE FROM GenesysCard WHERE id = @id;", connection);
                command.Parameters.AddWithValue("@id", cardID);

                int affectedRows = await command.ExecuteNonQueryAsync();
                return (affectedRows > 0, string.Empty);
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{CMess.errorDeleteCard.ToText()} {ex.Message}");
                return (false, ex.Message);
            }
        }
        public async Task<(bool, string)> DeleteMultipleGenesysCardDatabase(IEnumerable<GenesysCard> cards)
        {
            if (cards == null || !cards.Any()) return (false, CMess.noCardSelec.ToText());

            var validCards = cards.Where(c => c != null).ToList();
            if (!validCards.Any()) return (false, CMess.noCardSelec.ToText());

            SQLiteTransaction transaction = null;

            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.GenesysDBPath};Version=3;");
                await connection.OpenAsync();

                transaction = connection.BeginTransaction();
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                var idParamNames = new List<string>();
                for (int i = 0; i < validCards.Count; i++)
                {
                    var param = command.CreateParameter();
                    param.ParameterName = $"@id{i}";
                    param.Value = validCards[i].Id;
                    command.Parameters.Add(param);
                    idParamNames.Add(param.ParameterName);
                }

                command.CommandText = $"DELETE FROM GenesysCard WHERE id IN ({string.Join(", ", idParamNames)});";
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                try
                {
                    transaction?.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    OnErrorOccurred?.Invoke($"Rollback failed: {rollbackEx.Message}");
                }
                OnErrorOccurred?.Invoke($"{CMess.errorDeleteCard.ToText()} {ex.Message}");
                return (false, ex.Message);
            }
        }
        public async Task<(bool, string)> DeleteAllGenesysCardsDatabase()
        {
            SQLiteTransaction transaction = null;

            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.GenesysDBPath};Version=3;");
                await connection.OpenAsync();

                transaction = connection.BeginTransaction();
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = "DELETE FROM GenesysCard;";
                await command.ExecuteNonQueryAsync();

                transaction.Commit();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                try
                {
                    transaction?.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    OnErrorOccurred?.Invoke($"Rollback failed: {rollbackEx.Message}");
                }
                OnErrorOccurred?.Invoke($"{CMess.errorDeleteCard.ToText()} {ex.Message}");
                return (false, ex.Message);
            }
        }

        public (bool, bool) ModifyGenesysCard(GenesysCard genesysCard)
        {
            if (genesysCard == null) return (false, false);

            bool isExisting = _genesysCardsData.ContainsKey(genesysCard.Id);

            _genesysCardsData[genesysCard.Id] = genesysCard;
            return (true, !isExisting);
        }
        public (bool, bool) ModifyGenesysCardByID(ulong cardID, GenesysCard newCard)
        {
            if (newCard == null) return (false, false);

            bool isExisting = _genesysCardsData.ContainsKey(cardID);
            var newCardAdd = new GenesysCard { Id = cardID, Name = newCard.Name, GPoints = newCard.GPoints };
            _genesysCardsData[cardID] = newCardAdd;

            OnDataChanged?.Invoke();
            return (true, !isExisting);
        }
        public bool AddGenesysCard(GenesysCard genesysCard)
        {
            if (genesysCard == null) return false;

            if (!_genesysCardsData.ContainsKey(genesysCard.Id))
            {
                _genesysCardsData[genesysCard.Id] = genesysCard;
                return true;
            }
            else
            {
                OnErrorOccurred?.Invoke($"Card with ID {genesysCard.Id} already exists");
                return false;
            }
        }
        public bool UpdateGenesysCard(GenesysCard oldCard, GenesysCard newCard)
        {
            if (oldCard == null || newCard == null) return false;

            if (_genesysCardsData.ContainsKey(oldCard.Id))
            {
                
                var newCardAdd = new GenesysCard { Id = oldCard.Id, Name = newCard.Name, GPoints = newCard.GPoints };
                _genesysCardsData[oldCard.Id] = newCardAdd;
            }
            else _genesysCardsData[oldCard.Id] = newCard;
            return true;
        }
        public bool UpdateGenesysCardByID(ulong cardID, GenesysCard newCard)
        {
            if (newCard == null) return false;

            if(_genesysCardsData.ContainsKey(cardID))
            {
                var newCardAdd = new GenesysCard { Id = cardID, Name = newCard.Name, GPoints = newCard.GPoints };
                _genesysCardsData[cardID] = newCardAdd;
                return true;
            }
            else _genesysCardsData[cardID] = newCard;
            return true;
        }

        public int ModifyMultipleGenesysCard(IEnumerable<GenesysCard> genesysCards)
        {
            if (genesysCards == null || !genesysCards.Any()) return 0;
            int modifiedCount = 0;

            foreach(var card in genesysCards)
            {
                if (card == null) continue;
                _genesysCardsData[card.Id] = card;
                modifiedCount++;
            }

            if (modifiedCount > 0)
            {
                OnDataChanged?.Invoke();
            }
            return modifiedCount;
        }
        public int AddGenesysCards(IEnumerable<GenesysCard> genesysCards)
        {
            if (genesysCards == null || !genesysCards.Any()) return 0;
            int addedCount = 0;

            foreach(var card in genesysCards)
            {
                if (card == null) continue;
                if (!_genesysCardsData.ContainsKey(card.Id))
                {
                    _genesysCardsData[card.Id] = card;
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                OnDataChanged?.Invoke();
            }
            return addedCount;
        }

        public bool RemoveGenesysCardByID(ulong cardID)
        {
            bool removed = _genesysCardsData.Remove(cardID);
            //if (removed)
            //{
            //    OnDataChanged?.Invoke();
            //}
            return removed;
        }
        public bool RemoveGenesysCard(GenesysCard genesysCard)
        {
            if (genesysCard == null) return false;
            return RemoveGenesysCardByID(genesysCard.Id);
        }
        public int RemoveMultipleGenesysCards(IEnumerable<GenesysCard> cards)
        {
            if (cards == null) return 0;
            return cards.Where(card => card != null).Count(cards => _genesysCardsData.Remove(cards.Id));
        }
        public int RemoveAllGenesysCards()
        {
            int removedCount = _genesysCardsData.Count;
            if (removedCount == 0) return 0;
            
            _genesysCardsData.Clear();
            OnDataChanged?.Invoke();
            return removedCount;
        }
        public void ClearGenesysCards()
        {
            if (_genesysCardsData.Count > 0)
            {
                _genesysCardsData.Clear();
                OnDataChanged?.Invoke();
            }
        }
        #endregion

        #region Helper Methods
        public async Task<(bool, string)> LoadData()
        {
            try
            {
                if (!Directory.Exists(CardEditor.Models.AppContext.Instance.GenesysFolderPath))
                    Directory.CreateDirectory(CardEditor.Models.AppContext.Instance.GenesysFolderPath);

                await Task.WhenAll(CreateGenesysCardDataBase());

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public int GetGenesysPointByID(ulong id)
        {
            return _genesysCardsData.TryGetValue(id, out var card) ? card.GPoints : 0;
        }

        public async Task<(bool, string)> BrowseDataCardDataBase(string filePath, bool Overwrite)
        {
            var tempList = new List<GenesysCard>();

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={filePath};Version=3;"))
                {
                    await connection.OpenAsync();
                    var tableNames = new HashSet<string>();
                    using (var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tableNames.Add(reader.GetString(0).ToLower());
                        }
                    }

                    if (tableNames.Contains("GenesysCard"))
                    {
                        using (var command = new SQLiteCommand("SELECT id, name, point FROM GenesysCard", connection))
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ulong id = reader.IsDBNull(0) ? 0UL : unchecked((ulong)Convert.ToUInt64(reader.GetValue(0)));
                                string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                                int point = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2));

                                var card = new GenesysCard
                                {
                                    Id = id,
                                    Name = name,
                                    GPoints = point
                                };
                                tempList.Add(card);
                            }
                        }
                    }
                    else
                    {
                        OnErrorOccurred?.Invoke($"{CMess.CardDB.ToText()} {CMess.invaFileForm.ToText()}");
                        return (false, $"{CMess.CardDB.ToText()} {CMess.invaFileForm.ToText()}");
                    }
                }

                try
                {
                    if (MergeGenesysCards(tempList, Overwrite))
                    {
                        IsSaveGenesysCardListToDB = false;
                        OnDataChanged?.Invoke();
                        return (true, string.Empty);
                    }
                    else
                    {
                        OnErrorOccurred?.Invoke(CMess.noValiCardFound.ToText());
                        return (false, CMess.noValiCardFound.ToText());
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccurred?.Invoke($"{CMess.errorImport.ToText()} {ex.Message}");
                    return (false, $"{CMess.errorImport.ToText()} {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{CMess.CardDB.ToText()} {CMess.errorRead.ToText()} {ex.Message}");
                return (false, $"{CMess.CardDB.ToText()} {CMess.errorRead.ToText()} {ex.Message}");
            }
        }
        public async Task<(bool, string)> BrowseDataCardText(string filePath, bool Overwrite)
        {
            var tempList = new List<GenesysCard>();

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    int lineNumber = 0;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNumber++;

                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var parts = line.Split('\t');
                        if (parts.Length != 3) continue;

                        if (!ulong.TryParse(parts[0], out var id) || !int.TryParse(parts[2], out var gPoints)) continue;

                        var name = parts[1].Trim();

                        tempList.Add(new GenesysCard
                        {
                            Id = id,
                            Name = name,
                            GPoints = gPoints
                        });
                    }
                }

                try
                {
                    if (MergeGenesysCards(tempList, Overwrite))
                    {
                        IsSaveGenesysCardListToDB = false;
                        OnDataChanged?.Invoke();
                        return (true, string.Empty);
                    }
                    else
                    {
                        OnErrorOccurred?.Invoke(CMess.noValiCardFound.ToText());
                        return (false, CMess.noValiCardFound.ToText());
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccurred?.Invoke($"{CMess.errorImport.ToText()} {ex.Message}");
                    return (false, $"{CMess.errorImport.ToText()} {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{CMess.CardDB.ToText()} {CMess.errorRead.ToText()} {ex.Message}");
                return (false, $"{CMess.errorRead.ToText()} {ex.Message}");
            }
        }

        private bool MergeGenesysCards(IEnumerable<GenesysCard> cards, bool Overwrite)
        {
            if (cards == null) return false;

            bool changed = false;

            if (Overwrite)
            {
                foreach (var card in cards)
                {
                    if (card == null) continue;
                    _genesysCardsData[card.Id] = card;
                    changed = true;
                }
            }
            else
            {
                foreach (var card in cards)
                {
                    if (card == null) continue;
                    if (!_genesysCardsData.ContainsKey(card.Id))
                    {
                        _genesysCardsData[card.Id] = card;
                        changed = true;
                    }
                }
            }
            return changed;
        }

        #endregion

        #region Event
        public event Action OnDataChanged;
        public event Action<string> OnErrorOccurred;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            _genesysCardsData.Clear();
            _genesysCardsData = null;

            OnDataChanged = null;
            OnErrorOccurred = null;
            PropertyChanged = null;
        }
        #endregion

    }
}
