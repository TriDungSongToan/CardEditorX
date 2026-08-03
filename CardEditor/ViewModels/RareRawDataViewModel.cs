using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Data.SQLite;
using System.Windows.Data;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using SkiaSharp;
using OfficeOpenXml;
using System.Runtime.CompilerServices;
using CardEditor.Models;
using CardEditor.Services;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using CardAppContext = CardEditor.Models.AppContext;


namespace CardEditor.ViewModels
{
    public class RareRawDataViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<RareRawDataViewModel> _instance = new Lazy<RareRawDataViewModel>(() =>  new RareRawDataViewModel());
        public static RareRawDataViewModel Instance => _instance.Value;

        #region Raw Data Storage
        private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();
        public bool IsLoadRareCardListFromDB = false;
        public bool IsSaveRareCardListToDB = true;
        public bool IsLoadedImageRareCache = false;
        public bool IsLoadedImageRareRect = false;
        
        private BulkObservableCollection<RareItem> _rareItemsForUI = new BulkObservableCollection<RareItem>();
        public BulkObservableCollection<RareItem> RareItemsForUI
        {
            get => _rareItemsForUI;
            set
            {
                if (_rareItemsForUI != value)
                {
                    _rareItemsForUI = value;
                    OnPropertyChanged();
                }
            }
        }
        private static readonly object _lockObject = new object();

        public List<KeyValuePair<long, SKBitmap>> RarityCache { get; set; } = new List<KeyValuePair<long, SKBitmap>>();

        private Dictionary<int, RareItem> _rareItemsData = new Dictionary<int, RareItem>();
        public IReadOnlyDictionary<int, RareItem> RareItemsData => _rareItemsData;

        private Dictionary<ulong, RareCard> _rareCardsData = new Dictionary<ulong, RareCard>();
        public IReadOnlyDictionary<ulong, RareCard> RareCardsData => _rareCardsData;
        #endregion

