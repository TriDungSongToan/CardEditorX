using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ScriptSupport.Legacy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly string DataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await CheckDataFolder();

            MainWindow mainWindow;
            mainWindow = new MainWindow();
            mainWindow.Show();
            Application.Current.MainWindow = mainWindow;
        }

        private async Task CheckDataFolder()
        {
            //try
            //{
            //    // Tạo thư mục data nếu chưa tồn tại
            //    if (!Directory.Exists(DataFolder))
            //    {
            //        Directory.CreateDirectory(DataFolder);
            //    }
            //    string CardDatapath = Path.Combine(DataFolder, "CardData");
            //    if (!Directory.Exists(CardDatapath) || !Repository.IsValid(CardDatapath))
            //    {
            //        string CardDataURL = ConfigurationManager.AppSettings["CardDataURL"];
            //        bool hasUpdateCardData = await GitHubService.CheckForUpdatesAsync(CardDatapath, CardDataURL);

            //        if (hasUpdateCardData)
            //            MessageBox.Show("Update Complete!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error checking repository: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }
    }
}
