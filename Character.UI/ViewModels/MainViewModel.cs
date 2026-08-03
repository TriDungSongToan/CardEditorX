using System.Windows;
using Microsoft.Win32;
using Character.UI.Commands;
using Character.UI.Collections;
using Character.Core.Models;
using Character.Core.Services;
using Character.Core.Interfaces;

namespace Character.UI.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Propertys
        public BulkObservableCollection<CharacterItem> CharacterItems { get; set; } = new();
        private CharacterItem? _selectedCharItem;
        public CharacterItem? SelectedCharItem
        {
            get => _selectedCharItem;
            set
            {
                if (SetProperty(ref _selectedCharItem, value))
                {
                    OnSelectedCharItemChanged();
                    ModifyCharItemCommand?.RaiseCanExecuteChanged();
                    DeleteCharItemCommand?.RaiseCanExecuteChanged();
                }
            }

        }

        private string _character = string.Empty;
        public string Character
        {
            get => _character;
            set
            {
                if (SetProperty(ref _character, value))
                {
                    AddCharItemCommand?.RaiseCanExecuteChanged();
                    ModifyCharItemCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private string _desc = string.Empty;
        public string Desc
        {
            get => _desc;
            set => SetProperty(ref _desc, value);
        }

        public IEnumerable<CharacterGroup> CharacterGroups { get; }
        private CharacterGroup? _selectedGroup;
        public CharacterGroup? SelectedGroup
        {
            get => _selectedGroup;
            set => SetProperty(ref _selectedGroup, value);
        }

        private string _subCategory = string.Empty;
        public string SubCategory
        {
            get => _subCategory;
            set => SetProperty(ref _subCategory, value);
        }

        private BulkObservableCollection<TagItem> _availableTags = new();
        public BulkObservableCollection<TagItem> AvailableTags
        {
            get => _availableTags;
            set => SetProperty(ref _availableTags, value);
        }
        private BulkObservableCollection<TagItem> _selectedAvailableTags = new();
        public BulkObservableCollection<TagItem> SelectedAvailableTags
        {
            get => _selectedAvailableTags;
            set => SetProperty(ref _selectedAvailableTags, value);
        }
        /// <summary>
        /// ////////////////////
        /// </summary>
        public BulkObservableCollection<TagItem> TagListInput { get; set; } = new();
        private TagItem? _selectedTagInput;
        public TagItem? SelectedTagInput
        {
            get => _selectedTagInput;
            set
            {
                if (SetProperty(ref _selectedTagInput, value))
                {
                    TagInput = SelectedTagInput != null ? SelectedTagInput.Name : string.Empty;
                    ModifyTagCommand?.RaiseCanExecuteChanged();
                    RemoveTagCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private string? _tagInput = string.Empty;
        public string? TagInput
        {
            get => _tagInput;
            set
            {
                if (SetProperty(ref _tagInput, value))
                {
                    AddTagCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion

        #region Commands
        public RelayCommand? AddTagCommand { get; set; }
        public RelayCommand? ModifyTagCommand { get; set; }
        public RelayCommand? RemoveTagCommand { get; set; }
        public RelayCommand? ExportTagListToFileCommand { get; set; }
        public RelayCommand? ImportTagListFromFileCommand { get; set; }

        public RelayCommand? AddCharItemCommand { get; set; }
        public RelayCommand? ModifyCharItemCommand { get; set; }
        public RelayCommand? DeleteCharItemCommand { get; set; }

        public RelayCommand? ExportCharItemToJsonCommand { get; set; }
        public RelayCommand? ImportCharItemFromJsonCommand { get; set; }

        public RelayCommand? LoadTagListFromCharListCommand { get; set; }
        public RelayCommand? SynchronizeTagListCommand { get; set; }

        #endregion

        #region Constructor
        public MainViewModel()
        {
            CharacterGroups = Enum.GetValues(typeof(CharacterGroup)).Cast<CharacterGroup>().ToList();
            SelectedGroup = CharacterGroup.Punctuation;

            InitializeCommand();

            CharacterItems.CollectionChanged += CharacterItems_CollectionChanged;
        }

        private void CharacterItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ExportCharItemToJsonCommand?.RaiseCanExecuteChanged();
            ImportCharItemFromJsonCommand?.RaiseCanExecuteChanged();
        }

        private void InitializeCommand()
        {
            AddTagCommand = new RelayCommand(_ => AddTag(), _ => CanAddTag());
            ModifyTagCommand = new RelayCommand( _ => ModifyTag(), _ => SelectedTagInputNotNull());
            RemoveTagCommand = new RelayCommand(_ => RemoveTag(), _ => SelectedTagInputNotNull());
            ExportTagListToFileCommand = new RelayCommand(async _ => await ExportTagListToFile());
            ImportTagListFromFileCommand = new RelayCommand(async () => await ImportTagListFromFile());

            AddCharItemCommand = new RelayCommand(_ => AddCharItem(), _ => CanAddCharItem());
            ModifyCharItemCommand = new RelayCommand(_ => ModifiCharItem(), _ => CanAddCharItem() && SelectedCharItemNotNull());
            DeleteCharItemCommand = new RelayCommand(_ => RemoveCharItem(), _ => CanAddCharItem() && SelectedCharItemNotNull());


            ExportCharItemToJsonCommand = new RelayCommand(async _ => await ExportCharListToJson(), _ => CharListNotEmpty());
            ImportCharItemFromJsonCommand = new RelayCommand(async _ => await ImportCharListFromJson(), _ => CharacterItems != null);

            LoadTagListFromCharListCommand = new RelayCommand(_ => LoadTagListFromCharList());
            SynchronizeTagListCommand = new RelayCommand(_ => SynchronizeTagList());
        }
        #endregion

        #region Commands
        private void OnSelectedCharItemChanged()
        {
            if (SelectedCharItem == null) return;

            Character = SelectedCharItem.Character;
            Desc = SelectedCharItem.Description;

            var group = SelectedCharItem.Metadata.Group;
            if (CharacterGroups.Contains(group)) SelectedGroup = group;
            else SelectedGroup = null;

            SubCategory = SelectedCharItem.Metadata.SubCategory;

            List<TagItem> tags = new List<TagItem>();
            SelectedAvailableTags.Clear();
            foreach (var tag in SelectedCharItem.Metadata.Tags)
            {
                foreach (var AvailableItem in AvailableTags)
                {
                    if (AvailableItem.Name == tag.Name)
                        SelectedAvailableTags.Add(AvailableItem);

                }
            }
        }
        private void AddTag()
        {
            if (string.IsNullOrWhiteSpace(TagInput)) return;
            bool exists = TagListInput.Any(item => item.Name.Equals(TagInput, StringComparison.OrdinalIgnoreCase));
            if (exists) return;

            TagItem newItem = new TagItem { Name = TagInput };

            TagListInput.Add(newItem);
            TagInput = string.Empty;
        }
        private bool CanAddTag()
        {
            return !string.IsNullOrWhiteSpace(TagInput);
        }
        private void ModifyTag()
        {
            if (SelectedTagInput == null) return;
            if (TagInput == null) return;

            SelectedTagInput.Name = TagInput;
        }
        private void RemoveTag()
        {
            if (SelectedTagInput == null) return;
            TagListInput.Remove(SelectedTagInput);
            SelectedTagInput = null;
        }
        private bool SelectedTagInputNotNull()
        {
            return SelectedTagInput != null;
        }
        private async Task ExportTagListToFile()
        {
            SaveFileDialog saveFileDiaLog = new SaveFileDialog();
            saveFileDiaLog.Title = "Save File";
            saveFileDiaLog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            bool? result = saveFileDiaLog.ShowDialog();
            if (result == true)
            {
                string filePath = saveFileDiaLog.FileName;
                ITagInterface storage = new TagsService();
                var (resultWrite, messageWrite) = await storage.WriteLinesAsync(TagListInput.ToList(), filePath);
                if (resultWrite) MessageBox.Show("Export File Suc!", "Info", MessageBoxButton.OK);
                else MessageBox.Show($"An error occurred: {messageWrite}", "Error", MessageBoxButton.OK);
            }
        }
        private async Task ImportTagListFromFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Chọn file";
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = openFileDialog.FileName;
                ITagInterface storage = new TagsService();
                var (ResultListItem, messageItem) = await storage.ReadLinesAsync(filePath);
                if (ResultListItem == null)
                {
                    MessageBox.Show($"An error occurred: {messageItem}", "Error", MessageBoxButton.OK);
                    return;
                }
                TagListInput.ReplaceAll(ResultListItem);
            }
        }
        private void SynchronizeTagList()
        {
            AvailableTags.ReplaceAll(TagListInput);
        }
        private void LoadTagListFromCharList()
        {
            try
            {
                ITagInterface storage = new TagsService();
                HashSet<TagItem> tagList = storage.ExtractTags(CharacterItems);
                MessageBox.Show("Export TagList from Char List Suc");
                MessageBox.Show($"TagList has: {tagList.Count} Item");
                TagListInput.ReplaceAll(tagList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An Error Occurred: {ex.Message}", "Error", MessageBoxButton.OK);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private CharacterItem GetCurrentCharItem()
        {
            CharacterMetadata newMeta = new CharacterMetadata()
            {
                Group = SelectedGroup != null ? SelectedGroup.Value : CharacterGroup.All,
                SubCategory = SubCategory,
                Tags = SelectedAvailableTags.ToList()
            };

            CharacterItem newItem = new CharacterItem()
            {
                Character = Character,
                Metadata = newMeta,
                Description = Desc,
            };
            return newItem;
        }
        private void AddCharItem()
        {
            if (string.IsNullOrWhiteSpace(Character)) return;
            var newitem = GetCurrentCharItem();

            CharacterItems.Add(newitem);
        }
        private bool CanAddCharItem()
        {
            return !string.IsNullOrWhiteSpace(Character);
        }
        private void ModifiCharItem()
        {
            if (SelectedCharItem == null) return;
            if (string.IsNullOrWhiteSpace(Character)) return;

            SelectedCharItem.Character = Character;
            SelectedCharItem.Description = Desc;

            SelectedCharItem.Metadata.Group = SelectedGroup != null ? SelectedGroup.Value : CharacterGroup.All;
            SelectedCharItem.Metadata.SubCategory = SubCategory;
            SelectedCharItem.Metadata.Tags = SelectedAvailableTags.ToList();
        }
        private void RemoveCharItem()
        {
            if (SelectedCharItem == null) return;

            CharacterItems.Remove(SelectedCharItem);
        }
        private bool SelectedCharItemNotNull()
        {
            return SelectedCharItem != null;
        }

        private async Task ExportCharListToJson()
        {
            SaveFileDialog saveFileDiaLog = new SaveFileDialog();
            saveFileDiaLog.Title = "Save File";
            saveFileDiaLog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

            bool? result = saveFileDiaLog.ShowDialog();
            if (result == true)
            {
                string filePath = saveFileDiaLog.FileName;

                ICharacterInterface storage = new CharacterService();
                var (Result, Message) = await storage.SaveAsync(CharacterItems.ToList(), filePath);

                if (Result) MessageBox.Show("Export File Suc!", "Info", MessageBoxButton.OK);
                else MessageBox.Show($"An error occurred: {Message}", "Error", MessageBoxButton.OK);
            }
        }
        private async Task ImportCharListFromJson()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Chọn file";
            openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = openFileDialog.FileName;

                ICharacterInterface storage = new CharacterService();
                var (ResultListItem, MessageListItem) = await storage.LoadAsync(filePath);
                if (ResultListItem == null)
                {
                    MessageBox.Show($"An error occurred: {MessageListItem}", "Error", MessageBoxButton.OK);
                    return;
                }
                CharacterItems.AddRange(ResultListItem);
                MessageBox.Show($"Import thanh cong {CharacterItems.Count} item");
            }
        }
        private bool CharListNotEmpty()
        {
            return (CharacterItems != null && CharacterItems.Any());
        }

        #endregion

    }
}
