using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Constants;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.CreateFile
{
    public class CreateCedsService
    {
        public static async Task<CreateCardListResult> CreateCedsCommand(string filePath, CardListFormat format)
        {
            CreateCardListResult result = new();

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    result.Result = false;
                    result.Messenger = CMess.fileNotExit.ToText();
                    return result;
                }

                string cedsExtension = ConstantExtension.CedsExtensions.First();
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                string cedsFilePath;
                if (ConstantExtension.CedsExtensions.Contains(extension)) cedsFilePath = filePath;
                else cedsFilePath = Path.ChangeExtension(filePath, cedsExtension);

                string folderPath = System.IO.Path.GetDirectoryName(cedsFilePath);
                string cdbFileName = System.IO.Path.GetFileName(cedsFilePath);

                result = await Task.Run(() => CreateCeds(folderPath, cdbFileName, format));
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static CreateCardListResult CreateCeds(string folderPath, string cedsFileName, CardListFormat format)
        {
            CreateCardListResult result = new();

            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(cedsFileName))
            {
                result.Result = false;
                result.Messenger = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText());
                return result;
            }
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string cedsFilePath = Path.Combine(folderPath, cedsFileName);
            string tempCedsFilePath = cedsFilePath + ".tmp";

            try
            {
                JSONCeds ceds = new();
                ceds.Format = format;

                string json = JsonSerializer.Serialize(ceds, JsonStringHelper.SerializerCEDSOptions);
                File.WriteAllText(tempCedsFilePath, json, Encoding.UTF8);

                if (File.Exists(cedsFilePath)) File.Delete(cedsFilePath);
                File.Move(tempCedsFilePath, cedsFilePath);

                result.Result = true;
                result.Messenger = string.Empty;
                result.FilePath = cedsFilePath;
                result.Format = format;
                result.HasFlag = (format == CardListFormat.YGONoFlag) ? false : true;
            }
            catch (Exception ex)
            {
                if (File.Exists(tempCedsFilePath)) File.Delete(tempCedsFilePath);
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
    }
}
