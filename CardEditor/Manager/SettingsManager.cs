using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Models;

namespace CardEditor.Manager
{
    public static class SettingsManager
    {
        public class SettingItem
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        public static async Task<(bool, string)> CreateSettingFile(IEnumerable<SettingItem> settings)
        {
            if (!Directory.Exists(CardEditor.Models.AppContext.Instance.ConfigFolderPath))
                Directory.CreateDirectory(CardEditor.Models.AppContext.Instance.ConfigFolderPath);

            string filePath = Path.Combine(CardEditor.Models.AppContext.Instance.ConfigFolderPath, "AppSetting.conf");

            try
            {
                if (File.Exists(filePath)) File.Delete(filePath);

                using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8)) // false = overwrite
                {
                    foreach (var setting in settings)
                    {
                        string line = $"{setting.Name} = {setting.Value}";
                        await writer.WriteLineAsync(line);
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static async Task<(bool, string)> CreateSetting()
        {
            var (success, error) = await CreateSettingFile(new List<SettingsManager.SettingItem>
            {
                // Folder path
                new SettingItem { Name = "DataSourcePath", Value = @"C:\ProjectIgnis" },
                new SettingItem { Name = "OriginalCardFolder", Value = "" },
                new SettingItem { Name = "ArtWordFolder", Value = "" },
                new SettingItem { Name = "OutPutFolder", Value = "" },

                // UI
                //new SettingItem { Name = "Background", Value = "#FF000000" },
                //new SettingItem { Name = "Foreground", Value = "#FFFFFFFF" },
                //new SettingItem { Name = "FontFamily", Value = "Consolas" },
                //new SettingItem { Name = "FontSize", Value = "12" },
                //new SettingItem { Name = "Theme", Value = "DeepPurple" },
                //new SettingItem { Name = "HighLight", Value = "Default" },
                //new SettingItem { Name = "FlowDirectionC", Value = "true" },
                //new SettingItem { Name = "TextAlignmentLeft", Value = "true" },
                //new SettingItem { Name = "ButtonChat", Value = "45,45" },
                //new SettingItem { Name = "WidthChat", Value = "545" },
                //new SettingItem { Name = "DeveloperEncrypted", Value = "" },

                // Image
                new SettingItem { Name = "ImageSize", Value = "1388,2026" },
                new SettingItem { Name = "StampSize", Value = "350,100" },
                new SettingItem { Name = "StampPosition", Value = "1" },
                new SettingItem { Name = "StampMargin", Value = "0,0" },
                new SettingItem { Name = "DPIImage", Value = "100" },
                new SettingItem { Name = "BackgroundArt", Value = "None" },
                new SettingItem { Name = "Foild", Value = "None" },
                new SettingItem { Name = "Series", Value = "0" },
                new SettingItem { Name = "Secret", Value = "0" },
                new SettingItem { Name = "Rare", Value = "0" },
                new SettingItem { Name = "RarityLabel", Value = "false" },

                new SettingItem { Name = "Language", Value = "English" },
                new SettingItem { Name = "Game", Value = "EDOPro" },
                new SettingItem { Name = "WriteMode", Value = "0" },
                new SettingItem { Name = "FilterMode", Value= "0" },
                new SettingItem { Name = "ScopePath", Value = "false" },
                new SettingItem { Name = "WordWrap", Value = "false" },
                new SettingItem { Name = "FoldCode", Value = "true" },
                new SettingItem { Name = "Advanced", Value = "5" },
                new SettingItem { Name = "Arrange", Value = "1" },
                
                new SettingItem {Name = "ConfirmClear", Value = "true" },
                new SettingItem {Name = "ConfirmDelete", Value = "true" },
                new SettingItem {Name = "ConfirmReSet", Value = "true" },
                new SettingItem {Name = "ConfirmReLoad", Value = "true" },

                new SettingItem { Name = "ApiEndpoint", Value = "" },
                new SettingItem { Name = "ApiKey", Value = "" },
                new SettingItem { Name = "UserName", Value = "User" },

                // Deck
                new SettingItem {Name = "MaxMainSize", Value = "60" },
                new SettingItem {Name = "MaxExtraSize", Value = "15" },
                new SettingItem {Name = "MaxSideSize", Value = "15" },
                new SettingItem {Name = "DeckEditor", Value = "62" },

            });
            return (success, error);
        }

        /// <summary>
        /// ///
        /// </summary>
        
        public static async Task<(bool, string)> CreateSortFile(IEnumerable<SelectedSortItem> sortItems)
        {
            if (!Directory.Exists(CardEditor.Models.AppContext.Instance.ConfigFolderPath))
                Directory.CreateDirectory(CardEditor.Models.AppContext.Instance.ConfigFolderPath);

            string filePath = Path.Combine(CardEditor.Models.AppContext.Instance.ConfigFolderPath, "AppSorting.conf");

            try
            {
                if (File.Exists(filePath)) File.Delete(filePath);

                sortItems = sortItems ?? Enumerable.Empty<SelectedSortItem>();

                if (sortItems.Any())
                {
                    var sortData = new List<SortItemData>();
                    foreach (var item in sortItems)
                    {
                        sortData.Add(new SortItemData
                        {
                            Sort = (int)item.SelectedItem.Sort,
                            OrderByAsc = item.OrderByAsc
                        });
                    }

                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(sortData, jsonOptions);

                    using (var writer = new StreamWriter(filePath, false))
                    {
                        await writer.WriteAsync(jsonString);
                    }
                }
                else
                {
                    using (var writer = new StreamWriter(filePath, false))
                    {
                        await writer.WriteAsync("[]");
                    }
                }
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
