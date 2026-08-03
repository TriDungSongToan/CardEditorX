using System.Windows;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using ScriptSupport.Views;
using ScriptSupport.Models;
using ScriptSupport.Interfaces;
using ScriptSupport.ViewModels;
using ScriptSupport.Localization;
using CMess = ScriptSupport.Localization.Language;

namespace ScriptSupport.Services
{
    public class DialogService : IDialogInterface
    {
        private readonly IServiceProvider _serviceProvider;
        public DialogService(IServiceProvider serviceProvider, IImageAppInterface imageCache)
        {
            _serviceProvider = serviceProvider;
            _imageCache = imageCache;
        }

        #region Window
        private static readonly Dictionary<Type, Type> _mappings = new();
        public static void Register<TViewModel, TView>() where TViewModel : BaseViewModel where TView : Window
        {
            _mappings[typeof(TViewModel)] = typeof(TView);
        }
        public void Show<TViewModel>() where TViewModel : BaseViewModel
        {
            var window = BuildWindow<TViewModel>();
            window.Show();
        }
        public bool? ShowDialog<TViewModel>() where TViewModel : BaseViewModel
        {
            var window = BuildWindow<TViewModel>();
            return window.ShowDialog();
        }
        public void Show<TViewModel, TParam>(TParam param)  where TViewModel : BaseViewModel, IInitializable<TParam>
        {
            var window = BuildWindow<TViewModel>();
            ((IInitializable<TParam>)window.DataContext!).Initialize(param);
            window.Show();
        }
        public bool? ShowDialog<TViewModel, TParam>(TParam param) where TViewModel : BaseViewModel, IInitializable<TParam>
        {
            var window = BuildWindow<TViewModel>();
            ((IInitializable<TParam>)window.DataContext!).Initialize(param);
            return window.ShowDialog();
        }
        private Window BuildWindow<TViewModel>() where TViewModel : BaseViewModel
        {
            var vmType = typeof(TViewModel);

            if (!_mappings.TryGetValue(vmType, out var windowType))
                throw new InvalidOperationException(
                    $"[DialogService] Chưa đăng ký mapping cho '{vmType.Name}'. " +
                    $"Gọi DialogService.Register<{vmType.Name}, YourWindow>() trong App.");

            var window = (Window)_serviceProvider.GetRequiredService(windowType);
            var viewModel = (TViewModel)_serviceProvider.GetRequiredService(vmType);
            window.DataContext = viewModel;
            window.Owner = Application.Current.MainWindow;
            return window;
        }
        #endregion

