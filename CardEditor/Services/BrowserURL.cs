using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Configuration;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public static class BrowserURL
    {
        private static string GetBrowser()
        {
            string browserPath = ConfigurationManager.AppSettings["browserPath"];
            if (!string.IsNullOrEmpty(browserPath) && File.Exists(browserPath))
            {
                return browserPath;
            }
            else
            {
                return null;
            }
        }
        public static (bool, string) NavigateBrowser(string URLPath)
        {
            string browserPathFinal = GetBrowser();
            try
            {
                if (!string.IsNullOrEmpty(browserPathFinal))
                {
                    Process.Start(browserPathFinal, URLPath);
                }
                else
                {
                    Process.Start(URLPath);
                }
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
