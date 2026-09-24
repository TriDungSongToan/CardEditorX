using CardEditor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardEditor.Services
{
    public interface IRequireMainWindowService
    {
        void SetMainWindowService(IMainWindowService service);
    }
    public interface IMainWindowService
    {
        (bool, List<CardEditor.Models.FileItem>) SelectFileToOpen(IReadOnlyList<(string fullPath, string archiveFilePath, string archiveEntryName)> entries);

        Task OpenDataEditorTab(IEnumerable<string> filePaths, ulong id);
        Task OpenDataEditorTab(string filePath, ulong id);
        Task OpenDataEditorTab(ulong id);

        Task OpenCodeEditorTab(IEnumerable<string> filePaths);
        Task OpenCodeEditorTab(string filePath, string archiveFilePath = null, string archiveEntryName = null);
        Task OpenCodeEditorTab(CardOmegaScript omegaScript);
        Task OpenCodeEditorTab(ulong id);

        void OpenFinterSetting();
        void OpenCreateImage();
        void OpenViewImage(string ImageUrl);
        void OpenFileLocation(string ImageUrl);

        void UpdateWindowTitle(AppTitle title);
        void UpdateWindowSavedFlag(bool isSaved);

        void UpdateTabItemHeader(string title);

        void ImportDataCreateNewDataEdit(IEnumerable<CardEditor.Models.Card> importedCards);
        void ImportDataCreateNewDataEdit(IEnumerable<CardEditor.Models.CardOmega> importedCards);
        void ImportDataCreateNewBanListEdit(IEnumerable<CardEditor.Models.CardBanList> importedCards);

        void SettingCommand();

        Task OpenKonamiDB(ulong id, string name);
        Task OpenYugipedia(ulong id, string name);
        Task OpenYGOResources(ulong id, string name);


        void OpenPreViewDescWindowCard(CardEditor.Models.Card card, CardEditor.Models.PendulumLanguageRule rule);
        void OpenPreViewDescWindowCardOmega(CardEditor.Models.CardOmega card, CardEditor.Models.PendulumLanguageRule rule);
    }
}
