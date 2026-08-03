using System.Threading.Tasks;
using System.Collections.Generic;

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
        Task OpenCodeEditorTab(ulong id);

        void OpenFinterSetting();
        void OpenCreateImage();
        void OpenViewImage(string ImageUrl);
        void OpenFileLocation(string ImageUrl);

        void UpdateWindowTitle(string title);
        void UpdateWindowSavedFlag(bool isSaved);

        void UpdateTabItemHeader(string title);

        void ImportDataCreateNewDataEdit(IEnumerable<CardEditor.Models.Card> importedCards);
        void ImportDataCreateNewBanListEdit(IEnumerable<CardEditor.Models.CardBanList> importedCards);


        Task OpenKonamiDB(ulong id, string name);
        Task OpenYugipedia(ulong id, string name);
        Task OpenYGOResources(ulong id, string name);


        void OpenPreViewDescWindow(CardEditor.Models.Card card, CardEditor.Models.PendulumLanguageRule rule);
    }
}