        private RareRawDataViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(RareItemsForUI, _lockObject);
        }

        #region Rare List
        public async Task CreateRaresListDataBase()
        {
            if (!File.Exists(CardAppContext.Instance.RaresListDBPath))
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(CardAppContext.Instance.RaresFolderPath, "RaresListDB.cdb")))
                {
                    int result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                        $"{CMess.filealreadyExit.ToText()} {CMess.QuestOverwrite.ToText()}", new[] { CMess.yes.ToText(), CMess.no.ToText() });
                    if (result != 0) return;
                }
                var (resultCreate, messageCreate) = await Task.Run(() => CreateFileServices.CreateRaresListDatabase(CardAppContext.Instance.RaresFolderPath, "RaresListDB.cdb"));
                if (!resultCreate)
                {
                    OnErrorOccurred?.Invoke($"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.ListRareDB.ToText(), CMess.File.ToText())} {messageCreate}");
                    return;
                }
                
            }
            await LoadRareListDataFromDatabase();
        }
        public async Task LoadRareListDataFromDatabase()
        {
            try
            {
                _rareItemsData.Clear();
                using (var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.RaresListDBPath};Version=3;"))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT IdRare, Name, Code, ImagePath FROM RareList ORDER BY Code ASC";
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new RareItem
                                {
                                    IdRare = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Code = reader.GetInt64(2),
                                    ImagePath = reader.IsDBNull(3) ? null : reader.GetString(3)
                                };
                                _rareItemsData[item.IdRare] = item;
                            }
                        }
                    }

                }
                OnDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{CMess.errorLoadDB.ToText()} {ex.Message}");
            }
        }
        public async Task<(bool,string)> ModifyRareListDatabase(RareItem rareItem)
        {
            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.RaresListDBPath};Version=3;");
                await connection.OpenAsync();
                const string upsertQuery = @"
                    INSERT INTO RareList (IdRare, Name, Code, ImagePath)
                    VALUES (@IdRare, @Name, @Code, @ImagePath)
                    ON CONFLICT(IdRare) DO UPDATE SET
                    Name = excluded.Name,
                    Code = excluded.Code,
                    ImagePath = excluded.ImagePath;";

                using var command = new SQLiteCommand(upsertQuery, connection);
                command.Parameters.AddWithValue("@IdRare", rareItem.IdRare);
                command.Parameters.AddWithValue("@Name", rareItem.Name);
                command.Parameters.AddWithValue("@Code", rareItem.Code);
                command.Parameters.AddWithValue("@ImagePath", string.IsNullOrEmpty(rareItem.ImagePath) ? DBNull.Value : (object)rareItem.ImagePath);

                await command.ExecuteNonQueryAsync();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.Rarity.ToText())} {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> DeleteRareListDatabase(RareItem rareItem)
        {
            return await DeleteRareListDatabaseInternal(rareItem.IdRare);
        }
        public async Task<(bool, string)> DeleteRareListDatabase(int idRare)
        {
            return await DeleteRareListDatabaseInternal(idRare);
        }

        public async Task<(bool,string)> DeleteRareListDatabaseInternal(int idRare)
        {
            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.RaresListDBPath};Version=3;");
                await connection.OpenAsync();

                using var command = new SQLiteCommand("DELETE FROM RareList WHERE IdRare = @IdRare", connection);
                command.Parameters.AddWithValue("@IdRare", idRare);

                int affectedRows = await command.ExecuteNonQueryAsync();
                return (affectedRows > 0, string.Empty);
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Rarity.ToText())} {ex.Message}");
                return (false, ex.Message);
            }
        }

        public (bool, bool) ModifyRareItem(RareItem rareItem)
        {
            if (rareItem == null) return (false, false);

            bool isExisting = _rareItemsData.ContainsKey(rareItem.IdRare);
            _rareItemsData[rareItem.IdRare] = rareItem;
            return (true, !isExisting);
        }
        public bool AddRareItem(RareItem item)
        {
            if (item == null) return false;

            if (!RareItemsData.ContainsKey(item.IdRare))
            {
                _rareItemsData[item.IdRare] = item;
                return true;
            }
            else
            {
                OnErrorOccurred?.Invoke($"Item with ID {item.IdRare} already exists");
                return false;
            }
        }
        public bool UpdateRareItem(RareItem oldItem, RareItem newItem)
        {
            if (oldItem == null || newItem == null) return false;
            if (_rareItemsData.TryGetValue(oldItem.IdRare, out RareItem existingItem))
            {
                if (existingItem != null)
                {
                    existingItem.Name = newItem.Name;
                    existingItem.Code = newItem.Code;
                    existingItem.ImagePath = newItem.ImagePath;

                    return true;
                }
                else return false;
            }
            else return false;
        }

        public int ModifyMultipleRareItems(IEnumerable<RareItem> items)
        {
            if (items == null || !items.Any()) return 0;

            int modifiedCount = 0;

            foreach(var item in items)
            {
                if (item == null) continue;

                _rareItemsData[item.IdRare] = item;
                modifiedCount++;
            }
            if (modifiedCount > 0)
            {
                OnDataChanged?.Invoke();
            }
            return modifiedCount;
        }
        public int AddMultipleRareItems(IEnumerable<RareItem> items)
        {
            if (items == null || !items.Any()) return 0;

            int addedCount = 0;

            foreach (var item in items)
            {
                if (!_rareItemsData.ContainsKey(item.IdRare))
                {
                    _rareItemsData.Add(item.IdRare, item);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                OnDataChanged?.Invoke();
            }
            return addedCount;
        }

        public bool RemoveRareItemById(int rareId)
        {
            bool removed = _rareItemsData.Remove(rareId);
            if (removed)
            {
                //OnDataChanged?.Invoke();
            }
            return removed;
        }
        public bool RemoveRareItem(RareItem item)
        {
            if (item == null) return false;
            return RemoveRareItemById(item.IdRare);
        }
        public int RemoveMultipleRareItems(IEnumerable<RareItem> items)
        {
            if (items == null) return 0;
            return items.Where(item => item != null).Count(item => _rareItemsData.Remove(item.IdRare));
        }

        public void ClearRareItems()
        {
            if (_rareItemsData.Count > 0)
            {
                _rareItemsData.Clear();
                OnDataChanged?.Invoke();
            }
        }
        #endregion

        #region Rare Card
        public async Task CreateRareCardDataBase()
        {
            if (!File.Exists(CardAppContext.Instance.RareCardDBPath))
            {
                var (resultCreate, messageCreate) = await Task.Run(() => CreateFileServices.CreateRareCardsDatabase(CardAppContext.Instance.RaresFolderPath, "RareCardsDB.cdb"));
                if (!resultCreate)
                {
                    OnErrorOccurred?.Invoke($"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardRareDB.ToText(), CMess.File.ToText())} {messageCreate}");
                    return;
                }
            }
            await LoadRareCardDataFromDatabase();
        }
        public async Task LoadRareCardDataFromDatabase()
        {
            try
            {
                _rareCardsData.Clear();
                using (var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.RareCardDBPath};Version=3;"))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT id, name, rare FROM RareCard";
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var item = new RareCard
                                {
                                    id = reader.IsDBNull(0) ? 0UL : unchecked((ulong)Convert.ToUInt64(reader.GetValue(0))),
                                    name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                    rare = reader.IsDBNull(2) ? 0L : (long)reader.GetInt64(2),
                                };
                                _rareCardsData[item.id] = item;
                            }
                        }
                    }
                }
                IsLoadRareCardListFromDB = true;
                IsSaveRareCardListToDB = true;
                // OnDataChanged?.Invoke();
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{CMess.errorLoadDB.ToText()} {ex.Message}");
            }
        }

        public async Task<(bool,string)> ModifyRareCardDatabase(RareCard rareCard)
        {
            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.RareCardDBPath};Version=3;");
                await connection.OpenAsync();
                const string upsertQuery = @"
                    INSERT INTO RareCard (id, name, rare)
                    VALUES (@id, @name, @rare)
                    ON CONFLICT(id) DO UPDATE SET
                    name = excluded.name,
                    rare = excluded.rare;";
                using var command = new SQLiteCommand(upsertQuery, connection);
                command.Parameters.AddWithValue("@id", rareCard.id);
                command.Parameters.AddWithValue("@name", rareCard.name);
                command.Parameters.AddWithValue("@rare", rareCard.rare);
                await command.ExecuteNonQueryAsync();
                return (true,string.Empty);
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.Card.ToText())} {ex.Message}");
                return (false, ex.Message);
            }
        }
        public async Task<(bool,string)> SaveAllRareCardDatabase()
        {
            SQLiteTransaction transaction = null;
            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.RareCardDBPath};Version=3;");
                await connection.OpenAsync();

                transaction = connection.BeginTransaction();
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = @"
                    INSERT INTO RareCard (id, name, rare)
                    VALUES (@id, @name, @rare)
                    ON CONFLICT(id) DO UPDATE SET
                    name = excluded.name,
                    rare = excluded.rare;";

                var paramId = command.CreateParameter();
                paramId.ParameterName = "@id";
                command.Parameters.Add(paramId);

                var paramName = command.CreateParameter();
                paramName.ParameterName = "@name";
                command.Parameters.Add(paramName);

                var paramRare = command.CreateParameter();
                paramRare.ParameterName = "@rare";
                command.Parameters.Add(paramRare);

                foreach (var card in _rareCardsData)
                {
                    paramId.Value = card.Key;
                    paramName.Value = card.Value.name;
                    paramRare.Value = card.Value.rare;

                    await command.ExecuteNonQueryAsync();
                }
                transaction.Commit();
                IsSaveRareCardListToDB = true;
                return (true, _rareCardsData.Count.ToString());
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

                return (false, $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.Card.ToText())} {ex.Message}");
            }
        }
        public async Task<(bool, string)> DeleteRareCardDatabase(RareCard rareCard)
        {
            return await DeleteRareCardDatabaseInternal(rareCard.id);
        }
        public async Task<(bool, string)> DeleteRareCardDatabase(ulong cardID)
        {
            return await DeleteRareCardDatabaseInternal(cardID);
        }
        public async Task<(bool, string)> DeleteRareCardDatabaseInternal(ulong cardID)
        {
            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.RareCardDBPath};Version=3;");
                await connection.OpenAsync();

                using var command = new SQLiteCommand("DELETE FROM RareCard WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", cardID);

                int affectedRows = await command.ExecuteNonQueryAsync();
                return (affectedRows > 0, string.Empty);
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Card.ToText())} {ex.Message}");
                return (false, ex.Message);
            }
        }
        public async Task<(bool, string)> DeleteMultipleRareCardDatabase(IEnumerable<RareCard> cards)
        {
            if (cards == null || !cards.Any()) return (false, CMess.noCardSelec.ToText());

            var validCards = cards.Where(c => c != null).ToList();
            if (!validCards.Any()) return (false, CMess.noCardSelec.ToText());

            SQLiteTransaction transaction = null;

            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.RareCardDBPath};Version=3;");
                await connection.OpenAsync();

                transaction = connection.BeginTransaction();
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                var idParamNames = new List<string>();
                for (int i = 0; i < validCards.Count; i++)
                {
                    var param = command.CreateParameter();
                    param.ParameterName = $"@id{i}";
                    param.Value = validCards[i].id;
                    command.Parameters.Add(param);
                    idParamNames.Add(param.ParameterName);
                }

                command.CommandText = $"DELETE FROM RareCard WHERE id IN ({string.Join(",", idParamNames)})";
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
                OnErrorOccurred?.Invoke($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Card.ToText())} {ex.Message}");
                return (false, ex.Message);
            }
        }
        public async Task<(bool, string)> DeleteAllRareCardsDatabase()
        {
            SQLiteTransaction transaction = null;

            try
            {
                using var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.RareCardDBPath};Version=3;");
                await connection.OpenAsync();

                transaction = connection.BeginTransaction();
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = "DELETE FROM RareCard;";
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
                OnErrorOccurred?.Invoke($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Card.ToText())} {ex.Message}");
                return (false, ex.Message);
            }
        }

        public (bool, bool) ModifyRareCard(RareCard rareCard)
        {
            if (rareCard == null) return (false, false);

            bool isExisting = _rareCardsData.ContainsKey(rareCard.id);
            _rareCardsData[rareCard.id] = rareCard;
            return (true, !isExisting);
        }
        // Item 1: Result
        // Item 2: Add-True, Update:False
        public (bool, bool) ModifyRareCardByID(ulong cardID, RareCard newCard)
        {
            if (newCard == null) return (false, false);

            bool isExisting = _rareCardsData.ContainsKey(cardID);
            var newCardAdd = new RareCard { id = cardID, name = newCard.name, rare = newCard.rare };
            _rareCardsData[newCardAdd.id] = newCardAdd;

            OnDataChanged?.Invoke();
            return (true, !isExisting);
        }
        public bool AddRareCard(RareCard card)
        {
            if (card == null) return false;

            // Kiểm tra duplicate ID
            if (!_rareCardsData.ContainsKey(card.id))
            {
                _rareCardsData[card.id] = card;
                return true;
            }
            else
            {
                OnErrorOccurred?.Invoke($"Card with ID {card.id} already exists");
                return false;
            }
            // OnDataChanged?.Invoke();
        }
        public bool UpdateRareCard(RareCard oldCard, RareCard newCard)
        {
            if (oldCard == null || newCard == null) return false;

            if (_rareCardsData.ContainsKey(oldCard.id))
            {
                var newCardAdd = new RareCard { id = oldCard.id, name = newCard.name, rare = newCard.rare };
                _rareCardsData[oldCard.id] = newCardAdd;
            }
            else _rareCardsData[newCard.id] = newCard;
            return true;
        }
        public bool UpdateRareCardById(ulong cardId, RareCard newCard)
        {
            if (newCard == null) return false;

            if (_rareCardsData.ContainsKey(cardId))
            {
                var newCardAdd = new RareCard { id = cardId, name = newCard.name, rare = newCard.rare };
                _rareCardsData[cardId] = newCardAdd;
            }
            else _rareCardsData[newCard.id] = newCard;
            return true;

            //var index = _rareCardsData.FindIndex(c => c.id == cardId);
            //if (index >= 0)
            //{
            //    _rareCardsData[index] = newCard;
            //    OnDataChanged?.Invoke();
            //    return true;
            //}
            //return false;
        }

        public int ModifyMultipleRareCards(IEnumerable<RareCard> cards)
        {
            if (cards == null || !cards.Any()) return 0;

            int modifiedCount = 0;

            foreach(var item in cards)
            {
                if (item == null) continue;
                _rareCardsData[item.id] = item;
                modifiedCount++;
            }

            if (modifiedCount > 0)
            {
                OnDataChanged?.Invoke();
            }
            IsSaveRareCardListToDB = false;
            return modifiedCount;
        }
        public int AddMultipleRareCards(IEnumerable<RareCard> cards)
        {
            if (cards == null || !cards.Any()) return 0;

            int addedCount = 0;

            foreach(var card in cards)
            {
                if (card == null) continue;
                if (!RareCardsData.ContainsKey(card.id))
                {
                    _rareCardsData[card.id] = card;
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                OnDataChanged?.Invoke();
            }
            IsSaveRareCardListToDB = false;
            return addedCount;
        }

        public bool RemoveRareCardById(ulong cardId)
        {
            bool removed = _rareCardsData.Remove(cardId);
            //if (removed)
            //{
                // OnDataChanged?.Invoke();
            //}
            return removed;
        }
        public bool RemoveRareCard(RareCard card)
        {
            if (card == null) return false;
            return RemoveRareCardById(card.id);
        }
        public int RemoveMultipleRareCards(IEnumerable<RareCard> cards)
        {
            if (cards == null) return 0;
            return cards.Where(card => card!= null).Count(cards => _rareCardsData.Remove(cards.id));
        }
        public int RemoveAllRareCards()
        {
            int removedCount = _rareCardsData.Count;
            if (removedCount == 0) return 0;

            _rareCardsData.Clear();

            OnDataChanged?.Invoke();

            return removedCount;
        }

        public void ClearRareCards()
        {
            if (_rareCardsData.Count > 0)
            {
                _rareCardsData.Clear();
                OnDataChanged?.Invoke();
            }
        }
        #endregion

        #region Helper Methods
        public async Task<(bool, string)> LoadData()
        {
            try
            {
                if (!Directory.Exists(CardEditor.Models.AppContext.Instance.RaresFolderPath))
                    Directory.CreateDirectory(CardEditor.Models.AppContext.Instance.RaresFolderPath);

                await Task.WhenAll(CreateRaresListDataBase(), CreateRareCardDataBase());
                RareListSyncToUI();
                return (true,string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public void RareListSyncToUI(bool replaceInstance = false)
        {
            if (replaceInstance) RareItemsForUI = new BulkObservableCollection<RareItem>(_rareItemsData.Values);
            else
            {
                RareItemsForUI.Clear();
                RareItemsForUI.AddRange(_rareItemsData.Values);
            }
        }
        public long GetRareByCardId(ulong id)
        {
            return _rareCardsData.TryGetValue(id, out var card) ? card.rare : 0;
        }
        public RareCard FindRareCardById(ulong id)
        {
            return _rareCardsData.TryGetValue(id, out var card) ? card : null;
        }
        public List<RareCard> FindRareCardsByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return new List<RareCard>();
            return _rareCardsData.Values.Where(card => card.name != null && card.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }
        public List<RareCard> FindRareCardsByRare(long rare)
        {
            return _rareCardsData.Values.Where(card => card.rare == rare).ToList();
        }
        public int GetRareCardsCount() => _rareCardsData.Count;
        public int GetRareItemsCount() => _rareItemsData.Count;

        public bool HasRareCard(ulong id) => _rareCardsData.ContainsKey(id);
        public bool IsRareCardsEmpty() => _rareCardsData.Count == 0;
        public bool IsRareItemsEmpty() => _rareItemsData.Count == 0;

        public async Task<(bool,string)> BrowseDataCardDataBase(string filePath, bool Overwrite)
        {
            var tempList = new List<RareCard>();

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
                    
                    if (tableNames.Contains("rarecard"))
                    {
                        using (var command = new SQLiteCommand("SELECT id, name, rare FROM RareCard", connection))
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ulong id = reader.IsDBNull(0) ? 0UL : unchecked((ulong)Convert.ToUInt64(reader.GetValue(0)));
                                string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                                long rare = reader.IsDBNull(2) ? 0L : (long)reader.GetInt64(2);

                                var card = new RareCard
                                {
                                    id = id,
                                    name = name,
                                    rare = rare
                                };
                                tempList.Add(card);
                            }
                        }
                    }
                    else if (tableNames.Contains("texts"))
                    {
                        using (var command = new SQLiteCommand("SELECT id, name FROM texts", connection))
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ulong id = reader.IsDBNull(0) ? 0UL : unchecked((ulong)Convert.ToUInt64(reader.GetValue(0)));
                                string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);

                                var card = new RareCard
                                {
                                    id = id,
                                    name = name,
                                    rare = 0
                                };
                                tempList.Add(card);
                            }
                        }
                    }
                    else
                    {
                        OnErrorOccurred?.Invoke($"{CMess.CardDB.ToText()} {string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText())}");
                        return (false, $"{CMess.CardDB.ToText()} {string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText())}");
                    }
                }

                try
                {
                    if (MergeRareCards(tempList, Overwrite))
                    {
                        IsSaveRareCardListToDB = false;
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
                    OnErrorOccurred?.Invoke($"{string.Format(CMess.PlaceholderError.ToText(), CMess.Import.ToText())} {ex.Message}");
                    return (false, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Import.ToText())} {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Read.ToText())} {CMess.CardDB.ToText()} {ex.Message}");
                return (false, $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Read.ToText())} {CMess.CardDB.ToText()} {ex.Message}");
            }
        }
        private static JsonSerializerOptions CreateSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }
        public async Task<(bool,string)> BrowseDataCeds(string filePath, bool Overwrite)
        {
            try
            {
                string json = string.Empty;

                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    json = await reader.ReadToEndAsync();
                }

                var importedCards = JsonSerializer.Deserialize<List<CardEditor.Models.Card>>(json, SerializerOptions);

                if (importedCards != null && importedCards.Any())
                {
                    var newCards = importedCards.Select(card => new RareCard
                    {
                        id = (ulong)card.id,
                        name = card.name,
                        rare = 0
                    }).ToList();

                    try
                    {
                        if (MergeRareCards(newCards, Overwrite))
                        {
                            IsSaveRareCardListToDB = false;
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
                        OnErrorOccurred?.Invoke($"{string.Format(CMess.PlaceholderError.ToText(), CMess.Import.ToText())} {ex.Message}");
                        return (false, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Import.ToText())} {ex.Message}");
                    }

                }
                else
                {
                    OnErrorOccurred?.Invoke(CMess.noValiCardFound.ToText());
                    return (false, CMess.noValiCardFound.ToText());
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{string.Format(CMess.PlaceholderError.ToText(), CMess.Import.ToText())} {ex.Message}");
                return (false, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Import.ToText())} {ex.Message}");
            }
        }
        public async Task<(bool,string)> BrowseDataExcel(string filePath, bool Overwrite)
        {
            var tempList = new List<RareCard>();

            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                byte[] fileBytes = new byte[fileStream.Length];
                await fileStream.ReadAsync(fileBytes, 0, fileBytes.Length);

                using var package = new ExcelPackage(new MemoryStream(fileBytes));
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (!CheckDatabase.CheckExcelValidity(worksheet))
                {
                    OnErrorOccurred?.Invoke(string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()));
                    return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()));
                }

                int lastRow = worksheet.Dimension?.End.Row ?? 1;
                if (lastRow < 2)
                {
                    OnErrorOccurred?.Invoke($"{CMess.CardDB.ToText()} {CMess.noValiCardFound.ToText()}");
                    return (false, CMess.noValiCardFound.ToText());
                }

                for (int row = 2; row <= lastRow; row++)
                {
                    var idCell = worksheet.Cells[row, 1].Value;
                    if (idCell == null) continue;
                    if (!ulong.TryParse(idCell.ToString(), out ulong id)) continue;

                    string name = worksheet.Cells[row, 2].Text ?? "";

                    var card = new RareCard
                    {
                        id = id,
                        name = name,
                        rare = 0
                    };
                    tempList.Add(card);
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Read.ToText(), CMess.Excel.ToText(), CMess.File.ToText())} {ex.Message}");
                return (false, $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Read.ToText(), CMess.Excel.ToText(), CMess.File.ToText())} {ex.Message}");
            }

            try
            {
                if (MergeRareCards(tempList, Overwrite))
                {
                    IsSaveRareCardListToDB = false;
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
                OnErrorOccurred?.Invoke($"{string.Format(CMess.PlaceholderError.ToText(), CMess.Import.ToText())} {ex.Message}");
                return (false, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Import.ToText())} {ex.Message}");
            }
        }
        public async Task<(bool,string)> BrowseDataDeck(string filePath, bool Overwrite)
        {
            string[] lines;
            var newCards = new List<RareCard>();

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                using (var reader = new StreamReader(stream))
                {
                    var list = new List<string>();
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        list.Add(line);
                    }
                    lines = list.ToArray();
                }

                var filteredLines = lines
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .Where(line =>
                    !(line.StartsWith("#main", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("#extra", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("!side", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("#created", StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                int hashLineCount = filteredLines.Count(line => line.StartsWith("#"));
                bool includesCardName = hashLineCount >= 2;

                if (includesCardName)
                {
                    string currentCardName = null;
                    foreach (var line in filteredLines)
                    {
                        if (line.StartsWith("#"))
                        {
                            currentCardName = line.Substring(1).Trim();
                        }
                        else if (ulong.TryParse(line, out ulong cardId))
                        {
                            newCards.Add(new RareCard
                            {
                                id = cardId,
                                name = currentCardName ?? string.Empty,
                                rare = 0
                            });
                            currentCardName = null;
                        }
                    }
                }
                else
                {
                    foreach (var line in filteredLines)
                    {
                        if (ulong.TryParse(line, out ulong cardId))
                        {
                            newCards.Add(new RareCard
                            {
                                id = cardId,
                                name = string.Empty,
                                rare = 0
                            });
                        }
                    }
                }

                try
                {
                    if (MergeRareCards(newCards, Overwrite))
                    {
                        IsSaveRareCardListToDB = false;
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
                    OnErrorOccurred?.Invoke($"{string.Format(CMess.PlaceholderError.ToText(), CMess.Import.ToText())} {ex.Message}");
                    return (false, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Import.ToText())} {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{string.Format(CMess.PlaceholderError.ToText(), CMess.Import.ToText())} {ex.Message}");
                return (false,ex.Message);
            }
        }

        private bool MergeRareCards(IEnumerable<RareCard> cards, bool Overwrite)
        {
            if (cards == null) return false;

            bool changed = false;

            if (Overwrite)
            {
                foreach (var card in cards)
                {
                    if (card == null) continue;
                    _rareCardsData[card.id] = card;
                    changed = true;
                }
            }
            else
            {
                foreach(var card in cards)
                {
                    if (card == null) continue;
                    if (!_rareCardsData.ContainsKey(card.id))
                    {
                        _rareCardsData[card.id] = card;
                        changed = true;
                    }
                }
            }
            return changed;
        }

        public string GetRareItemNamesFromCode(long codeInput, string separa = ", ")
        {
            if (codeInput <= 0) return string.Empty;

            var matchedNames = RareItemsForUI
                .Where(item => item.Code > 0 && (codeInput & item.Code) == item.Code)
                .Select(item => item.Name)
                .ToList();

            return string.Join(separa, matchedNames);
        }
        #endregion

        #region Image Generation
        public async Task<bool> LoadImageCache()
        {
            var loadTasks = new List<Task<bool>>
            {
                LoadRarityCache()
            };
            var results = await Task.WhenAll(loadTasks);
            bool allTasksSucceeded = results.All(result => result);
            IsLoadedImageRareCache = allTasksSucceeded;
            return allTasksSucceeded;
        }
        private async Task<bool> LoadRarityCache()
        {
            if (string.IsNullOrWhiteSpace(CardAppContext.Instance.RaresListDBPath) ||
                !File.Exists(CardAppContext.Instance.RaresListDBPath)) return false;

            try
            {
                foreach (var item in RarityCache)
                {
                    item.Value?.Dispose();
                }
                RarityCache.Clear();

                var validEntries = new List<KeyValuePair<long, string>>();
                using (var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.RaresListDBPath};Version=3;"))
                {
                    await connection.OpenAsync();
                    string query = "SELECT Code, ImagePath FROM RareList";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            long code = reader.GetInt64(0);
                            string imagePath = reader.IsDBNull(1) ? null : reader.GetString(1);
                            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath) && imagePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                            {
                                validEntries.Add(new KeyValuePair<long, string>(code, imagePath));
                            }
                        }
                    }
                }
                var tasks = validEntries.Select(entry => LoadImageAsync(entry.Value));
                var bitmaps = await Task.WhenAll(tasks);

                var combined = validEntries
                    .Zip(bitmaps, (entry, bitmap) => new { Code = entry.Key, Bitmap = bitmap })
                    .Where(x => x.Bitmap != null)
                    .OrderByDescending(x => x.Code)
                    .Select(x => new KeyValuePair<long, SKBitmap>(x.Code, x.Bitmap))
                    .ToList();

                RarityCache.AddRange(combined);
                OnPropertyChanged();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading rarity cache: {ex.Message}");
                return false;
            }
        }
        private async Task<SKBitmap> LoadImageAsync(string imagePath)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using (var stream = new SKFileStream(imagePath))
                    {
                        return SKBitmap.Decode(stream);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading image {imagePath}: {ex.Message}");
                return null;
            }
        }
        public bool SetRarityLabelRect()
        {
            try
            {
                int stampPosition = ConfigViewModel.Instance.imageSetting.StampPosition;

                System.Drawing.Size stampSize = ConfigViewModel.Instance.imageSetting.StampSize;
                System.Drawing.Point stampMargin = ConfigViewModel.Instance.imageSetting.StampMarrgin;
                int stampPositionX, stampPositionY;

                switch (stampPosition)
                {
                    case 1:
                        stampPositionX = stampMargin.Y;
                        stampPositionY = stampMargin.X;
                        break;

                    case 2:
                        stampPositionX = GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth - (stampSize.Width + stampMargin.Y);
                        stampPositionY = stampMargin.X;
                        break;

                    case 3:
                        stampPositionX = stampMargin.Y;
                        stampPositionY = GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight - (stampSize.Height + stampMargin.X);
                        break;

                    case 4:
                        stampPositionX = GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth - (stampSize.Width + stampMargin.Y);
                        stampPositionY = GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight - (stampSize.Height + stampMargin.X);
                        break;

                    case 5:
                        stampPositionX = (GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth - stampSize.Width) / 2;
                        stampPositionY = (GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight - stampSize.Height) / 2;
                        break;

                    default:
                        stampPositionX = GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth - (stampSize.Width + stampMargin.Y);
                        stampPositionY = GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight - (stampSize.Height + stampMargin.X);
                        break;
                }

                GeneraImageViewModel.Instance.CurrentImageInfo.RarityLabelRect = new SKRect(
                    stampPositionX,
                    stampPositionY,
                    stampPositionX + stampSize.Width,
                    stampPositionY + stampSize.Height);
                IsLoadedImageRareRect = true;
                return true;
            }
            catch
            {
                GeneraImageViewModel.Instance.CurrentImageInfo.RarityLabelRect = new SKRect(
                    GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth - 350,
                    GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight - 100,
                    GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth,
                    GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight);
                IsLoadedImageRareRect = true;
                return true;
            }
        }
        private (int W, int H) ParseSize(string value, int defaultW, int defaultH)
        {
            var parts = value.Split(',');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int w) &&
                int.TryParse(parts[1], out int h))
            {
                return (w, h);
            }
            return (defaultW, defaultH);
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
            RareItemsForUI.Clear();
            RarityCache.Clear();
            _rareItemsData.Clear();
            _rareCardsData.Clear();
            _rareItemsData = null;
            _rareCardsData = null;

            PropertyChanged = null;
            OnDataChanged = null;
            OnErrorOccurred = null;
        }
        #endregion
    }
}
