using System;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Models;
using CardEditor.Services;
using CardEditor.Collections;
using CardEditor.Localization;
using CardAppContext = CardEditor.Models.AppContext;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.ViewModels
{
    public class CreditsViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<CreditsViewModel> _instance = new Lazy<CreditsViewModel>(() => new CreditsViewModel());
        public static CreditsViewModel Instance => _instance.Value;

        #region Data Storage
        private Dictionary<int, CreditItem> _creditData = new();
        public IReadOnlyDictionary<int, CreditItem> CreditData => _creditData;
        private Dictionary<Card, string> _lastSnapshot;

        private BulkObservableCollection<CreditItem> _creditDataUI = new BulkObservableCollection<CreditItem>();
        public BulkObservableCollection<CreditItem> CreditDataUI
        {
            get => _creditDataUI;
            set
            {
                if (_creditDataUI != value)
                {
                    _creditDataUI = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoaded { get; private set; } = false;
        #endregion

        #region Credits Functions
        public async Task<(bool, string)> LoadData()
        {
            try
            {
                if (!Directory.Exists(CardEditor.Models.AppContext.Instance.CreditFolderPath))
                    Directory.CreateDirectory(CardEditor.Models.AppContext.Instance.CreditFolderPath);

                await Task.WhenAll(CreateCreditsDatabase());
                CreditListSyncToUI(true);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task CreateCreditsDatabase()
        {
            if (!File.Exists(CardAppContext.Instance.CreditDBPath))
            {
                var (resultCreate, messageCreate) = await Task.Run(() => CreateFileServices.CreateCreditsDatabase(CardAppContext.Instance.CreditFolderPath, "CreditDB.cdb"));
                if (!resultCreate)
                {
                    OnErrorOccurred?.Invoke($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Create.ToText(), CMess.CreditTeam.ToText())} {messageCreate}");
                    return;
                }
            }
            await LoadCreditsData();
        }
        public async Task LoadCreditsData()
        {
            try
            {
                _creditData.Clear();
                using (var connection = new SQLiteConnection($"Data Source={CardAppContext.Instance.CreditDBPath};Version=3;"))
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"SELECT Id, Name, Header, Footer, Description FROM Credit";
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var creditItem = new CreditItem
                                {
                                    Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                    Header = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    Footer = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                    Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                };
                                _creditData[creditItem.Id] = creditItem;
                            }
                        }
                    }
                }
                
                IsLoaded = true;
            }
            catch (Exception ex)
            {
                OnErrorOccurred?.Invoke($"{CMess.errorLoadDB.ToText()} {ex.Message}");
            }
        }
        public async Task<(bool, string)> ModifyCreditsDatabase(CreditItem item)
        {
            if (item == null)
                return (false, string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.CreditTeam.ToText()));

            try
            {
                using (var connection = new SQLiteConnection(
                    $"Data Source={CardAppContext.Instance.CreditDBPath};Version=3;"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"INSERT INTO Credit (Id, Name, Header, Footer, Description) VALUES (@Id, @Name, @Header, @Footer, @Description)
                        ON CONFLICT(Id) DO UPDATE SET
                        Name = excluded.Name,
                        Header = excluded.Header,
                        Footer = excluded.Footer,
                        Description = excluded.Description;";

                        command.Parameters.AddWithValue("@Id", item.Id);
                        command.Parameters.AddWithValue("@Name", item.Name ?? string.Empty);
                        command.Parameters.AddWithValue("@Header", item.Header ?? string.Empty);
                        command.Parameters.AddWithValue("@Footer", item.Footer ?? string.Empty);
                        command.Parameters.AddWithValue("@Description", item.Description ?? string.Empty);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                OnDataChanged?.Invoke();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.CreditTeam.ToText())} {ex.Message}");
            }
        }
        public (bool, bool) ModifyCreditItem(CreditItem item)
        {
            if (item == null) return (false, false);

            bool isExisting = _creditData.ContainsKey(item.Id);
            _creditData[item.Id] = item;
            CreditListSyncToUI(false);
            return (true, !isExisting);
        }

        public async Task<(bool, string)> RemoveCreditDatabase(CreditItem item)
        {
            return await RemoveCreditDatabase(item?.Id ?? 0);
        }
        public async Task<(bool, string)> RemoveCreditDatabase(int id)
        {
            if (id <= 0) return (false, "Invalid ID");

            try
            {
                using (var connection = new SQLiteConnection(
                    $"Data Source={CardAppContext.Instance.CreditDBPath};Version=3;"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM Credit WHERE Id = @Id";
                        command.Parameters.AddWithValue("@Id", id);

                        int rows = await command.ExecuteNonQueryAsync();

                        if (rows == 0)
                            return (false, "Item not found");

                        OnDataChanged?.Invoke();
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Error removing credit: {ex.Message}");
            }
        }
        public bool RemoveCreditItem(CreditItem item)
        {
            return RemoveCreditItem(item?.Id ?? 0);
        }
        public bool RemoveCreditItem(int id)
        {
            if (id <= 0) return false;

            bool removed = _creditData.Remove(id);

            if (!removed)
            {
                OnErrorOccurred?.Invoke($"Item with ID {id} not found");
                return false;
            }
            
            CreditListSyncToUI(false);
            return true;
        }

        public void CreditListSyncToUI(bool replaceInstance = false)
        {
            if (replaceInstance) CreditDataUI = new BulkObservableCollection<CreditItem>(_creditData.Values);
            else
            {
                CreditDataUI.Clear();
                CreditDataUI.AddRange(_creditData.Values);
            }
        }
        public void ClearCreditItems()
        {
            if (_creditData.Count > 0)
            {
                _creditData.Clear();
                CreditDataUI.Clear();
                OnDataChanged?.Invoke();
            }
        }
        #endregion

        #region Proces Desc
        private static readonly ParallelOptions DefaultParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 2) };
        public async Task<ResultItem> CreditTeam(IEnumerable<Card> cards, CreditItem credit, int writeMode, CancellationToken cancellationToken = default)
        {
            if (credit == null || string.IsNullOrWhiteSpace(credit.Header))
                return new ResultItem { Succeeded = false, Message = "Credit.Header không được trống." };

            var cardList = cards?.ToList() ?? new List<Card>();
            if (cardList.Count == 0)
                return new ResultItem { Succeeded = true, TotalCount = 0, FilteredCount = 0, Message = "Không có Card nào để xử lý." };

            // === TẠO SNAPSHOT TRƯỚC KHI THỰC HIỆN ===
            _lastSnapshot = CreateSnapshot(cardList);

            try
            {
                var result = await ProcessCreditInternal(cardList, credit, writeMode, cancellationToken);
                return result;
            }
            catch (OperationCanceledException)
            {
                Rollback(_lastSnapshot);
                return new ResultItem
                {
                    Succeeded = false,
                    Message = "Thao tác đã bị hủy bởi người dùng."
                };
            }
            catch (Exception ex)
            {
                Rollback(_lastSnapshot);
                // TODO: Log lỗi chi tiết nếu cần
                return new ResultItem
                {
                    Succeeded = false,
                    Message = $"Đã xảy ra lỗi: {ex.Message}"
                };
            }
        }
        private async Task<ResultItem> ProcessCreditInternal(List<Card> cardList, CreditItem credit, int writeMode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var creditItems = CreditData.Values.Where(c => !string.IsNullOrWhiteSpace(c.Header)).ToList();
            var headers = new HashSet<string>(creditItems.Select(c => c.Header?.Trim()), StringComparer.Ordinal);

            int modifiedCount = 0;

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 2),
                CancellationToken = token
            };

            switch (writeMode)
            {
                case 1: // Overwrite Duplicate
                    modifiedCount = await Task.Run(() => OverwriteDuplicate(cardList, credit, options), token);
                    break;

                case 2: // Overwrite All
                    modifiedCount = await Task.Run(() => OverwriteAll(cardList, creditItems, credit, options), token);
                    break;

                case 3: // Append
                    modifiedCount = await Task.Run(() => AppendWrite(cardList, credit, options), token);
                    break;

                case 4: // Skip
                    modifiedCount = await Task.Run(() => SkipWrite(cardList, headers, credit, options), token);
                    break;

                default:
                    return new ResultItem { Succeeded = false, Message = $"WriteMode không hợp lệ: {writeMode}" };
            }

            // Nếu thành công thì clear snapshot để giải phóng memory
            //_lastSnapshot = null;

            return new ResultItem
            {
                Succeeded = true,
                TotalCount = cardList.Count,
                FilteredCount = modifiedCount,
                Message = writeMode == 5
                    ? $"Đã xóa hết Credit trong {modifiedCount} Card."
                    : $"Đã xử lý {cardList.Count} Card, ghi Credit cho {modifiedCount} Card."
            };
        }

        /// <summary>
        /// Skip: Nếu desc đã có bất kỳ Credit nào → bỏ qua. Chưa có → append Credit mới.
        /// </summary>
        private int SkipWrite(List<Card> cards, HashSet<string> headers, CreditItem newCredit, ParallelOptions options)
        {
            int count = 0;
            Parallel.ForEach(cards, options, card =>
            {
                if (HasAnyCredit(card.desc, headers))
                    return;

                card.desc = AppendCreditBlockOptimized(card.desc, newCredit);
                Interlocked.Increment(ref count);
            });
            return count;
        }

        /// <summary>
        /// Appendwrite: Luôn append Credit mới, không quan tâm desc đã có Credit chưa.
        /// </summary>
        private int AppendWrite(List<Card> cards, CreditItem newCredit, ParallelOptions options)
        {
            int count = 0;
            Parallel.ForEach(cards, options, card =>
            {
                card.desc = AppendCreditBlockOptimized(card.desc, newCredit);
                Interlocked.Increment(ref count);
            });
            return count;
        }

        /// <summary>
        /// Overwrite Only Duplicate: Xoá toàn bộ instance có cùng Header với Credit mới,
        /// rồi append Credit mới. Nếu không có instance nào → vẫn append bình thường.
        /// </summary>
        private int OverwriteDuplicate(List<Card> cards, CreditItem newCredit, ParallelOptions options)
        {
            int count = 0;
            Parallel.ForEach(cards, options, card =>
            {
                string cleaned = RemoveCreditBlocks(card.desc, newCredit); // xóa tất cả instance của credit này
                card.desc = AppendCreditBlockOptimized(cleaned, newCredit);
                Interlocked.Increment(ref count);
            });
            return count;
        }

        /// <summary>
        /// Overwrite All: Xoá mọi Credit cũ thuộc bất kỳ CreditItem nào trong database,
        /// rồi append Credit mới.
        /// </summary>
        private int OverwriteAll(List<Card> cards, List<CreditItem> creditItems, CreditItem newCredit, ParallelOptions options)
        {
            int count = 0;
            Parallel.ForEach(cards, options, card =>
            {
                string cleaned = RemoveAllKnownCreditBlocks(card.desc, creditItems);
                card.desc = AppendCreditBlockOptimized(cleaned, newCredit);
                Interlocked.Increment(ref count);
            });
            return count;
        }

        /// <summary>
        /// Xoá mọi Credit cũ thuộc bất kỳ CreditItem nào trong database, không append Credit mới.
        /// </summary>
        public async Task<ResultItem> RemoveAllCredits(IEnumerable<Card> cards)
        {
            if (cards == null || cards.Count() == 0)
            {
                var emptyResult = new ResultItem
                {
                    Succeeded = false,
                    TotalCount = 0,
                    FilteredCount = 0,
                    Message = CMess.noCardFound.ToText()
                };
                return emptyResult;
            }

            var creditItems = CreditData.Values.Where(c => !string.IsNullOrEmpty(c.Header)).ToList();
            if (creditItems == null || creditItems.Count == 0)
            {
                var noCreditResult = new ResultItem
                {
                    Succeeded = true,
                    TotalCount = cards.Count(),
                    FilteredCount = 0,
                    Message = "Không có Credit nào trong database để xóa."
                };
            }

            int count = 0;

            Parallel.ForEach(cards, DefaultParallelOptions, card =>
            {
                string cleaned = RemoveAllKnownCreditBlocks(card.desc, creditItems);

                // Chỉ cập nhật nếu có thay đổi
                if (cleaned != card.desc)
                {
                    card.desc = cleaned;
                    Interlocked.Increment(ref count);
                }
            });

            var result = new ResultItem
            {
                Succeeded = true,
                TotalCount = cards.Count(),
                FilteredCount = count,
                Message = $"Đã xử lý {cards.Count()} Card, xóa Credit cho {count} Card."
            };
            return result;
        }

        private Dictionary<Card, string> CreateSnapshot(List<Card> cards)
        {
            var snapshot = new Dictionary<Card, string>(cards.Count);
            foreach (var card in cards)
            {
                snapshot[card] = card.desc ?? string.Empty;
            }
            return snapshot;
        }
        private void Rollback(Dictionary<Card, string> snapshot)
        {
            if (snapshot == null) return;

            foreach (var kv in snapshot)
            {
                kv.Key.desc = kv.Value;
            }
        }
        public (bool, string) PerformManualRollback()
        {
            try
            {
                Rollback(_lastSnapshot);
                _lastSnapshot = null;
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            
        }
        // ──────────────────────────────────────────────
        // Các hàm helper
        // ──────────────────────────────────────────────
        private static bool HasAnyCredit(string desc, HashSet<string> headers)
        {
            if (string.IsNullOrWhiteSpace(desc)) return false;

            return desc.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                       .Any(line => headers.Contains(line.Trim()));
        }
        private static string AppendCreditBlockOptimized(string desc, CreditItem credit)
        {
            if (string.IsNullOrWhiteSpace(desc))
                return BuildCreditBlock(credit);

            desc = desc.TrimEnd('\r', '\n', ' ');
            return desc + "\n" + BuildCreditBlock(credit);
        }
        private static string BuildCreditBlock(CreditItem credit)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(credit.Header);

            if (!string.IsNullOrEmpty(credit.Description))
                sb.Append('\n').Append(credit.Description);

            if (!string.IsNullOrEmpty(credit.Footer))
                sb.Append('\n').Append(credit.Footer);

            return sb.ToString();
        }
        private static string RemoveCreditBlocks(string desc, CreditItem credit)
        {
            if (string.IsNullOrWhiteSpace(desc) || string.IsNullOrWhiteSpace(credit?.Header))
                return desc;

            var lines = desc.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string headerTrim = credit.Header.Trim();

            for (int i = lines.Count - 1; i >= 0; i--)
            {
                if (string.Equals(lines[i].Trim(), headerTrim, StringComparison.Ordinal))
                {
                    int endIndex = FindBlockEnd(lines, i, credit.Footer);
                    lines.RemoveRange(i, endIndex - i + 1);
                }
            }

            return string.Join("\n", lines);
        }
        private static string RemoveAllKnownCreditBlocks(string desc, List<CreditItem> creditItems)
        {
            if (string.IsNullOrWhiteSpace(desc)) return desc;

            var lines = desc.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            for (int i = lines.Count - 1; i >= 0; i--)
            {
                var matchingCredit = creditItems.FirstOrDefault(c =>
                    string.Equals(lines[i].Trim(), c.Header?.Trim(), StringComparison.Ordinal));

                if (matchingCredit != null)
                {
                    int endIndex = FindBlockEnd(lines, i, matchingCredit.Footer);
                    lines.RemoveRange(i, endIndex - i + 1);
                }
            }

            return string.Join("\n", lines);
        }
        private static int FindBlockEnd(List<string> lines, int startIndex, string footer)
        {
            if (string.IsNullOrEmpty(footer))
                return lines.Count - 1; // Không có Footer → xóa hết từ Header

            string footerTrim = footer.Trim();

            // Tìm Footer đầu tiên sau Header
            for (int i = startIndex + 1; i < lines.Count; i++)
            {
                if (string.Equals(lines[i].Trim(), footerTrim, StringComparison.Ordinal))
                    return i;
            }

            // return startIndex => chỉ xóa Header nếu không tìm thấy Footer
            // return lines.Count - 1 => xóa hết từ Header đến cuối
            return startIndex;
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

        }
        #endregion

    }
}