        #region MessageBox
        private readonly IImageAppInterface _imageCache;
        private async Task<ImageSource?> ResolveIcon(MessageBoxIconType? iconType)
        {
            if (iconType == null) return null;

            var appImage = iconType switch
            {
                MessageBoxIconType.Error => AppImage.Error,
                MessageBoxIconType.Warning => AppImage.Warning,
                MessageBoxIconType.Notification => AppImage.Notification,
                MessageBoxIconType.Information => AppImage.Information,
                MessageBoxIconType.Question => AppImage.Question,
                _ => AppImage.Information
            };

            return await _imageCache.Get(appImage);
        }
        public async Task<int> ShowMessage(MessageBoxRequest request)
        {
            var icon = await ResolveIcon(request.IconType);

            if (request.ResponseSource == null)
            {
                _ = Application.Current.Dispatcher.InvokeAsync(() =>
                    CMSG.Show(request.Title, icon, request.Message, request.Buttons, request.DefaultButtonIndex));
                return -1;
            }
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                int result = CMSG.Show(request.Title, icon, request.Message, request.Buttons, request.DefaultButtonIndex);
                request.ResponseSource.TrySetResult(result);
                return result;
            });
        }
        #endregion

        #region Open
        public string OpenFile(string title, params (string label, string pattern)[] filters)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                Title = title,
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            })
            {
                foreach (var f in filters)
                    dialog.Filters.Add(new CommonFileDialogFilter(f.label, f.pattern));

                return dialog.ShowDialog() == CommonFileDialogResult.Ok
                    ? dialog.FileName ?? string.Empty
                    : string.Empty;
            }
        }
        public IEnumerable<string> OpenFiles(string title, params (string label, string pattern)[] filters)
        {
            using (var dialog = new CommonOpenFileDialog()
            {
                Title = title,
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = true,
            })
            {
                foreach (var f in filters)
                    dialog.Filters.Add(new CommonFileDialogFilter(f.label, f.pattern));

                return dialog.ShowDialog() == CommonFileDialogResult.Ok
                    ? dialog.FileNames.Where(f => f is not null).Select(f => f!) : Enumerable.Empty<string>();
            }
        }
        public string OpenCardList()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.CardDB.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())} (*.cdb; *.db; *.sqlite)", "*.cdb;*.db;*.sqlite"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())} (*.ceds)", "*.ceds"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())} (*.xlsx)", "*.xlsx"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())} (*.lflist.conf)", "*.lflist.conf"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string OpenDataBase()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.CardDB.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())} (*.cdb; *.db; *.sqlite)", "*.cdb;*.db;*.sqlite"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())} (*.ceds)", "*.ceds"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())} (*.xlsx)", "*.xlsx"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string OpenDeck()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.Deck.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string OpenRare()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.File.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())} (*.cdb)", "*.cdb"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())} (*.ceds)", "*.ceds"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())} (*.xlsx)", "*.xlsx"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.ydk)", "*.ydk"));
        }
        public string OpenGenesys()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.File.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())} (*.cdb)", "*.cdb"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string OpenScript()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.File.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardScript.ToText())} (*.lua)", "*.lua"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Md.ToText())} (*.md)", "*.md"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Log.ToText())} (*.log)", "*.log"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Yaml.ToText())} (*.yml)", "*.yml"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())} (*.conf)", "*.conf"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public IEnumerable<string> OpenScripts()
        {
            return OpenFiles($"{CMess.Open.ToText()} {CMess.File.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardScript.ToText())} (*.lua)", "*.lua"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Md.ToText())} (*.md)", "*.md"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Log.ToText())} (*.log)", "*.log"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Yaml.ToText())} (*.yml)", "*.yml"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())} (*.conf)", "*.conf"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string OpenCeds()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.Ceds.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())} (*.ceds)", "*.ceds"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string OpenExcel()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.Excel.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())} (*.xlsx)", "*.xlsx"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string OpenLua(string filter = "")
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.File.ToText()} {CMess.cardlabelSetCode.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            string filterDiaLog = string.IsNullOrWhiteSpace(filter) ? CMess.cardlabelSetCode.ToText() : filter;
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{filterDiaLog} (*.lua)", "*.lua"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));

            return (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                ? openFileDialog.FileName ?? string.Empty
                : string.Empty;
        }
        public string OpenConf(string filter = "")
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.File.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            string filterDiaLog = string.IsNullOrWhiteSpace(filter) ? CMess.cardlabelSetCode.ToText() : filter;
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{filterDiaLog} (*.conf)", "*.conf"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));

            return (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                ? openFileDialog.FileName ?? string.Empty
                : string.Empty;
        }
        public string OpenBanList()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.File.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())} (*.lflist.conf)", "*.lflist.conf"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string OpenImage()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.File.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Image.ToText())} (*.png, *.jpg, *.jpeg, *.bmp)", "*.jpg;*.jpeg;*.png;*.bmp"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string OpenVideo()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.File.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Video.ToText())} (*.mp4, *.avi)", "*.mp4;*.avi"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string OpenText()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.File.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string OpenFile()
        {
            return OpenFile($"{CMess.Open.ToText()} {CMess.File.ToText()}",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }

        public string OpenFolder(string title = "")
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.IsNullOrWhiteSpace(title)
                ? string.Format(CMess.PlaceholderSelect.ToText(), CMess.Folder.ToText())
                : string.Format(CMess.PlaceholderSelect.ToText(), title),
                IsFolderPicker = true,
            };

            return (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                ? openFileDialog.FileName ?? string.Empty
                : string.Empty;
        }
        #endregion

        #region Save
        public string SaveFile(string title, string defaultExt, params (string label, string pattern)[] filters)
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = title,
                DefaultExtension = defaultExt,
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };

            foreach (var f in filters)
                saveFileDialog.Filters.Add(new CommonFileDialogFilter(f.label, f.pattern));

            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog() != CommonFileDialogResult.Ok) return string.Empty;
            string? path = saveFileDialog.FileName;
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            string ext = System.IO.Path.GetExtension(path);
            bool hasUserExtension = !string.IsNullOrWhiteSpace(ext);
            int selected = saveFileDialog.SelectedFileTypeIndex;

            if (hasUserExtension) return path;
            if (selected == allFilesIndex) return path + "." + defaultExt;

            var filter = saveFileDialog.Filters[selected - 1];
            string filterExt = filter.Extensions?.FirstOrDefault()?.TrimStart('*', '.') ?? defaultExt;
            return path + "." + filterExt;
        }
        public string SaveDataBase()
        {
            return SaveFile($"{CMess.Save.ToText()} {CMess.CardDB.ToText()}", "cdb",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())} (*.cdb; *.db; *.sqlite)", "*.cdb;*.db;*.sqlite"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string SaveDeck()
        {
            return SaveFile($"{CMess.Save.ToText()} {CMess.Deck.ToText()}", "ydk",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string SaveScript()
        {
            return SaveFile($"{CMess.Save.ToText()} {CMess.CardScript.ToText()}", "lua",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardScript.ToText())} (*.lua)", "*.lua"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Md.ToText())} (*.md)", "*.md"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Log.ToText())} (*.log)", "*.log"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Yaml.ToText())} (*.yml)", "*.yml"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())} (*.conf)", "*.conf"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string SaveRes()
        {
            return SaveFile($"{CMess.Save.ToText()} {CMess.File.ToText()}", "res",
                ($"Registry (*.res)", "*.res"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string SaveText()
        {
            return SaveFile($"{CMess.Save.ToText()} {CMess.File.ToText()}", "txt",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string SaveCeds()
        {
            return SaveFile($"{CMess.Save.ToText()} {CMess.Ceds.ToText()}", "ceds",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())} (*.ceds)", "*.ceds"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string SaveZip()
        {
            return SaveFile($"{CMess.newzip.ToText()}", "zip",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())} (*.zip)", "*.zip"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string SaveExcel()
        {
            return SaveFile($"{CMess.Save.ToText()} {CMess.CardScript.ToText()}", "xlsx",
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())} (*.xlsx)", "*.xlsx"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        public string SaveBanList()
        {
            return SaveFile($"{CMess.Save.ToText()} {CMess.File.ToText()}", "lflist.conf",
                ($"{CMess.File.ToText()} (*.lflist.conf)", "*.lflist.conf"),
                ($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
        }
        #endregion
    }
}
