using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Constants;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.CreateFile
{
    public class CreateScriptService
    {
        public static async Task<CreateFileResult> CreateScriptCommand(string filePath, bool newScript)
        {
            CreateFileResult result = new();
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    result.Result = false;
                    result.Messenger = CMess.fileNotExit.ToText();
                    return result;
                }

                string scriptExtension = ConstantExtension.ScriptExtensions.First();
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                string scriptFilePath = ConstantExtension.ScriptExtensions.Contains(extension)
                    ? filePath : Path.ChangeExtension(filePath, scriptExtension);

                string folderPath = System.IO.Path.GetDirectoryName(scriptFilePath);
                string scriptFileName = System.IO.Path.GetFileName(scriptFilePath);

                result = await Task.Run(() => CreateScript(folderPath, scriptFileName, newScript));
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static CreateFileResult CreateScript(string folderPath, string fileName, bool newScript, string luaContent = "", LanguageArea Area = LanguageArea.Unknown)
        {
            CreateFileResult result = new();

            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(fileName))
            {
                result.Result = false;
                result.Messenger = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText());
                return result;
            }
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileName);
            string tempFilePath = filePath + ".tmp";

            try
            {
                using (StreamWriter writer = new StreamWriter(tempFilePath, false, new UTF8Encoding(false)))
                {
                    if (newScript && Regex.IsMatch(Path.GetFileName(filePath), @"^c\d+\.lua$", RegexOptions.IgnoreCase))
                    {
                        string safeLuaContent = (luaContent ?? "")
                            .Replace("--", "")
                            .Replace("\n", " ")
                            .Replace("\r", "");

                        string ocgComment = (Area == LanguageArea.OCG || Area == LanguageArea.Mixed) ? $"--{safeLuaContent}" : "-- add OCG card name";
                        string tcgComment = (Area == LanguageArea.TCG || Area == LanguageArea.Mixed) ? $"--{safeLuaContent}" : "-- add TCG card name";

                        writer.WriteLine(ocgComment);
                        writer.WriteLine(tcgComment);
                        writer.WriteLine("local s,id,o=GetID()");
                        writer.WriteLine("function s.initial_effect(c)");
                        writer.WriteLine("\t");
                        writer.WriteLine("end");
                    }
                }
                if (File.Exists(filePath)) File.Delete(filePath);
                File.Move(tempFilePath, filePath);
                result.Result = true;
                result.Messenger = string.Empty;
                result.FilePath = filePath;
            }
            catch (Exception ex)
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static string CreateScriptContent(string luaContent, LanguageArea Area = LanguageArea.Unknown)
        {
            string safeLuaContent = (luaContent ?? "")
                .Replace("--", "")
                .Replace("\n", " ")
                .Replace("\r", "");

            string ocgComment = (Area == LanguageArea.OCG || Area == LanguageArea.Mixed) ? $"--{safeLuaContent}" : "-- add OCG card name";
            string tcgComment = (Area == LanguageArea.TCG || Area == LanguageArea.Mixed) ? $"--{safeLuaContent}" : "-- add TCG card name";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(ocgComment);
            sb.AppendLine(tcgComment);
            sb.AppendLine("local s,id,o=GetID()");
            sb.AppendLine("function s.initial_effect(c)");
            sb.AppendLine("\t");
            sb.AppendLine("end");

            return sb.ToString();
        }

    }
}
