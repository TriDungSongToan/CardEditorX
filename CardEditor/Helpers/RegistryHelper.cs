using System;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Helpers
{
    public static class RegistryHelper
    {
        private const string ProgId = "CardEditorX.Document";
        private static readonly string[] DoubleClickExtensions =
            { ".ypk", ".cdb", ".ceds", ".lua", ".ydk" };
        private static readonly string[] OpenWithExtensions =
            { ".zip", ".db", ".sqlite", ".xlsx", ".txt", ".md", ".log", ".yml", ".conf" };

        public static (bool, string) CreateRegistry(string targetPath)
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                string escapedExePath = exePath.Replace(@"\", @"\\");

                string regContent = $@"Windows Registry Editor Version 5.00
[HKEY_CURRENT_USER\Software\Classes\CardEditorX.Document]
@=""CardEditorX Document""
""FriendlyTypeName""=""CardEditor X""

[HKEY_CURRENT_USER\Software\Classes\CardEditorX.Document\DefaultIcon]
@=""\""{escapedExePath}\"",0""

[HKEY_CURRENT_USER\Software\Classes\CardEditorX.Document\shell\open\command]
@=""\""{escapedExePath}\"" \""%1\""""

[HKEY_CURRENT_USER\Software\Classes\.ypk]
@=""CardEditorX.Document""

[HKEY_CURRENT_USER\Software\Classes\.cdb]
@=""CardEditorX.Document""

[HKEY_CURRENT_USER\Software\Classes\.ceds]
@=""CardEditorX.Document""

[HKEY_CURRENT_USER\Software\Classes\.lua]
@=""CardEditorX.Document""

[HKEY_CURRENT_USER\Software\Classes\.ydk]
@=""CardEditorX.Document""

[HKEY_CURRENT_USER\Software\Classes\.zip\OpenWithProgids]
""CardEditorX.Document""=""""

[HKEY_CURRENT_USER\Software\Classes\.db\OpenWithProgids]
""CardEditorX.Document""=""""

[HKEY_CURRENT_USER\Software\Classes\.sqlite\OpenWithProgids]
""CardEditorX.Document""=""""

[HKEY_CURRENT_USER\Software\Classes\.xlsx\OpenWithProgids]
""CardEditorX.Document""=""""

[HKEY_CURRENT_USER\Software\Classes\.txt\OpenWithProgids]
""CardEditorX.Document""=""""

[HKEY_CURRENT_USER\Software\Classes\.md\OpenWithProgids]
""CardEditorX.Document""=""""

[HKEY_CURRENT_USER\Software\Classes\.log\OpenWithProgids]
""CardEditorX.Document""=""""

[HKEY_CURRENT_USER\Software\Classes\.yml\OpenWithProgids]
""CardEditorX.Document""=""""

[HKEY_CURRENT_USER\Software\Classes\.conf\OpenWithProgids]
""CardEditorX.Document""=""""
";

                File.WriteAllText(targetPath, regContent);
                return (true, targetPath);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool, string) UnregisterRegistry()
        {
            try
            {
                var classesRoot = Registry.CurrentUser.OpenSubKey(@"Software\Classes", writable: true);
                if (classesRoot == null) return (true, CMess.unRegistrySuc.ToText());

                // 1. Xóa key double-click, CHỈ khi giá trị hiện tại vẫn là của mình
                foreach (var ext in DoubleClickExtensions)
                {
                    using var extKey = classesRoot.OpenSubKey(ext);
                    if (extKey != null && string.Equals(extKey.GetValue(null) as string, ProgId, StringComparison.OrdinalIgnoreCase))
                    {
                        classesRoot.DeleteSubKeyTree(ext, throwOnMissingSubKey: false);
                    }
                    // Nếu giá trị khác ProgId -> đã bị app khác ghi đè, bỏ qua, không đụng vào
                }

                // 2. Xóa từng value trong OpenWithProgids, KHÔNG xóa nguyên subkey
                //    (subkey OpenWithProgids có thể chứa entries của app khác)
                foreach (var ext in OpenWithExtensions)
                {
                    using var openWithKey = classesRoot.OpenSubKey($@"{ext}\OpenWithProgids", writable: true);
                    if (openWithKey != null && openWithKey.GetValue(ProgId) != null)
                    {
                        openWithKey.DeleteValue(ProgId, throwOnMissingValue: false);
                    }
                }

                // 3. Xóa định nghĩa ProgID của chính mình - key này chỉ mình sở hữu, an toàn xóa hẳn
                classesRoot.DeleteSubKeyTree(ProgId, throwOnMissingSubKey: false);

                return (true, CMess.unRegistrySuc.ToText());
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}