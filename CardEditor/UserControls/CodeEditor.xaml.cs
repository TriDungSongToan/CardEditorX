using System;
using System.IO;
using System.Web.UI.WebControls;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using CardEditor.Enums;
using CardEditor.Tools;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Services;
using CardEditor.Services.LoadData;
using CardEditor.Services.SaveData;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl, INotifyPropertyChanged, IDisposable, ISaveable
    {
        #region Variable
        private IMainWindowService MainWindowService;
        private AppTitle _mainWindowTitle = new();
        public AppTitle MainWindowTitle
        {
            get => _mainWindowTitle;
            set
            {
                if (_mainWindowTitle != value)
                {
                    _mainWindowTitle = value;
                    OnPropertyChanged(nameof(MainWindowTitle));

                    if (MainWindowService == null) return;
                    MainWindowService.UpdateWindowTitle(MainWindowTitle);
                    MainWindowService.UpdateTabItemHeader(MainWindowTitle.DisplayTabItemHeader);
                }
            }
        }
        private void RebuildWindowTitleYGO()
        {
            if (string.IsNullOrWhiteSpace(archiveFilePath))
            {
                if (!string.IsNullOrEmpty(luaFilePath)) MainWindowTitle = FileLocationService.BuildTitlePhysicalFile(luaFilePath);
                else MainWindowTitle = FileLocationService.BuildTitlePhysicalFile(string.Empty);
            }
            else MainWindowTitle = FileLocationService.BuildTitleZipEntry(archiveFilePath, archiveEntryName);
        }
        private void RebuildWindowTitleOMEGA(CardOmegaScript omegaScript)
        {
            if (omegaScript == null)
            {
                RebuildWindowTitleYGO();
                return;
            }
            string physicalFullPath =
                omegaScript.cdbFilePath ??
                omegaScript.xlsxFilePath ??
                omegaScript.cedsFilePath ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(archiveFilePath))
                MainWindowTitle = FileLocationService.BuildTitleDBCell(physicalFullPath, "datas", "script", omegaScript.id);
            else
                MainWindowTitle = FileLocationService.BuildTitleDBCellZipEntry(physicalFullPath, "datas", "script", omegaScript.id,
                    omegaScript.archiveFilePath, omegaScript.archiveEntryName);
        }

        private bool _isSaved = true;
        public bool IsSaved
        {
            get => _isSaved;
            set
            {
                if (_isSaved != value)
                {
                    _isSaved = value;
                    OnPropertyChanged(nameof(IsSaved));
                    UpdateWindowSavedFlag();
                }
            }
        }

        private bool _isOverwrite = false;
        public bool IsOverwrite
        {
            get => _isOverwrite;
            set
            {
                if (_isOverwrite != value)
                {
                    _isOverwrite = value;
                    OnPropertyChanged(nameof(IsOverwrite));
                    codePaneTop.ChangeOVR(IsOverwrite);
                    codePaneBottom.ChangeOVR(IsOverwrite);
                }
            }
        }

        private int _selectedIndentOption;
        public int SelectedIndentOption
        {
            get => _selectedIndentOption;
            set
            {
                if (_selectedIndentOption != value)
                {
                    _selectedIndentOption = value;
                    OnPropertyChanged(nameof(SelectedIndentOption));
                    OnTabSpcChange();
                }
            }
        }
        private int _selectedNewLineOption;
        public int SelectedNewLineOption
        {
            get => _selectedNewLineOption;
            set
            {
                if (_selectedNewLineOption != value)
                {
                    _selectedNewLineOption = value;
                    OnPropertyChanged(nameof(SelectedNewLineOption));
                    OnNewLineOptionChange();
                }
            }
        }

        public string luaFilePath { get; set; }
        public string luaFileName { get; set; }
        public string archiveFilePath;
        public string archiveEntryName;

        private TextDocument _sharedDocument;
        public TextDocument SharedDocument
        {
            get => _sharedDocument;
            set
            {
                if (_sharedDocument != value)
                {
                    _sharedDocument = value;
                    OnPropertyChanged(nameof(SharedDocument));
                }
            }
        }

        private LuaFoldingManager foldingManager;

        public CardOmegaScript OmegaScript { get; set; } = null;
        public CardListFormat CurrentFormat { get; set; } = CardListFormat.YGONoFlag;
        #endregion

        #region Constructor
        public CodeEditor()
        {
            InitializeComponent();
            SharedDocument = new TextDocument();
            
            DataContext = this;
            codePaneTop.DataContext = this;
            codePaneBottom.DataContext = this;
        }
        public CodeEditor(IMainWindowService service) : this()
        {
            MainWindowService = service;
        }
        #endregion

        #region Load
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            LoadScriptData();
            SharedDocument.TextChanged += SharedDocument_TextChanged;

            codePaneTop.TabSpcChanged += CodePaneTop_TabSpcChanged;
            codePaneTop.NewLineChanged += CodePaneTop_NewLineChanged;
            codePaneBottom.TabSpcChanged += CodePaneBottom_TabSpcChanged;
            codePaneBottom.NewLineChanged += CodePaneBottom_NewLineChanged;

            SelectedIndentOption = 0;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                SelectedNewLineOption = 0;
            else SelectedNewLineOption = 1;

            // await LoadLuaFile();

            UpdateThumbPosition();
        }
        public void LoadConfig()
        {
            codePaneTop.LoadConfig();
            codePaneBottom.LoadConfig();

            double fontSize = (double)UIConfigViewModel.Instance.FontSize;
            if (FontSize > 0)
            {
                codePaneTop.FontSizeText = fontSize;
                codePaneBottom.FontSizeText = fontSize;
            }

            foldingManager = new LuaFoldingManager(codePaneTop.textEditorPane);
            foldingManager = new LuaFoldingManager(codePaneBottom.textEditorPane);

            if (ConfigViewModel.Instance.codeEditSetting.CodeFolding) foldingManager.Enable();
            else foldingManager.Disable();
        }
        private void LoadScriptData()
        {
            if (!ScriptViewModel.Instance.IsLoaded)
            {
                var (result, message) = ScriptViewModel.Instance.LoadScriptData();
                if (!result)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                }
            }
        }
        public async Task LoadLuaFileYGO()
        {
            if (string.IsNullOrWhiteSpace(luaFilePath) || !System.IO.File.Exists(luaFilePath)) return;
            if (OmegaScript != null) return;

            try
            {
                CurrentFormat = CardListFormat.YGONoFlag;

                string text = string.Empty;
                using (var stream = new FileStream(luaFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 8192, useAsync: true))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    text = await reader.ReadToEndAsync();
                }
                await Dispatcher.InvokeAsync(() =>
                {
                    SharedDocument.Text = text;
                });
                codePaneTop.ResetSavedMarker();
                codePaneBottom.ResetSavedMarker();

                RebuildWindowTitleYGO();
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            finally
            {
                IsSaved = true;
            }
        }
        public async Task LoadLuaFileOMEGA(CardOmegaScript omegaScript)
        {
            if (omegaScript == null) return;
            if (!string.IsNullOrEmpty(luaFilePath)) return;

            try
            {
                OmegaScript = omegaScript;
                CurrentFormat = CardListFormat.OMEGA;

                await Dispatcher.InvokeAsync(() =>
                {
                    SharedDocument.Text = OmegaScript.BaseCard.script;
                });
                codePaneTop.ResetSavedMarker();
                codePaneBottom.ResetSavedMarker();

                RebuildWindowTitleOMEGA(OmegaScript);
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            finally
            {
                IsSaved = true;
            }
        }
        #endregion

        #region Save
        public async Task<bool> Save()
        {
            try
            {
                return await SaveCodeCommand();
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SaveCodeCommand(string targetedPath = null)
        {
            if (CurrentFormat == CardListFormat.OMEGA) return await SaveCodeOMEGACommand();
            else return await SaveCodeYGOCommand(targetedPath);
        }
        public async Task<bool> SaveCodeYGOCommand(string targetedPath = null)
        {
            if (string.IsNullOrWhiteSpace(SharedDocument.Text))
            {
                var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    CMess.confirmSaveBlank.ToText(), new[] { CMess.yes.ToText(), CMess.no.ToText() }, 1);
                if (result != 0) return false;
            }

            try
            {
                string directory = Path.GetDirectoryName(targetedPath);
                if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

                // Không truyền TargerPath => Save vào file hiện tại.
                if (string.IsNullOrWhiteSpace(targetedPath))
                {
                    if (!string.IsNullOrWhiteSpace(luaFilePath)) targetedPath = luaFilePath;
                    else
                    {
                        string newFilePath = FileDiaLogHelper.SaveScript();
                        if (string.IsNullOrWhiteSpace(newFilePath)) return false;

                        luaFilePath = newFilePath;
                        luaFileName = Path.GetFileName(newFilePath);
                        targetedPath = newFilePath;
                    }
                }
                // Có truyền TargerPath => Save As
                else
                {
                    luaFilePath = targetedPath;
                    luaFileName = Path.GetFileName(targetedPath);
                }

                string textToSave = ConvertLineEndings(SharedDocument.Text, SelectedNewLineOption);
                using (var stream = new FileStream(targetedPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.NewLine = GetNewLineString(SelectedNewLineOption);
                    await writer.WriteAsync(textToSave);
                }
                var (resultArchi, messArchi) = await SaveToArchive(targetedPath);
                if (!resultArchi)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, messArchi, new[] { CMess.ok.ToText() });
                    return false;
                }
                IsSaved = true;
                codePaneTop.MarkAsSaved();
                codePaneBottom.MarkAsSaved();
                RebuildWindowTitleYGO();

                return true;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.Script.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                return false;
            }
        }
        public async Task<bool> SaveCodeOMEGACommand()
        {
            if (OmegaScript == null)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.Data.ToText())} Card Cannot be Null",
                    new[] { CMess.ok.ToText() });
                return false;
            }
            if (string.IsNullOrWhiteSpace(SharedDocument.Text))
            {
                var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    CMess.confirmSaveBlank.ToText(), new[] { CMess.yes.ToText(), CMess.no.ToText() }, 1);
                if (result != 0) return false;
            }

            OmegaScript.BaseCard.script = SharedDocument.Text;

            try
            {
                if (!string.IsNullOrEmpty(OmegaScript.cdbFilePath))
                {
                    WriteResult resultModify = await SaveDatabaseOMEGAService.ModifyCurrentCard(OmegaScript.BaseCard, OmegaScript.cdbFilePath, hasFlag: true);
                    if (!resultModify.Result)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.CardScript.ToText())} {resultModify.Messenger}",
                            new[] { CMess.ok.ToText() });
                        return false;
                    }
                    var (resultArchi, messArchi) = await SaveToArchive(OmegaScript, OmegaScript.cdbFilePath);
                    if (!resultArchi)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, messArchi, new[] { CMess.ok.ToText() });
                        return false;
                    }
                    IsSaved = true;
                    codePaneTop.MarkAsSaved();
                    codePaneBottom.MarkAsSaved();

                    RebuildWindowTitleOMEGA(OmegaScript);
                }
                else if (!string.IsNullOrEmpty(OmegaScript.xlsxFilePath))
                {
                    WriteResult resultModify = await SaveExcelOMEGAService.ModifyCurrentCard(OmegaScript.BaseCard, OmegaScript.xlsxFilePath, hasFlagHint: true);
                    if (!resultModify.Result)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.CardScript.ToText())} {resultModify.Messenger}",
                            new[] { CMess.ok.ToText() });
                        return false;
                    }
                    var (resultArchi, messArchi) = await SaveToArchive(OmegaScript, OmegaScript.xlsxFilePath);
                    if (!resultArchi)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, messArchi, new[] { CMess.ok.ToText() });
                        return false;
                    }
                    IsSaved = true;
                    codePaneTop.MarkAsSaved();
                    codePaneBottom.MarkAsSaved();

                    RebuildWindowTitleOMEGA(OmegaScript);
                }
                else if (!string.IsNullOrEmpty(OmegaScript.cedsFilePath))
                {
                    WriteResult resultModify = await SaveCedsOMEGAService.ModifyCurrentCard(OmegaScript.BaseCard, OmegaScript.cedsFilePath, hasFlag: true);
                    if (!resultModify.Result)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.CardScript.ToText())} {resultModify.Messenger}",
                            new[] { CMess.ok.ToText() });
                        return false;
                    }
                    var (resultArchi, messArchi) = await SaveToArchive(OmegaScript, OmegaScript.cedsFilePath);
                    if (!resultArchi)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, messArchi, new[] { CMess.ok.ToText() });
                        return false;
                    }
                    IsSaved = true;
                    codePaneTop.MarkAsSaved();
                    codePaneBottom.MarkAsSaved();

                    RebuildWindowTitleOMEGA(OmegaScript);
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                       $"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText())} Card Cannot be Null",
                       new[] { CMess.ok.ToText() });
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.CardScript.ToText())} {ex.Message}",
                    new[] { CMess.ok.ToText() });
                return false;
            }
        }
        public async Task<(bool, string)> SaveToArchive(string targetSourcePath)
        {
            if (string.IsNullOrEmpty(archiveFilePath) ||
                !System.IO.File.Exists(archiveFilePath) ||
                string.IsNullOrEmpty(archiveEntryName))
                return (true, string.Empty);

            if (string.IsNullOrEmpty(targetSourcePath) || !System.IO.File.Exists(targetSourcePath))
                return (false, CMess.fileNotExit.ToText());

            return await LoadArchiveService.SaveEntryToZip(archiveFilePath, archiveEntryName, targetSourcePath);
        }
        public async Task<(bool, string)> SaveToArchive(CardOmegaScript cardScript, string targetSourcePath)
        {
            if (string.IsNullOrEmpty(cardScript.archiveFilePath) ||
                !System.IO.File.Exists(cardScript.archiveFilePath) ||
                string.IsNullOrEmpty(cardScript.archiveEntryName))
                return (true, string.Empty);

            if (string.IsNullOrEmpty(targetSourcePath) || !System.IO.File.Exists(targetSourcePath))
                return (false, CMess.fileNotExit.ToText());

            return await LoadArchiveService.SaveEntryToZip(cardScript.archiveFilePath, cardScript.archiveEntryName, targetSourcePath);
        }
        private string ConvertLineEndings(string text, int newLineOption)
        {
            string normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");

            return newLineOption switch
            {
                0 => normalized.Replace("\n", "\r\n"),  // CRLF
                1 => normalized,                        // LF (giữ nguyên)
                2 => normalized.Replace("\n", "\r"),    // CR
                _ => normalized.Replace("\n", "\r\n")   // Default CRLF
            };
        }
        private string GetNewLineString(int newLineOption)
        {
            return newLineOption switch
            {
                0 => "\r\n", // CRLF
                1 => "\n",   // LF
                2 => "\r",   // CR
                _ => "\r\n"  // Default
            };
        }
        #endregion

        #region Scroll
        private void ScrollPane()
        {
            int line = codePaneBottom.GetFirstVisibleLine();
            codePaneTop.ScrollToLine(line);
        }
        #endregion

        #region Lua 
        private void chkerror_Click(object sender, RoutedEventArgs e)
        {
            LuaLinter linter = new LuaLinter();

            string formattedCode = linter.FormatLuaCode(SharedDocument.Text);

            if (!formattedCode.StartsWith("Lỗi"))
            {
                SharedDocument.Text = formattedCode;
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()),
                    new[] { CMess.ok.ToText() });
            }
        }
        public void CheckLua()
        {
            LuaLinter linter = new LuaLinter();

            string formattedCode = linter.FormatLuaCode(SharedDocument.Text);

            if (!formattedCode.StartsWith("Lỗi"))
            {
                SharedDocument.Text = formattedCode;
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()),
                    new[] { CMess.ok.ToText() });
            }
        }
        public string FormatLuaWithStylua(string luaCode)
        {
            try
            {
                // string toolsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools");
                string toolsPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(CardEditor.Models.AppContext.Instance.ExeFilePath), "Tools");
                string exePath = System.IO.Path.Combine(toolsPath, "stylua.exe");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = "--stdin",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    using (StreamWriter sw = process.StandardInput)
                    {
                        if (sw.BaseStream.CanWrite)
                        {
                            sw.Write(luaCode);
                        }
                    }

                    string formattedCode = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return formattedCode;
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()),
                    new[] { CMess.ok.ToText() });
                return $"Lỗi định dạng: {ex.Message}";
            }
        }
        #endregion

        #region Change
        private const int TabSize = 4;
        private void OnTabSpcChange()
        {
            codePaneTop.ChangeTabSpcOptionPane(SelectedIndentOption);
            codePaneBottom.ChangeTabSpcOptionPane(SelectedIndentOption);
            if (SelectedIndentOption == 0) TabsToSpaces();
            else SpacesToTabs();
        }
        private void OnNewLineOptionChange()
        {
            codePaneTop.ChangeNewLineOptionPane(SelectedNewLineOption);
            codePaneBottom.ChangeNewLineOptionPane(SelectedNewLineOption);
        }
        public void TabsToSpaces()
        {
            if (SharedDocument == null) return;

            using (SharedDocument.RunUpdate())
            {
                var lines = SharedDocument.Lines.ToList();

                foreach (var line in lines)
                {
                    string lineText = SharedDocument.GetText(line.Offset, line.Length);

                    // Tìm phần leading whitespace
                    int leadingWhitespaceLength = 0;
                    for (int i = 0; i < lineText.Length; i++)
                    {
                        if (lineText[i] == '\t' || lineText[i] == ' ')
                            leadingWhitespaceLength++;
                        else
                            break;
                    }

                    if (leadingWhitespaceLength > 0)
                    {
                        string leadingPart = lineText.Substring(0, leadingWhitespaceLength);
                        string remainingPart = lineText.Substring(leadingWhitespaceLength);

                        // Chỉ thay thế tab trong phần leading
                        string newLeadingPart = leadingPart.Replace("\t", new string(' ', TabSize));

                        SharedDocument.Replace(line.Offset, line.Length, newLeadingPart + remainingPart);
                    }
                }
            }
        }
        public void SpacesToTabs()
        {
            if (SharedDocument == null) return;

            using (SharedDocument.RunUpdate())
            {
                var lines = SharedDocument.Lines.ToList();

                foreach (var line in lines)
                {
                    string lineText = SharedDocument.GetText(line.Offset, line.Length);

                    // Tìm phần leading whitespace
                    int leadingWhitespaceLength = 0;
                    for (int i = 0; i < lineText.Length; i++)
                    {
                        if (lineText[i] == '\t' || lineText[i] == ' ')
                            leadingWhitespaceLength++;
                        else
                            break;
                    }

                    if (leadingWhitespaceLength > 0)
                    {
                        string leadingPart = lineText.Substring(0, leadingWhitespaceLength);
                        string remainingPart = lineText.Substring(leadingWhitespaceLength);

                        // Chỉ thay thế spaces trong phần leading
                        string newLeadingPart = leadingPart.Replace(new string(' ', TabSize), "\t");

                        SharedDocument.Replace(line.Offset, line.Length, newLeadingPart + remainingPart);
                    }
                }
            }
        }

        private void UpdateWindowSavedFlag()
        {
            if (MainWindowService != null)
            {
                MainWindowService.UpdateWindowSavedFlag(IsSaved);
            }
        }
        #endregion

        #region MiniMap
        private void ThumbSplit_DragStarted(object sender, DragStartedEventArgs e)
        {
            ScrollPane();
        }
        private void MainSplitter_DragStarted(object sender, DragStartedEventArgs e)
        {
            ThumbSplit.Visibility = Visibility.Collapsed;
            ScrollPane();
        }
        private const double COLLAPSED_THRESHOLD = 10.0;
        private void MainSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            double finalHeight = TopPaneRow.ActualHeight;

            if (finalHeight <= COLLAPSED_THRESHOLD)
            {
                TopPaneRow.Height = new GridLength(0);
                ThumbSplit.Visibility = Visibility.Visible;
                btnHiddenTopPane.Visibility = Visibility.Collapsed;
            }
            else
            {
                ThumbSplit.Visibility = Visibility.Collapsed;
                btnHiddenTopPane.Visibility = Visibility.Visible;
            }
        }
        private void ThumbSplit_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double currentTop = Canvas.GetTop(ThumbSplit);
            double newTop = currentTop + e.VerticalChange;

            double maxTop = ThumbCanvas.ActualHeight - ThumbSplit.ActualHeight;
            if (newTop < 0) newTop = 0;
            if (newTop > maxTop) newTop = maxTop;

            Canvas.SetTop(ThumbSplit, newTop);
            TopPaneRow.Height = new GridLength(newTop);
        }
        private void ThumbSplit_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            double finalHeight = TopPaneRow.ActualHeight;

            if (finalHeight <= COLLAPSED_THRESHOLD)
            {
                TopPaneRow.Height = new GridLength(0);
                Canvas.SetTop(ThumbSplit, 0);
                ThumbSplit.Visibility = Visibility.Visible;
                btnHiddenTopPane.Visibility = Visibility.Collapsed;
            }
            else
            {
                Canvas.SetTop(ThumbSplit, 0);
                ThumbSplit.Visibility = Visibility.Collapsed;
                btnHiddenTopPane.Visibility = Visibility.Visible;
            }
        }
        private void ThumbCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateThumbPosition();
        }
        private void UpdateThumbPosition()
        {
            Canvas.SetLeft(ThumbSplit, ThumbCanvas.ActualWidth - ThumbSplit.Width);
            double currentTop = Canvas.GetTop(ThumbSplit);
            if (double.IsNaN(currentTop)) currentTop = 0;

            double maxTop = ThumbCanvas.ActualHeight - ThumbSplit.ActualHeight;
            if (currentTop > maxTop) Canvas.SetTop(ThumbSplit, maxTop);
        }
        private void btnHiddenTopPane_Click(object sender, RoutedEventArgs e)
        {
            TopPaneRow.Height = new GridLength(0);
            Canvas.SetTop(ThumbSplit, 0);
            ThumbSplit.Visibility = Visibility.Visible;
            btnHiddenTopPane.Visibility = Visibility.Collapsed;
        }
        private void MiniMapSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (GridMiniMap.ActualWidth > 0) ConfigViewModel.Instance.displaySetting.WidthMiniMap = (int)GridMiniMap.ActualWidth;
            else ConfigViewModel.Instance.displaySetting.WidthMiniMap = 50;
            UpdateThumbPosition();
            ConfigViewModel.Instance.SaveDisplaySettingFile();
        }
        #endregion

        #region IDisposable
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }
        public void Dispose()
        {
            SharedDocument.TextChanged -= SharedDocument_TextChanged;

            codePaneTop.TabSpcChanged -= CodePaneTop_TabSpcChanged;
            codePaneTop.NewLineChanged -= CodePaneTop_NewLineChanged;
            codePaneBottom.TabSpcChanged -= CodePaneBottom_TabSpcChanged;
            codePaneBottom.NewLineChanged -= CodePaneBottom_NewLineChanged;


            SharedDocument.Text = string.Empty;
            foldingManager?.Disable();
        }
        #endregion

        #region Event
        private void CodePaneTop_TabSpcChanged(object sender, int e)
        {
            SelectedIndentOption = e;
        }
        private void CodePaneBottom_TabSpcChanged(object sender, int e)
        {
            SelectedIndentOption = e;
        }

        private void CodePaneTop_NewLineChanged(object sender, int e)
        {
            SelectedNewLineOption = e;
        }
        private void CodePaneBottom_NewLineChanged(object sender, int e)
        {
            SelectedNewLineOption = e;
        }



        private void rootEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Insert)
            {
                IsOverwrite = !IsOverwrite;
                e.Handled = true;
            }
        }
        private void SharedDocument_TextChanged(object sender, EventArgs e)
        {
            IsSaved = false;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
