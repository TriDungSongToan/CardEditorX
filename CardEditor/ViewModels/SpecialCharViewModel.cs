using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Models;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using CardAppContext = CardEditor.Models.AppContext;

namespace CardEditor.ViewModels
{
    public class SpecialCharViewModel : IDisposable
    {
        private static readonly Lazy<SpecialCharViewModel> _instance = new Lazy<SpecialCharViewModel>(() => new SpecialCharViewModel());
        public static SpecialCharViewModel Instance => _instance.Value;

        #region Data Store
        private List<CharacterItem> _charItems = new();
        private List<TagItem> _tagItems = new();
        public IReadOnlyList<CharacterItem> CharItems => _charItems;
        public IReadOnlyList<TagItem> TagItems => _tagItems;

        public void SetCharItems(List<CharacterItem> charItems)
        {
            if (charItems == null) return;
            Interlocked.Exchange(ref _charItems, charItems);
        }
        public void SetTagItems(List<TagItem> tagItems)
        {
            if (tagItems == null) return;
            Interlocked.Exchange(ref _tagItems, tagItems);
        }
        #endregion

        #region Constructor
        public SpecialCharViewModel()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            _options.Converters.Add(new JsonStringEnumConverter());
        }
        #endregion

        #region Load

        #region Char
        private readonly JsonSerializerOptions _options;
        public async Task<(bool success, string message)> SaveAsync(List<CharacterItem> items, string fullPath)
        {
            try
            {
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

                var data = new CharacterDataFile
                {
                    Version = 1,
                    Items = items
                };

                using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                await JsonSerializer.SerializeAsync(stream, data, _options);
                return (true, "Saved successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(List<CharacterItem> data, string message)> LoadAsync(string fullPath)
        {
            try
            {
                if (!File.Exists(fullPath)) return (null, "File does not exist");

                using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);

                var fileData = await JsonSerializer.DeserializeAsync<CharacterDataFile>(stream, _options);

                if (fileData == null) return (null, "File is empty or invalid");

                foreach (var item in fileData.Items)
                {
                    if (item == null) continue;
                    item.Metadata.SubCategory = item.Metadata.SubCategory.Trim().ToLowerInvariant();
                }

                // xử lý version sau này
                if (fileData.Version != 1)
                {
                    // migrate nếu cần
                }

                return (fileData.Items, "Loaded successfully");
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
        public async Task<(bool Success, string Message)> LoadChar()
        {
            string lag = ConfigViewModel.Instance.userSetting.Language;
            if (string.IsNullOrWhiteSpace(lag)) return (false, $"{string.Format(CMess.PlaceholderInva.ToText(), CMess.Setting.ToText())}");
            string filePath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, $@"CardData\Language\{lag}\SpecialCharacters.json");
            if (!System.IO.File.Exists(filePath)) return (false, $"{CMess.fileNotExit.ToText()} SpecialCharacters.json");
            try
            {
                var result = await LoadAsync(filePath);
                if (result.data == null) return (false, result.message);

                SetCharItems(result.data);

                var resultTag = ExtractTags(result.data);
                SetTagItems(resultTag.ToList());

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region Tag
        public async Task<(List<TagItem>, string)> ReadLinesAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath)) return (null, CMess.fileNotExit.ToText());

                var result = new List<TagItem>();

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    string line;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            result.Add(new TagItem
                            {
                                Name = line
                            });
                        }
                    }
                }

                return (result, string.Empty);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
        public async Task<(bool, string)> WriteLinesAsync(List<TagItem> items, string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (items == null) return (false, "Item không tồn tại");

                var directory = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    foreach (var item in items)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!string.IsNullOrWhiteSpace(item.Name))
                        {
                            await writer.WriteLineAsync(item.Name);
                        }
                    }

                    await writer.FlushAsync();
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public HashSet<TagItem> ExtractTags(IEnumerable<CharacterItem> items)
        {
            var tagStrings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                if (item.Metadata?.Tags == null) continue;

                foreach (var tagItem in item.Metadata.Tags)
                {
                    var tag = tagItem.Name?.Trim();

                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        tagStrings.Add(tag);
                    }
                }
            }
            return tagStrings.Select(t => new TagItem { Name = t }).ToHashSet();
        }
        #endregion

        #endregion

        #region Search
        public IEnumerable<CharacterItem> Filter(CharacterFilter filter)
        {
            var query = CharItems.AsEnumerable();

            // 1. Filter by Group
            if (filter.Group.HasValue && filter.Group.Value != CharacterGroup.All)
            {
                query = query.Where(x => x.Metadata.Group == filter.Group.Value);
            }

            // 2. Filter by SubCategory (case-insensitive)
            if (!string.IsNullOrWhiteSpace(filter.SubCategory))
            {
                query = query.Where(x =>
                    x.Metadata.SubCategory != null &&
                    x.Metadata.SubCategory.IndexOf(
                        filter.SubCategory,
                        StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // 3. Filter by Tags (item phải có ít nhất 1 tag khớp)
            if (filter.Tags != null && filter.Tags.Count > 0)
            {
                var tagSet = filter.Tags
                    .Select(t => t.ToLowerInvariant())
                    .ToHashSet(); // O(1) lookup

                query = query.Where(x =>
                    x.Metadata.Tags != null &&
                    x.Metadata.Tags.Any(t =>
                        tagSet.Contains(t.Name.ToLowerInvariant())));
            }

            // 4. Search text trên Description (và cả Character)
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var text = filter.SearchText.Trim();

                query = query.Where(x =>
                    (!string.IsNullOrEmpty(x.Description) &&
                     x.Description.IndexOf(
                         text,
                         StringComparison.OrdinalIgnoreCase) >= 0)
                    ||
                    (!string.IsNullOrEmpty(x.Character) &&
                     x.Character.IndexOf(
                         text,
                         StringComparison.OrdinalIgnoreCase) >= 0));
            }

            return query;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Interlocked.Exchange(ref _charItems, new List<CharacterItem>());
            Interlocked.Exchange(ref _tagItems, new List<TagItem>());
        }
        #endregion
    }
}
