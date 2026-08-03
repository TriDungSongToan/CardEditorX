using System;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CardEditor.Models;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.ViewModels
{
    public class DeckViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<DeckViewModel> _instance = new Lazy<DeckViewModel>(() => new DeckViewModel());
        public static DeckViewModel Instance => _instance.Value;

        public bool IsLoadedDecksList = false;
        public string DeckFolderPath = string.Empty;

        private BulkObservableCollection<CardEditor.Models.Deck> _decks = new BulkObservableCollection<CardEditor.Models.Deck>();
        public BulkObservableCollection<CardEditor.Models.Deck> Decks
        {
            get => _decks;
            set
            {
                if (_decks != value)
                {
                    _decks = value;
                    OnPropertyChanged(nameof(Decks));
                }
            }
        }

        private DeckViewModel()
        {

        }

        public async Task<bool> LoadDeckAsync()
        {
            string dataSourcePath = ConfigViewModel.Instance.userSetting.DataSource;
            if (string.IsNullOrWhiteSpace(dataSourcePath) || !System.IO.Directory.Exists(dataSourcePath))
            {
                return false;
            }

            var deckFolders = Directory.GetDirectories(dataSourcePath, "deck", SearchOption.AllDirectories);
            if (deckFolders.Length == 0)
            {
                string newDeckFolder = Path.Combine(dataSourcePath, "deck");
                Directory.CreateDirectory(newDeckFolder);
                DeckFolderPath = newDeckFolder;
                return false;
            }
            DeckFolderPath = deckFolders[0];
            var tempDecks = new List<Deck>();

            foreach (var deckFolder in deckFolders)
            {
                var ydkFiles = Directory.GetFiles(deckFolder, "*.ydk", SearchOption.TopDirectoryOnly);

                foreach (var file in ydkFiles)
                {
                    Deck deck = await LoadDeckFromFile(file);
                    tempDecks.Add(deck);
                }
            }
            Decks.AddRange(tempDecks);
            IsLoadedDecksList = true;
            return true;
        }
        public async Task<Deck> LoadDeckFromFile(string filePath, string archiveFilePath = null, string archiveEntryName = null)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return null;

            var mainIds = new List<ulong>();
            var extraIds = new List<ulong>();
            var sideIds = new List<ulong>();
            var currentList = mainIds;

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line.StartsWith("#extra", StringComparison.OrdinalIgnoreCase))
                        {
                            currentList = extraIds;
                            continue;
                        }
                        if (line.StartsWith("!side", StringComparison.OrdinalIgnoreCase))
                        {
                            currentList = sideIds;
                            continue;
                        }
                        if (ulong.TryParse(line, out ulong id))
                        {
                            currentList.Add(id);
                        }
                    }
                }

                var deck = new Deck { Path = filePath, archiveFilePath = archiveFilePath, archiveEntryName = archiveEntryName };
                deck.MainDeck = new List<CardInstance>(mainIds.Select(id => CardEXDataViewModel.Instance.AllCards.TryGetValue(id, out var card) ? card : null).Where(c => c != null).Select(card => new CardInstance { Card = card }));
                deck.ExtraDeck = new List<CardInstance>(extraIds.Select(id => CardEXDataViewModel.Instance.AllCards.TryGetValue(id, out var card) ? card : null).Where(c => c != null).Select(card => new CardInstance { Card = card }));
                deck.SideDeck = new List<CardInstance>(sideIds.Select(id => CardEXDataViewModel.Instance.AllCards.TryGetValue(id, out var card) ? card : null).Where(c => c != null).Select(card => new CardInstance { Card = card }));

                return deck;
            }
            catch
            {
                return null;
            }
        }

        public (List<ulong> MainIDs, List<ulong> ExtraIDs, List<ulong> SideIDs, string) GetListIDFromYDK(string input, string newName = null, string newPath = null)
        {
            if (string.IsNullOrEmpty(input)) return (null, null, null, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));

            try
            {
                List<ulong> mainIds = new List<ulong>();
                List<ulong> extraIds = new List<ulong>();
                List<ulong> sideIds = new List<ulong>();
                List<ulong> current = mainIds;

                using (var reader = new StringReader(input))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line)) continue;

                        if (line.StartsWith("#extra", StringComparison.OrdinalIgnoreCase))
                        {
                            current = extraIds;
                            continue;
                        }
                        if (line.StartsWith("!side", StringComparison.OrdinalIgnoreCase))
                        {
                            current = sideIds;
                            continue;
                        }
                        if (ulong.TryParse(line, out ulong id))
                        {
                            current.Add(id);
                        }
                    }
                }
                return (mainIds, extraIds, sideIds, string.Empty);
            }
            catch (Exception ex)
            {
                return (null, null, null, ex.Message);
            }
        }
        public (Deck, string) AddNewDeckMemoryFromListIDs(List<ulong> MainCardID, List<ulong> ExtraCardID, List<ulong> SideCardID,
            string newName = null, string newPath = null)
        {
            try
            {
                CardEditor.Models.Deck newDeck = new CardEditor.Models.Deck();
                if (!string.IsNullOrWhiteSpace(newName)) newDeck.Name = newName;
                if (!string.IsNullOrWhiteSpace(newPath)) newDeck.Path = newPath;
                newDeck.MainDeck = CardEXDataViewModel.Instance.GetListCardInstance(MainCardID);
                newDeck.ExtraDeck = CardEXDataViewModel.Instance.GetListCardInstance(ExtraCardID);
                newDeck.SideDeck = CardEXDataViewModel.Instance.GetListCardInstance(SideCardID);
                Decks.Add(newDeck);
                return (newDeck, string.Empty);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public (bool, string) CreateNewDeckMemory(string newName = null, string newPath = null)
        {
            try
            {
                CardEditor.Models.Deck newDeck = new CardEditor.Models.Deck();
                if (!string.IsNullOrWhiteSpace(newName)) newDeck.Name = newName;
                if (!string.IsNullOrWhiteSpace(newPath)) newDeck.Path = newPath;

                Decks.Add(newDeck);
                return (true, newDeck.Path);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool, string) CreateNewDeckFile(string fileName, string filePath = null, string folderPath = null)
        {
            string filePathFinal = string.Empty;

            if (!string.IsNullOrWhiteSpace(filePath)) filePathFinal = filePath;
            else
            {
                if (string.IsNullOrWhiteSpace(fileName)) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));
                if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath)) folderPath = DeckFolderPath;
                filePathFinal = Path.Combine(folderPath, fileName);
            }

            if (string.IsNullOrWhiteSpace(filePathFinal))
            {
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));
            }
            else
            {
                try
                {
                    using (File.Create(filePathFinal)) { }
                    return (true, filePathFinal);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        }

        public (bool, string) SaveDeckMemory(CardEditor.Models.Deck deck)
        {
            if (deck == null) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));

            try
            {
                var existingDeck = Decks.FirstOrDefault(d => d.Path == deck.Path);

                if (existingDeck == null)
                {
                    var (success, actualPath) = CreateNewDeckMemory(deck.Name, deck.Path);
                    if (!success) return (false, actualPath);

                    existingDeck = Decks.FirstOrDefault(d => d.Path == actualPath);
                    if (existingDeck == null) return (false, string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Create.ToText(), CMess.Deck.ToText()));
                }

                existingDeck.MainDeck = new List<CardInstance>(deck.MainDeck);
                existingDeck.ExtraDeck = new List<CardInstance>(deck.ExtraDeck);
                existingDeck.SideDeck = new List<CardInstance>(deck.SideDeck);

                return (true, existingDeck.Name);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task <(bool, string)> SaveDeckFile(CardEditor.Models.Deck deck,
            bool SaveCardName, string newPath = null)
        {
            if (deck == null) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));
            string filePath = string.Empty;

            if (!string.IsNullOrWhiteSpace(newPath)) filePath = newPath;
            else if (!string.IsNullOrWhiteSpace(deck.Path) && System.IO.File.Exists(deck.Path)) filePath = deck.Path;
            else
            {
                var (success, actualPath) = CreateNewDeckFile(deck.Name, deck.Path, null);
                if (!success) return (false, actualPath);
                filePath = actualPath;
            }
            if (string.IsNullOrWhiteSpace(filePath)) return (false, CMess.fileNotExit.ToText());

            StringBuilder stringDeck = new StringBuilder();
            stringDeck.AppendLine($"#created by {ConfigViewModel.Instance.userSetting.UserName}");

            if (SaveCardName)
            {
                stringDeck.AppendLine($"#main");
                foreach (var cardMain in deck.MainDeck)
                {
                    stringDeck.AppendLine($"# {cardMain.Card.BaseCard.name}");
                    stringDeck.AppendLine(cardMain.Card.ID.ToString());
                }
                stringDeck.AppendLine($"#extra");
                foreach (var cardExtra in deck.ExtraDeck)
                {
                    stringDeck.AppendLine($"# {cardExtra.Card.BaseCard.name}");
                    stringDeck.AppendLine(cardExtra.Card.ID.ToString());
                }
                stringDeck.AppendLine($"!side");
                foreach (var cardSide in deck.SideDeck)
                {
                    stringDeck.AppendLine($"# {cardSide.Card.BaseCard.name}");
                    stringDeck.AppendLine(cardSide.Card.ID.ToString());
                }
            }
            else
            {
                stringDeck.AppendLine($"#main");
                foreach (var cardMain in deck.MainDeck)
                {
                    stringDeck.AppendLine(cardMain.Card.ID.ToString());
                }
                stringDeck.AppendLine($"#extra");
                foreach (var cardExtra in deck.ExtraDeck)
                {
                    stringDeck.AppendLine(cardExtra.Card.ID.ToString());
                }
                stringDeck.AppendLine($"!side");
                foreach (var cardSide in deck.SideDeck)
                {
                    stringDeck.AppendLine(cardSide.Card.ID.ToString());
                }
            }
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    await writer.WriteAsync(stringDeck.ToString());
                }
                return (true, filePath);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool, string) DeleteDeckMemory(CardEditor.Models.Deck deck)
        {
            try
            {
                if (deck == null) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));
                if (!Decks.Contains(deck)) return (false, string.Empty);
                Decks.Remove(deck);
                return (true, deck.Name);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool, string) DeleteDeckMemory(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));

            var deck = Decks.FirstOrDefault(d => d.Path == path);
            if (deck == null) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));
            return DeleteDeckMemory(deck);
        }
        public (bool, string) DeleteDeckFile(CardEditor.Models.Deck deck)
        {
            if (deck == null) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));
            if (string.IsNullOrWhiteSpace(deck.Path)) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));
            return DeleteDeckFile(deck.Path);
        }
        public (bool, string) DeleteDeckFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return (true, path);
                }
                else
                {
                    return (false, CMess.fileNotExit.ToText());
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool success, string errorMessage) RenameDeckMemory(string oldPath, string newPath, string newName)
        {
            if (string.IsNullOrWhiteSpace(oldPath) ||
                string.IsNullOrWhiteSpace(newName) ||
                string.IsNullOrWhiteSpace(newName))
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));

            var existingDeck = Decks.FirstOrDefault(d => d.Path.Equals(oldPath, StringComparison.OrdinalIgnoreCase));
            if (existingDeck == null) return (false, CMess.fileNotExit.ToText());

            existingDeck.Path = newPath;
            existingDeck.Name = newName;

            return (true, existingDeck.Path);
        }
        public (bool success, string messageOrNewPath) ReNameDeckFile(string oldPath, string newName)
        {
            if (string.IsNullOrWhiteSpace(oldPath) ||
                !System.IO.File.Exists(oldPath) ||
                string.IsNullOrWhiteSpace(newName))
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));
            try
            {
                string directory = Path.GetDirectoryName(oldPath);
                string newPath = Path.Combine(directory, newName);

                if (File.Exists(newPath)) return (false, CMess.filealreadyExit.ToText());
                File.Move(oldPath, newPath);
                return (true, newPath);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public Deck FindDeckByPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            return Decks.FirstOrDefault(d => string.Equals(d.Path, path, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            if(Decks == null) return;
            foreach (var deck in Decks)
            {
                deck.MainDeck.Clear();
                deck.ExtraDeck.Clear();
                deck.SideDeck.Clear();
            }
            Decks.Clear();
            Decks = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
