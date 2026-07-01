using System.Linq;
using CardEditor.Localization;
using Microsoft.WindowsAPICodePack.Dialogs;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Helpers
{
    public static class FileDiaLogHelper
    {
        #region Open
        public static string OpenCardList()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.CardDB.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())} (*.cdb; *.db; *.sqlite)", "*.cdb;*.db;*.sqlite"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())} (*.ceds)", "*.ceds"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())} (*.xlsx)", "*.xlsx"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())} (*.lflist.conf)", "*.lflist.conf"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenDataBase()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.CardDB.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())} (*.cdb; *.db; *.sqlite)", "*.cdb;*.db;*.sqlite"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())} (*.ceds)", "*.ceds"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())} (*.xlsx)", "*.xlsx"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenDeck()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.File.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenRare()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.File.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())} (*.cdb)", "*.cdb"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())} (*.ceds)", "*.ceds"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())} (*.xlsx)", "*.xlsx"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenGenesys()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.File.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())} (*.cdb)", "*.cdb"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenScript()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.File.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardScript.ToText())} (*.lua)", "*.lua"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Md.ToText())} (*.md)", "*.md"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Log.ToText())} (*.log)", "*.log"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Yaml.ToText())} (*.yml)", "*.yml"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())} (*.conf)", "*.conf"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenCeds()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.Ceds.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())} (*.ceds)", "*.ceds"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenExcel()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.Excel.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Excel.ToText())} (*.xlsx)", "*.xlsx"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenLua(string filter = "")
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

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenConf(string filter = "")
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

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenBanList()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.File.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())} (*.lflist.conf)", "*.lflist.conf"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenImage()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.File.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Image.ToText())} (*.png, *.jpg, *.jpeg, *.bmp)", "*.jpg;*.jpeg;*.png;*.bmp"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenVideo()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.File.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Video.ToText())} (*.mp4, *.avi)", "*.mp4;*.avi"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenText()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.File.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"));
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        public static string OpenFile()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog()
            {
                Title = $"{CMess.Open.ToText()} {CMess.File.ToText()}",
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false,
            };
            openFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
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

            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok) return openFileDialog.FileName;
            else return string.Empty;
        }
        #endregion

        #region Save
        public static string SaveDataBase()
        {
            CommonSaveFileDialog saveFileDialog = new CommonSaveFileDialog
            {
                Title = $"{CMess.Save.ToText()} {CMess.CardDB.ToText()}",
                DefaultExtension = "cdb",
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardDB.ToText())} (*.cdb; *.db; *.sqlite)", "*.cdb;*.db;*.sqlite"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
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
                Title = $"{CMess.Save.ToText()} {CMess.CardDB.ToText()}",
                DefaultExtension = "ydk",
                AlwaysAppendDefaultExtension = true,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
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
                Title = $"{CMess.Save.ToText()} {CMess.File.ToText()}",
                DefaultExtension = "lua",
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardScript.ToText())} (*.lua)", "*.lua"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Md.ToText())} (*.md)", "*.md"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Log.ToText())} (*.log)", "*.log"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Deck.ToText())} (*.ydk)", "*.ydk"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Yaml.ToText())} (*.yml)", "*.yml"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())} (*.conf)", "*.conf"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
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
                Title = $"{CMess.Save.ToText()} {CMess.File.ToText()}",
                DefaultFileName = "CardEditorX.reg",
                DefaultExtension = "reg",
                EnsurePathExists = true,
                AddToMostRecentlyUsedList = false,
                OverwritePrompt = true
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"Registry (*.reg)", "*.reg"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
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
                Title = $"{CMess.Save.ToText()} {CMess.File.ToText()}",
                DefaultExtension = "txt",
                EnsurePathExists = true,
                AddToMostRecentlyUsedList = false,
                OverwritePrompt = true
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Text.ToText())} (*.txt)", "*.txt"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
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
                Title = $"{CMess.SaveCard.ToText()}",
                DefaultFileName = "cards.ceds",
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())} (*.ceds)", "*.ceds"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
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
                Title = $"{CMess.newzip.ToText()}",
                DefaultExtension = "zip",
                DefaultFileName = "cards.zip",
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Config.ToText())} (*.zip)", "*.zip"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
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
                Title = $"{CMess.SaveCard.ToText()}",
                DefaultExtension = "xlsx",
                DefaultFileName = "cards.xlsx",
                AlwaysAppendDefaultExtension = false,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.CardScript.ToText())} (*.xlsx)", "*.xlsx"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
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
                Title = $"{CMess.Save.ToText()} {CMess.File.ToText()}",
                DefaultFileName = "Card.lflist.conf",
                AlwaysAppendDefaultExtension = true,
                EnsurePathExists = true,
                OverwritePrompt = true,
            };
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.Ceds.ToText())} (*.lflist.conf)", "*.lflist.conf"));
            saveFileDialog.Filters.Add(new CommonFileDialogFilter($"{string.Format(CMess.PlaceholderFIle.ToText(), CMess.All.ToText())} (*.*)", "*.*"));
            int allFilesIndex = saveFileDialog.Filters.Count;

            if (saveFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
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
