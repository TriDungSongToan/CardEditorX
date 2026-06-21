using System;
using System.IO;
using System.Reflection;

namespace CardEditor.Helpers
{
    public static class RegistryHelper
    {
        public static Tuple<bool, string> CreateRegistry(string targetPath)
        {
            try
            {
                // string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CardEditor.exe");
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

[HKEY_CURRENT_USER\Software\Classes\.cdb]
@=""CardEditorX.Document""

[HKEY_CURRENT_USER\Software\Classes\.db]
@=""CardEditorX.Document""

[HKEY_CURRENT_USER\Software\Classes\.sqlite]
@=""CardEditorX.Document""

[HKEY_CURRENT_USER\Software\Classes\.ceds]
@=""CardEditorX.Document""

[HKEY_CURRENT_USER\Software\Classes\.lua]
@=""CardEditorX.Document""

[HKEY_CURRENT_USER\Software\Classes\.ydk]
@=""CardEditorX.Document""

[HKEY_CURRENT_USER\Software\Classes\.lflist.conf]
@=""CardEditorX.Document""

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
                return new Tuple<bool, string>(true, targetPath);
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string>(false, ex.Message);
            }
        }
    }
}
