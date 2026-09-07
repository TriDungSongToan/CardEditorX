using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Manager;
using CardEditor.Collections;
using CardAppContext = CardEditor.Models.AppContext;

namespace CardEditor.ViewModels
{
    public class SortsViewModel : IDisposable
    {
        private static readonly Lazy<SortsViewModel> _instance = new Lazy<SortsViewModel>(() => new SortsViewModel());
        public static SortsViewModel Instance => _instance.Value;

        public BulkObservableCollection<SelectedSortItem> SelectedSortItems { get; set; }

        public SortsViewModel()
        {
            SelectedSortItems = new BulkObservableCollection<SelectedSortItem>();
        }

        public async Task<(bool, string)> ReadSortingFileCommand()
        {
            return await LoadSortingFile();
        }
        public async Task<(bool, string)> LoadSortingFile(int attempt = 0)
        {
            const int MaxAttempts = 3;

            try
            {
                string filePath = Path.Combine(CardAppContext.Instance.ConfigFolderPath, "AppSorting.conf");
                if (File.Exists(filePath))
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        string jsonContent = await reader.ReadToEndAsync();

                        var sortData = JsonSerializer.Deserialize<List<SortItemData>>(jsonContent);
                        if (sortData == null)
                        {
                            return (false, "Failed to parse sorting file.");
                        }

                        foreach (var item in sortData)
                        {
                            var matchingSortItem = EnumsViewModel.Instance.SortCardItems
                                .FirstOrDefault(s => (int)s.Sort == item.Sort);

                            if (matchingSortItem != null)
                            {
                                SelectedSortItems.Add(new SelectedSortItem
                                {
                                    SelectedItem = matchingSortItem,
                                    OrderByAsc = item.OrderByAsc
                                });
                            }
                        }
                    }
                    return (true, string.Empty);
                }
                else
                {
                    if (attempt > MaxAttempts) return (false, "Unable to load or create sorting file after multiple attempts.");

                    var (result, message) = await ResetSortingFile();
                    if (result)
                    {
                        return await LoadSortingFile(attempt + 1);
                    }
                    else throw new Exception(message);
                    return (false, string.Empty);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public void UploadSortingList(List<SelectedSortItem> items)
        {
            if (SelectedSortItems == null)
                SelectedSortItems = new BulkObservableCollection<SelectedSortItem>();
            SelectedSortItems.Clear();
            foreach (var item in items)
            {
                SelectedSortItems.Add(item);
            }
        }

        public async Task <(bool, string)> SaveSortingFile()
        {
            var (result, message) = await SettingsManager.CreateSortFile(SelectedSortItems);
            if (result) return (true, string.Empty);
            else return (false, message);
        }
        public async Task<(bool, string)> ResetSortingFile()
        {
            BulkObservableCollection<SelectedSortItem> originalSorting = new BulkObservableCollection<SelectedSortItem>
            {
                new SelectedSortItem
                {
                    SelectedItem = new SortItem
                    {
                        Sort = SortType.ID,
                        Name = string.Empty,
                        DisplayName = string.Empty
                    },
                    OrderByAsc = true
                }
            };
            var (result, message) = await SettingsManager.CreateSortFile(originalSorting);
            if (result) return (true, string.Empty);
            else return (false, message);
        }

        public void Dispose()
        {
            SelectedSortItems.Clear();
        }
    }
}
