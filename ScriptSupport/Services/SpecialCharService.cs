using Character.Core.Models;
using Character.Core.Services;
using Character.Core.Interfaces;
using ScriptSupport.States;
using ScriptSupport.Stores;
using ScriptSupport.Interfaces;
using ScriptSupport.Environment;
using ScriptSupport.Localization;
using CMess = ScriptSupport.Localization.Language;

namespace ScriptSupport.Services
{
    public class SpecialCharService : ISpecialCharInterface
    {
        private readonly ConfigStore _config;
        private readonly AppEnvironment _aev;
        private readonly SpecialCharStore _store;
        private readonly FilterConfigState _filterState;

        public SpecialCharService(ConfigStore config, AppEnvironment aev,
            SpecialCharStore store, FilterConfigState filterState)
        {
            _config = config;
            _aev = aev;
            _store = store;
            _filterState = filterState;
        }

        #region Load
        public async Task<(bool Success, string Message)> LoadChar()
        {
            string lag = _config.UserSetting.Language;
            if (string.IsNullOrWhiteSpace(lag)) return (false, $"{string.Format(CMess.PlaceholderInva.ToText(), CMess.Setting.ToText())}");
            string filePath = System.IO.Path.Combine(_aev.DataFolderPath, $@"CardData\Language\{lag}\SpecialCharacters.json");
            if (!System.IO.File.Exists(filePath)) return (false, $"{CMess.fileNotExit.ToText()} SpecialCharacters.json");
            try
            {
                ICharacterInterface charInterface = new CharacterService();
                var result = await charInterface.LoadAsync(filePath);
                if (result.data == null) return (false, result.message);

                _store.SetCharItems(result.data);

                ITagInterface tagInterface = new TagsService();
                var resultTag = tagInterface.ExtractTags(result.data);
                _store.SetTagItems(resultTag.ToList());

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region Search
        public IEnumerable<CharacterItem> Filter(CharacterFilter filter)
        {
            var query = _store.CharItems.AsEnumerable();

            // 1. Filter by Group
            if (filter.Group.HasValue && filter.Group.Value != CharacterGroup.All)
            {
                query = query.Where(x => x.Metadata.Group == filter.Group.Value);
            }

            // 2. Filter by SubCategory (case-insensitive)
            if (!string.IsNullOrWhiteSpace(filter.SubCategory))
                query = query.Where(x =>
                    x.Metadata.SubCategory.Contains(
                        filter.SubCategory, StringComparison.OrdinalIgnoreCase));

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
                    x.Description.Contains(text, StringComparison.OrdinalIgnoreCase)
                    || x.Character.Contains(text, StringComparison.OrdinalIgnoreCase));
            }

            return query;
        }
        #endregion

    }
}
