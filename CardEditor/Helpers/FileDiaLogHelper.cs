using System;
using System.Linq;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Helpers
{
    public static class FileDiaLogHelper
    {
        #region Open
        public static string OpenCardPack()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.CardArchive.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardArchive.ToText())}", "*.ypk;*.zip;*.rar"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())}", "*.cdb;*.db;*.bytes;*.sqlite"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())}", "*.ceds;*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())}", "*.xlsx;*.xlsm;*.xltx;*.xltm"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenCardList()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.CardDB.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())}", "*.cdb;*.db;*.bytes;*.sqlite"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())}", "*.ceds;*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())}", "*.xlsx;*.xlsm;*.xltx;*.xltm"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.BanList.ToText())}", "*.lflist.conf"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenDataBase()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.CardDB.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())}", "*.cdb;*.db;*.bytes;*.sqlite"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())}", "*.ceds;*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())}", "*.xlsx;*.xlsm;*.xltx;*.xltm"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenDeck()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.Deck.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())}", "*.ydk"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())}", "*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenRare()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.CardDB.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())}", "*.cdb;*.db;*.bytes;*.sqlite"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())}", "*.ceds;*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())})", "*.xlsx;*.xlsm;*.xltx;*.xltm"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())}", "*.ydk"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenGenesys()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.CardDB.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())}", "*.cdb;*.db;*.bytes;*.sqlite"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())}", "*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())}", "*.ydk"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenScript()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.CardScript.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardScript.ToText())}", "*.lua"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())}", "*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Md.ToText())}", "*.md"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Log.ToText())}", "*.log"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())}", "*.ydk"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Yaml.ToText())}", "*.yml"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())}", "*.conf"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenCeds()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.Ceds.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())}", "*.ceds;*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenExcel()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.Excel.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())}", "*.xlsx;*.xlsm;*.xltx;*.xltm"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenLua(string filter = "")
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), filter, CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            string filterDiaLog = string.IsNullOrWhiteSpace(filter) ? CMess.cardlabelSetCode.ToText() : filter;
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{filterDiaLog}", "*.lua"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenConf(string filter = "")
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), filter, CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            string filterDiaLog = string.IsNullOrWhiteSpace(filter) ? CMess.cardlabelSetCode.ToText() : filter;
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{filterDiaLog}", "*.conf"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenBanList()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.BanList.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())}", "*.lflist.conf"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenImage()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.Image.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Image.ToText())}", "*.jpg;*.jpeg;*.png;*.bmp"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenVideo()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.Video.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Video.ToText())}", "*.mp4;*.avi"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenText()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.Text.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())}", "*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenJSON()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.TwoPlaceholderOpen.ToText(), CMess.Json.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Json.ToText())}", "*.json"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenFile()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.Format(CMess.PlaceholderOpen.ToText(), CMess.File.ToText()),
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }

        public static string OpenFolder(string title = "")
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = string.IsNullOrWhiteSpace(title)
                ? string.Format(CMess.PlaceholderSelect.ToText(), CMess.Folder.ToText())
                : string.Format(CMess.PlaceholderSelect.ToText(), title),
                IsFolderPicker = true,
            };

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            if (openFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        #endregion

        #region Save
        public static string SaveDataBase()
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = string.Format(CMess.TwoPlaceholderSave.ToText(), CMess.CardDB.ToText(), CMess.File.ToText()),
                DefaultExtension = "cdb",
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())}", "*.cdb;*.db;*.bytes;*.sqlite"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok)
            {
                string path = saveFileDialog.FileName;
                string ext = System.IO.Path.GetExtension(path);
                bool hasUserExtension = !string.IsNullOrWhiteSpace(ext);
                int selected = saveFileDialog.SelectedFileTypeIndex;
                string finalPath;
                if (hasUserExtension) finalPath = path;
                else
                {
                    if (selected == allFilesIndex)
                    {
                        finalPath = path + "." + saveFileDialog.DefaultExtension;
                    }
                    else
                    {
                        string filterExt = saveFileDialog.Filters[selected - 1].Extensions.First().TrimStart('*', '.');
                        finalPath = path + "." + filterExt;
                    }
                }
                return finalPath;
            }
            else return string.Empty;
        }
        public static string SaveDeck()
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = string.Format(CMess.TwoPlaceholderSave.ToText(), CMess.Deck.ToText(), CMess.File.ToText()),
                DefaultExtension = "ydk",
                AlwaysAppendDefaultExtension = true,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())}", "*.ydk"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())}", "*.txt"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok)
            {
                string path = saveFileDialog.FileName;
                string ext = System.IO.Path.GetExtension(path);
                bool hasUserExtension = !string.IsNullOrWhiteSpace(ext);
                int selected = saveFileDialog.SelectedFileTypeIndex;
                string finalPath;
                if (hasUserExtension) finalPath = path;
                else
                {
                    if (selected == allFilesIndex)
                    {
                        finalPath = path + "." + saveFileDialog.DefaultExtension;
                    }
                    else
                    {
                        string filterExt = saveFileDialog.Filters[selected - 1].Extensions.First().TrimStart('*', '.');
                        finalPath = path + "." + filterExt;
                    }
                }
                return finalPath;
            }
            else return string.Empty;
        }
        public static string SaveScript()
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = string.Format(CMess.TwoPlaceholderSave.ToText(), CMess.CardScript.ToText(), CMess.File.ToText()),
                DefaultExtension = "lua",
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardScript.ToText())}", "*.lua"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())}", "*.txt"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Md.ToText())}", "*.md"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Log.ToText())}", "*.log"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())}", "*.ydk"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Yaml.ToText())}", "*.yml"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())}", "*.conf"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok)
            {
                string path = saveFileDialog.FileName;
                string ext = System.IO.Path.GetExtension(path);
                bool hasUserExtension = !string.IsNullOrWhiteSpace(ext);
                int selected = saveFileDialog.SelectedFileTypeIndex;
                string finalPath;
                if (hasUserExtension) finalPath = path;
                else
                {
                    if (selected == allFilesIndex)
                    {
                        finalPath = path + "." + saveFileDialog.DefaultExtension;
                    }
                    else
                    {
                        string filterExt = saveFileDialog.Filters[selected - 1].Extensions.First().TrimStart('*', '.');
                        finalPath = path + "." + filterExt;
                    }
                }
                return finalPath;
            }    
            else return string.Empty;
        }
        public static string SaveRes()
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = string.Format(CMess.TwoPlaceholderSave.ToText(), "Registry", CMess.File.ToText()),
                DefaultFileName = "CardEditorX.reg",
                DefaultExtension = "reg",
                EnsurePathExists = true,
                AddToMostRecentlyUsedList = false,
                OverwritePrompt = true
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), "Registry")}", "*.reg"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok)
            {
                string path = saveFileDialog.FileName;
                string ext = System.IO.Path.GetExtension(path);
                bool hasUserExtension = !string.IsNullOrWhiteSpace(ext);
                int selected = saveFileDialog.SelectedFileTypeIndex;
                string finalPath;
                if (hasUserExtension) finalPath = path;
                else
                {
                    if (selected == allFilesIndex)
                    {
                        finalPath = path + "." + saveFileDialog.DefaultExtension;
                    }
                    else
                    {
                        string filterExt = saveFileDialog.Filters[selected - 1].Extensions.First().TrimStart('*', '.');
                        finalPath = path + "." + filterExt;
                    }
                }
                return finalPath;
            }
            else return string.Empty;
        }
        public static string SaveText()
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = string.Format(CMess.TwoPlaceholderSave.ToText(), CMess.Text.ToText(), CMess.File.ToText()),
                DefaultExtension = "txt",
                EnsurePathExists = true,
                AddToMostRecentlyUsedList = false,
                OverwritePrompt = true
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())}", "*.txt"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok)
            {
                string path = saveFileDialog.FileName;
                string ext = System.IO.Path.GetExtension(path);
                bool hasUserExtension = !string.IsNullOrWhiteSpace(ext);
                int selected = saveFileDialog.SelectedFileTypeIndex;
                string finalPath;
                if (hasUserExtension) finalPath = path;
                else
                {
                    if (selected == allFilesIndex)
                    {
                        finalPath = path + "." + saveFileDialog.DefaultExtension;
                    }
                    else
                    {
                        string filterExt = saveFileDialog.Filters[selected - 1].Extensions.First().TrimStart('*', '.');
                        finalPath = path + "." + filterExt;
                    }
                }
                return finalPath;
            }
            else return string.Empty;
        }
        public static string SaveCeds()
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = string.Format(CMess.TwoPlaceholderSave.ToText(), CMess.Ceds.ToText(), CMess.File.ToText()),
                DefaultFileName = "cards.ceds",
                DefaultExtension = "ceds",
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())}", "*.ceds"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())}", "*.txt"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok)
            {
                string path = saveFileDialog.FileName;
                string ext = System.IO.Path.GetExtension(path);
                bool hasUserExtension = !string.IsNullOrWhiteSpace(ext);
                int selected = saveFileDialog.SelectedFileTypeIndex;
                string finalPath;
                if (hasUserExtension) finalPath = path;
                else
                {
                    if (selected == allFilesIndex)
                    {
                        finalPath = path + "." + saveFileDialog.DefaultExtension;
                    }
                    else
                    {
                        string filterExt = saveFileDialog.Filters[selected - 1].Extensions.First().TrimStart('*', '.');
                        finalPath = path + "." + filterExt;
                    }
                }
                return finalPath;
            }
            else return string.Empty;
        }
        public static string SaveZip()
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = string.Format(CMess.TwoPlaceholderSave.ToText(), CMess.Zip.ToText(), CMess.File.ToText()),
                DefaultExtension = "zip",
                DefaultFileName = "cards.zip",
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Zip.ToText())}", "*.zip"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok)
            {
                string path = saveFileDialog.FileName;
                string ext = System.IO.Path.GetExtension(path);
                bool hasUserExtension = !string.IsNullOrWhiteSpace(ext);
                int selected = saveFileDialog.SelectedFileTypeIndex;
                string finalPath;
                if (hasUserExtension) finalPath = path;
                else
                {
                    if (selected == allFilesIndex)
                    {
                        finalPath = path + "." + saveFileDialog.DefaultExtension;
                    }
                    else
                    {
                        string filterExt = saveFileDialog.Filters[selected - 1].Extensions.First().TrimStart('*', '.');
                        finalPath = path + "." + filterExt;
                    }
                }
                return finalPath;
            }
            else return string.Empty;
        }
        public static string SaveExcel()
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = string.Format(CMess.TwoPlaceholderSave.ToText(), CMess.Excel.ToText(), CMess.File.ToText()),
                DefaultExtension = "xlsx",
                DefaultFileName = "cards.xlsx",
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())}", "*.xlsx;*.xlsm;*.xltx;*.xltm"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok)
            {
                string path = saveFileDialog.FileName;
                string ext = System.IO.Path.GetExtension(path);
                bool hasUserExtension = !string.IsNullOrWhiteSpace(ext);
                int selected = saveFileDialog.SelectedFileTypeIndex;
                string finalPath;
                if (hasUserExtension) finalPath = path;
                else
                {
                    if (selected == allFilesIndex)
                    {
                        finalPath = path + "." + saveFileDialog.DefaultExtension;
                    }
                    else
                    {
                        string filterExt = saveFileDialog.Filters[selected - 1].Extensions.First().TrimStart('*', '.');
                        finalPath = path + "." + filterExt;
                    }
                }
                return finalPath;
            }
            else return string.Empty;
        }
        public static string SaveJSON()
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = string.Format(CMess.TwoPlaceholderSave.ToText(), CMess.Json.ToText(), CMess.File.ToText()),
                DefaultExtension = "json",
                DefaultFileName = "cards.json",
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Json.ToText())}", "*.json"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok)
            {
                string path = saveFileDialog.FileName;
                string ext = System.IO.Path.GetExtension(path);
                bool hasUserExtension = !string.IsNullOrWhiteSpace(ext);
                int selected = saveFileDialog.SelectedFileTypeIndex;
                string finalPath;
                if (hasUserExtension) finalPath = path;
                else
                {
                    if (selected == allFilesIndex)
                    {
                        finalPath = path + "." + saveFileDialog.DefaultExtension;
                    }
                    else
                    {
                        string filterExt = saveFileDialog.Filters[selected - 1].Extensions.First().TrimStart('*', '.');
                        finalPath = path + "." + filterExt;
                    }
                }
                return finalPath;
            }
            else return string.Empty;
        }
        public static string SaveBanList()
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = string.Format(CMess.TwoPlaceholderSave.ToText(), CMess.BanList.ToText(), CMess.File.ToText()),
                DefaultExtension = ".lflist.conf",
                DefaultFileName = "Card.lflist.conf",
                AlwaysAppendDefaultExtension = true,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())}", "*.lflist.conf"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())}", "*.*"));

            Window owner = WindowHelper.GetActiveWindow();
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(owner).Handle;

            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog(handle) == CommonFileDialogResult.Ok)
            {
                string path = saveFileDialog.FileName;
                string ext = System.IO.Path.GetExtension(path);
                bool hasUserExtension = !string.IsNullOrWhiteSpace(ext);
                int selected = saveFileDialog.SelectedFileTypeIndex;
                string finalPath;
                if (hasUserExtension) finalPath = path;
                else
                {
                    if (selected == allFilesIndex)
                    {
                        finalPath = path + "." + saveFileDialog.DefaultExtension;
                    }
                    else
                    {
                        string filterExt = saveFileDialog.Filters[selected - 1].Extensions.First().TrimStart('*', '.');
                        finalPath = path + "." + filterExt;
                    }
                }
                return finalPath;
            }
            else return string.Empty;
        }
        #endregion

    }
}
