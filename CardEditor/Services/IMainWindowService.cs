using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Models;

namespace CardEditor.Services
{
    public interface IRequireMainWindowService
    {
        void SetMainWindowService(IMainWindowService service);
    }
    public interface IMainWindowService
    {
        void StartLoading();
        Task FinishLoading();

        Task OpenArchiveCommand(string url = null);
        Task OpenDatabaseCommand(string url = null);
        Task OpenScriptCommand(string url = null);
        Task OpenDeckCommand(string url = null);
        Task OpenBanListCommand(string url = null);


        void OpenArchive();
        void OpenImageEditor();
        void OpenDataEditor();
        void OpenOmegaDataEditor();
        void OpenDeckEditor();
        void OpenCodeEditor();
        void OpenBanListEditor();
        void OpenChatBot();
        void OpenScriptSupport();

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
        void UpdateTabItemHeader(UserControl control, string header);

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
