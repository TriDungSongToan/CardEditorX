using System;
using System.Diagnostics;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public interface IFileExplorerService
    {
        void ShowFileInExplorer(string filePath);
    }
    public class FileExplorerService : IFileExplorerService
    {
        public void ShowFileInExplorer(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    string args = $"/select,\"{filePath}\"";
                    Process.Start("explorer.exe", args);
                }
                catch (Exception ex)
                {
                    // Có thể throw hoặc báo lỗi qua service khác
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }    
        }
    }
}
