using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardEditor.Services
{
    public interface IRequireMainWindowService
    {
        void SetMainWindowService(IMainWindowService service);
    }
    public interface IMainWindowService
    {
        (bool, string) SelectFileToOpen(IReadOnlyList<string> listFilePath, string rootFolder1, string rootFolder2 = null);

        Task OpenDataEditorTab(IEnumerable<string> filePaths, ulong id);
        Task OpenDataEditorTab(string filePath, ulong id);
        Task OpenDataEditorTab(ulong id);

        Task OpenCodeEditorTab(IEnumerable<string> filePaths);
        Task OpenCodeEditorTab(string filePath);
        Task OpenCodeEditorTab(ulong id);

        void OpenFinterSetting();
        void OpenCreateImage();
        void OpenViewImage(string ImageUrl);
        void OpenFileLocation(string ImageUrl);

        void UpdateWindowTitle(string title);
        void UpdateTabItemHeader(string title);

        void ImportDataCreateNewDataEdit(IEnumerable<CardEditor.Models.Card> importedCards);
        void ImportDataCreateNewBanListEdit(IEnumerable<CardEditor.Models.CardBanList> importedCards);


        Task OpenKonamiDB(ulong id, string name);
        Task OpenYugipedia(ulong id, string name);
        Task OpenYGOResources(ulong id, string name);
    }
}
