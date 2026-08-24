using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Models;

namespace CardEditor.Manager
{
    public static class SettingsManager
    {
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
