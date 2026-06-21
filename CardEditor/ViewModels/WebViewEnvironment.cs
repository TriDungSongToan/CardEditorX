using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace CardEditor.ViewModels
{
    public static class WebViewEnvironment
    {
        public static CoreWebView2Environment _environment;

        public static async Task<CoreWebView2Environment> GetAsync()
        {
            string appName = Assembly.GetEntryAssembly()!.GetName().Name!;
            if (_environment == null)
            {
                _environment = await CoreWebView2Environment.CreateAsync(userDataFolder : Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName, "WebView"));
            }
            return _environment;
        }
    }
}
