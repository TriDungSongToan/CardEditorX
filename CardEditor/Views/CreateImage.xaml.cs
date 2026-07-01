using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Manager;
using CardEditor.Services;
using CardEditor.ImageGene;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using CardAppContext = CardEditor.Models.AppContext;
using CardEditor.Models.Settings;
using CardEditor.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CardEditor.Commands;
using System.Text.RegularExpressions;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for CreateImage.xaml
    /// </summary>
    public partial class CreateImage : Window, INotifyPropertyChanged
    {
        public MainWindow MainWindowReference { get; set; }
        private bool isCreating = false;

        #region Property
        public CardEditor.Models.Settings.ImageSetting imageSetting { get; set; }
        public CardEditor.Models.Settings.ImageSettingSource imageSettingSource { get; set; }

        private int _scope = -1;
        public int Scope
        {
            get => _scope;
            set
            {
                if (_scope != value)
                {
                    _scope = value;
                    OnPropertyChanged(nameof(Scope));
                }
            }
        }
        #endregion

        #region Command
        public RelayCommand CreateImageCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand CacelCommand { get; private set; }
        public RelayCommand BrowseArtWorkFolder { get; private set; }
        public RelayCommand BrowseOutPutFolder { get; private set; }
        #endregion

        public CreateImage()
        {
            InitializeComponent();
            InitializeCommand();

            this.Opacity = 0;
            this.DataContext = this;
        }
        private void InitializeCommand()
        {
            CreateImageCommand = new CardEditor.Commands.RelayCommand(async _ => await CreateImgCommand(), _ => CanSave() && CheckScope());
            SaveCommand = new CardEditor.Commands.RelayCommand(async _ => await Save(), _ => CanSave());
            CacelCommand = new CardEditor.Commands.RelayCommand(_ => CancelCommand());
            BrowseArtWorkFolder = new CardEditor.Commands.RelayCommand(_ => BrowseArtWork());
            BrowseOutPutFolder = new CardEditor.Commands.RelayCommand(_ => BrowseOutPut());
        }

        #region Load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            imageSettingSource = new Models.Settings.ImageSettingSource();
            if (LoadSettingSource()) LoadSetting();
            this.Opacity = 1;
            UpdateRunnerPosition();
        }
        private bool LoadSettingSource()
        {
            try
            {
                var (resultSeries, messageSeries) = GeneraImageViewModel.Instance.LoadSeriesList();

                //List<string> Series = new List<string>();
                //string seriesFilePath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, $@"CardImage\Series.txt");
                //if (File.Exists(seriesFilePath))
                //{
                //    using (StreamReader sr = new StreamReader(seriesFilePath))
                //    {
                //        string line;
                //        while ((line = sr.ReadLine()) != null)
                //        {
                //            Series.Add(line);
                //        }
                //    }
                //}
                //else Series.Add("Series 10");
                //imageSettingSource.Series.AddRange(Series);

                string FoildFolderPath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath,
                    $@"CardImage\{ConfigViewModel.Instance.imageSetting.Series}\Foild");
                var listFoild = FindNameHelper.LoadNameArtList(FoildFolderPath);
                imageSettingSource.FoildLists.AddRange(listFoild);

                string backgroundArtFolderPath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath,
                    $@"CardImage\{ConfigViewModel.Instance.imageSetting.Series}\BackgroundArt");
                var listBGArt = FindNameHelper.LoadNameArtList(backgroundArtFolderPath);
                imageSettingSource.BackgroundArts.AddRange(listBGArt);

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void LoadSetting()
        {
            imageSetting = ConfigViewModel.Instance.imageSetting.Clone();
        }
        private void UpdateRunnerPosition()
        {
            double progress = progressBar.Value / progressBar.Maximum;
            double barWidth = progressBar.ActualWidth;

            double imageWidth = imgRunner.ActualWidth;
            double offset = barWidth * progress;

            runnerTransform.X = offset;
        }

        #endregion

        #region Save
        private async Task Save()
        {
            try
            {
                ConfigViewModel.Instance.imageSetting = imageSetting;
                var (result, message) = ConfigViewModel.Instance.SaveSingleSettingFile("ImageSetting", ConfigViewModel.Instance.imageSetting);
                GeneraImageViewModel.Instance.IsChangedSeries = false;
                GeneraImageViewModel.Instance.IsLoadedImageCache = false;
                RareRawDataViewModel.Instance.IsLoadedImageRareCache = false;
                RareRawDataViewModel.Instance.IsLoadedImageRareRect = false;
                ImageValidate.isValid = false;
                await Task.CompletedTask;
                ImageValidate.MarkDirty();

                var (resultReload, messageReload) = await ImageValidate.ReLoadImageData();
                if (!resultReload)
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Create.ToText(), CMess.Image.ToText())} {messageReload}", new[] { CMess.ok.ToText() });
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderInva.ToText(), CMess.Setting.ToText())}\n{ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private bool CanSave()
        {
            if (string.IsNullOrWhiteSpace(imageSetting.ArtworkFolder) || string.IsNullOrWhiteSpace(imageSetting.OutPutFolder)) return false;
            if (!System.IO.Path.IsPathRooted(imageSetting.ArtworkFolder) || !System.IO.Path.IsPathRooted(imageSetting.OutPutFolder)) return false;
            if (imageSetting.ArtworkFolder.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0 ||
                imageSetting.OutPutFolder.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0) return false;
            if (!Directory.Exists(imageSetting.OutPutFolder) || !Directory.Exists(imageSetting.OutPutFolder)) return false;
            if (!HasReadWritePermission(imageSetting.OutPutFolder) || !HasReadWritePermission(imageSetting.OutPutFolder)) return false;

            if (imageSetting.StampPosition < 0 || imageSetting.Series < 0 ||
                imageSetting.Rare < 0 || imageSetting.Secret < 0) return false;

            if (!CheckSize(imageSetting.ImageSizeString) ||
                !CheckSize(imageSetting.StampSizeString) ||
                !CheckSize(imageSetting.StampMarrginString)) return false;
            if (string.IsNullOrWhiteSpace(imageSetting.BackgroundArt) || string.IsNullOrWhiteSpace(imageSetting.Foild)) return false;
            
            return true;
        }
        private bool CheckSize(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && Regex.IsMatch(input, @"^[^,]+,[^,]+$");
        }
        private bool CheckScope()
        {
            if (Scope >= 0 || Scope <= 2) return true;
            return false;
        }
        private bool HasReadWritePermission(string folderPath)
        {
            string tempFilePath = System.IO.Path.Combine(folderPath, System.IO.Path.GetRandomFileName());
            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write))
                {
                    stream.WriteByte(0x0);
                }
                using (var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                {
                    int b = stream.ReadByte();
                }
                File.Delete(tempFilePath);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Create Image
        private async Task CreateImgCommand()
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                await Save();
                await Task.Delay(100);
                var (checkResult, checkMessage) = await ImageValidate.CheckValidate();
                if (!checkResult)
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                        $"{string.Format(CMess.PlaceholderInva.ToText(), CMess.Setting.ToText())} {checkMessage}", new[] { CMess.ok.ToText() });
                    return;
                }
                await CreateImg();
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Create.ToText(), CMess.Image.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        private async Task CreateImg()
        {
            isCreating = true;
            btnCancelCreateImg.IsEnabled = true;
            progressBar.Value = 0;
            UpdateRunnerPosition();
            grProgress.Height = double.NaN;

            var (result, message) = await MainWindowReference.CreateImageMainWindow(Scope, imageSetting.Series, (processed, total) =>
            {
                Dispatcher.Invoke(() =>
                {
                    progressBar.Maximum = total;
                    progressBar.Value = processed;
                    UpdateRunnerPosition();
                });
            });
            Mouse.OverrideCursor = null;
            if (result) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, message, new[] { CMess.ok.ToText() });
            else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, message, new[] { CMess.ok.ToText() });

            grProgress.Height = 0;
            isCreating = false;
            btnCancelCreateImg.IsEnabled = false;
        }
        #endregion

        #region Browse
        private void BrowseArtWork()
        {
            string folderPath = FileDiaLogHelper.OpenFolder(CMess.ArtWorkFolder.ToText());
            if (!string.IsNullOrWhiteSpace(folderPath) && Directory.Exists(folderPath))
            {
                imageSetting.ArtworkFolder = folderPath;
            }
        }
        private void BrowseOutPut()
        {
            string folderPath = FileDiaLogHelper.OpenFolder(CMess.OutPutFolder.ToText());
            if (!string.IsNullOrWhiteSpace(folderPath) && Directory.Exists(folderPath))
            {
                imageSetting.OutPutFolder = folderPath;
            }
        }

        #endregion

        private void CancelCommand()
        {
            this.Close();
        }

        #region Button
        private void blCreateImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void chkadvanfind_Checked(object sender, RoutedEventArgs e)
        {
            grAdvancedOption.Visibility = Visibility.Visible;
        }
        private void chkadvanfind_Unchecked(object sender, RoutedEventArgs e)
        {
            grAdvancedOption.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region TextBox
        private void txtArtWorkFolder_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (paths.Length > 0 && Directory.Exists(paths[0]))
                {
                    e.Effects = System.Windows.DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = System.Windows.DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            e.Handled = true;
        }
        private void txtArtWorkFolder_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (paths.Length > 0 && Directory.Exists(paths[0]))
                {
                    imageSetting.ArtworkFolder = paths[0];
                }
            }
        }
        private void txtOutPutFolder_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (paths.Length > 0 && Directory.Exists(paths[0]))
                {
                    e.Effects = System.Windows.DragDropEffects.Copy;
                }
                else
                {
                    e.Effects = System.Windows.DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            e.Handled = true;
        }
        private void txtOutPutFolder_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                if (paths.Length > 0 && Directory.Exists(paths[0]))
                {
                    imageSetting.OutPutFolder = paths[0];
                }
            }
        }
        private void Paste_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (System.Windows.Clipboard.ContainsText())
                {
                    string text = System.Windows.Clipboard.GetText();
                    if (text.StartsWith("\"") && text.EndsWith("\""))
                    {
                        text = text.Substring(1, text.Length - 2);

                        var textbox = sender as System.Windows.Controls.TextBox;
                        textbox.Text = text;
                        textbox.CaretIndex = text.Length;
                        e.Handled = true;
                    }
                }
            }
        }
        private async void btnCreateTest_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Value = 0;
            UpdateRunnerPosition();
            int totalDurationMs = 10000; // thời gian chạy
            int steps = 100;
            int delay = totalDurationMs / steps;
            grProgress.Height = double.NaN;

            for (int i = 1; i <= steps; i++)
            {
                await Task.Delay(delay);
                progressBar.Value = i;
                UpdateRunnerPosition();
            }

            grProgress.Height = 0;
        }
        private void btnCancelCreateImg_Click(object sender, RoutedEventArgs e)
        {
            if (isCreating)
            {
                MainWindowReference.CancelCreateImageMainWindow();
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
