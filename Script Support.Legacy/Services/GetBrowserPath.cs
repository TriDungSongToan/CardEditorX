using System;
using System.IO;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Policy;

namespace ScriptSupport.Legacy.Services
{
    internal class GetBrowserPath
    {
        private static string GetBrowser()
        {
            string browserPath = PropertiesSettingService.GetStringSetting("browserPath", "");
            if (!string.IsNullOrEmpty(browserPath) && File.Exists(browserPath))
            {
                return browserPath;
            }
            else
            {
                return null;
            }
        }

        public static void NavigateBrowser(string URLPath)
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
            }
            catch
            {
                MessageBox.Show("Không thể mở trình duyệt. Vui lòng kiểm tra lại đường dẫn.");
            }
        }

    }
}
