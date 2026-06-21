using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Windows.Threading;

namespace ScriptSupport.Legacy
{
    /// <summary>
    /// Interaction logic for ScrapiBook.xaml
    /// </summary>
    public partial class ScrapiBook : Window
    {
        #region Variable
        public ICommand SaveCommand { get; set; }
        private Brush themeColor;
        public string currentFilePath;
        private bool isResizing = false;
        private double WidthLeft;
        private double WidthRight;
        private Point MousePosition;
        #endregion

        public ScrapiBook()
        {
            InitializeComponent();
            // SaveCommand = new RelayCommand(SaveFile);
            DataContext = this;
        }

        #region Load Book
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            LoadDirectoryTreeAsync();
        }
        private void LoadConfig()
        {
            //#region Color
            //string backgroundHex = SettingService.GetStringSetting("backgroundset", "#FF000000");
            //Color backgroundColor = (Color)ColorConverter.ConvertFromString(backgroundHex);
            //string foregroundHex = SettingService.GetStringSetting("foregroundset", "#FFFFFFFF");
            //Color foregroundColor = (Color)ColorConverter.ConvertFromString(foregroundHex);
            //Brush backgroundBrush = new SolidColorBrush(backgroundColor);
            //Brush foregroundBrush = new SolidColorBrush(foregroundColor);

            //// this.Background = backgroundBrush;
            //// this.Foreground = foregroundBrush;
            //#endregion

            //#region Font
            //string fontFamilyConfig = SettingService.GetStringSetting("fontfamily", "Consolas");
            //FontFamily fontFamily = new FontFamily(fontFamilyConfig);
            //_fontService.SetFontFamily(fontFamily);

            //int fontSizeSetting = SettingService.GetIntSetting("fontsize", 14);
            //_fontService.SetFontSize(fontSizeSetting);
            //#endregion

            //#region Theme
            //string[] itemstheme = new string[] { "Amber", "Blue", "BlueGrey", "Brown", "Cyan", "DeepOrange", "DeepPurple", "Green", "Grey", "Indigo", "LightBlue", "LightGreen", "Lime", "Orange", "Pink", "Purple", "Red", "Teal", "Yellow" };
            //string themeSet = SettingService.GetStringSetting("themecolor", "DeepPurple");
            //if (!Array.Exists(itemstheme, theme => theme.Equals(themeSet, StringComparison.OrdinalIgnoreCase)))
            //{
            //    themeSet = "DeepPurple";
            //}
            //ColorDictionary colorDict = new ColorDictionary();
            //string hexCode = colorDict.GetHexCode(themeSet);
            //try
            //{
            //    Color color = (Color)ColorConverter.ConvertFromString(hexCode);
            //    themeColor = new SolidColorBrush(color);
            //}
            //catch
            //{
            //    Color defaultColor = (Color)ColorConverter.ConvertFromString("#673AB7");
            //    themeColor = new SolidColorBrush(defaultColor);
            //}
            //#endregion

            //_themeService.SetTheme(backgroundBrush, foregroundBrush, themeColor);
            //separatorvertical.Background = themeColor;

            //#region HighLight
            //try
            //{
            //    HighLightService highLightService = new HighLightService();
            //    string highlightFilePath = highLightService.HighLightFilePath();
            //    using (Stream stream = File.OpenRead(highlightFilePath))
            //    using (XmlTextReader reader = new XmlTextReader(stream))
            //    {
            //        FileTextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            //    }
            //}
            //catch (FileNotFoundException fnfEx)
            //{
            //    // Xử lý lỗi khi không tìm thấy file
            //    MessageBox.Show($"Error loading syntax highlighting file: {fnfEx.Message}",
            //                    "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //catch (XmlException xmlEx)
            //{
            //    // Xử lý lỗi khi đọc hoặc phân tích XML
            //    MessageBox.Show($"Error reading the syntax highlighting file: {xmlEx.Message}",
            //                    "XML Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //catch (Exception ex)
            //{
            //    // Xử lý các lỗi không mong muốn khác
            //    MessageBox.Show($"An unexpected error occurred: {ex.Message}",
            //                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            //#endregion

            //#region Wordwrap
            //bool wordWrap = SettingService.GetBoolSetting("wordwrap", false);
            //FileTextEditor.WordWrap = wordWrap;
            //#endregion
        }
        #endregion

        #region Load Data
        private async void LoadDirectoryTreeAsync()
        {
            string scrapiyardFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            TreeViewItem rootItem = new TreeViewItem
            {
                Header = "Data",
                Tag = scrapiyardFolderPath
            };
            FileTreeView.Items.Add(rootItem);
            await LoadSubdirectoriesAsync(rootItem);
        }

        // Phương thức bất đồng bộ để quét các thư mục con
        private async Task LoadSubdirectoriesAsync(TreeViewItem parentItem)
        {
            string directoryPath = parentItem.Tag.ToString();
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    // Tải danh sách thư mục con và files bất đồng bộ
                    var subdirectories = await Task.Run(() => Directory.GetDirectories(directoryPath));
                    var files = await Task.Run(() => Directory.GetFiles(directoryPath));
                    // Thêm các thư mục con vào TreeViewItem
                    foreach (string subdir in subdirectories)
                    {
                        TreeViewItem subItem = new TreeViewItem
                        {
                            Header = Path.GetFileName(subdir),
                            Tag = subdir
                        };
                        parentItem.Items.Add(subItem);
                        await LoadSubdirectoriesAsync(subItem);
                    }
                    foreach (string file in files)
                    {
                        TreeViewItem fileItem = new TreeViewItem
                        {
                            Header = Path.GetFileName(file),
                            Tag = file
                        };
                        parentItem.Items.Add(fileItem);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi quét thư mục: " + ex.Message);
                }
            }
        }

        #endregion

        #region Change
        private async void FileTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem selectedItemWithColor)
            {
                selectedItemWithColor.Background = themeColor;
            }
            if (e.OldValue is TreeViewItem oldItem)
            {
                oldItem.Background = Brushes.Transparent;
            }

            if (FileTreeView.SelectedItem is TreeViewItem selectedItem)
            {
                string filePath = selectedItem.Tag as string;

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    try
                    {
                        // Đọc tệp tin một cách bất đồng bộ
                        currentFilePath = filePath;
                        string fileContent = await ReadFileAsync(currentFilePath);
                        FileTextEditor.Text = fileContent;
                    }
                    catch (IOException ioEx)
                    {
                        MessageBox.Show($"Lỗi khi đọc tệp tin: {ioEx.Message}");
                    }
                    catch (UnauthorizedAccessException uaEx)
                    {
                        MessageBox.Show($"Không có quyền truy cập tệp tin: {uaEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}");
                    }
                }
                else
                {
                    // MessageBox.Show("Tệp không tồn tại hoặc không hợp lệ.");
                }
            }
        }
        private async Task<string> ReadFileAsync(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (FileNotFoundException ex)
            {
                // Xử lý trường hợp tệp không tìm thấy
                throw new Exception($"Tệp không tồn tại: {filePath}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Xử lý trường hợp không có quyền truy cập tệp
                throw new Exception($"Không có quyền truy cập tệp: {filePath}", ex);
            }
            catch (IOException ex)
            {
                // Xử lý các lỗi I/O (ví dụ: tệp bị khóa hoặc hỏng)
                throw new Exception($"Lỗi khi đọc tệp: {filePath}", ex);
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ không mong muốn
                throw new Exception($"Đã xảy ra lỗi không xác định khi đọc tệp: {filePath}", ex);
            }

        }

        private void separatorvertical_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                var mousePosition = Mouse.GetPosition(this);
                double deltaX = mousePosition.X - MousePosition.X;
                // Cập nhật bề rộng của cột trái và phải
                double newWidthLeft = WidthLeft + deltaX;
                double newWidrgRight = WidthRight - deltaX;
                // Đảm bảo không vượt quá bề rộng tối thiểu
                if (newWidthLeft > 0 && newWidrgRight > 0)
                {
                    int newWidth = (int)newWidthLeft;
                    grleft.Width = new GridLength(newWidth);
                }
            }
        }
        private void separatorvertical_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isResizing = true;
                WidthLeft = grleft.ActualWidth;
                WidthRight = grright.ActualWidth;
                MousePosition = Mouse.GetPosition(this);
                Mouse.Capture(separatorvertical);
            }
        }
        private void separatorvertical_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isResizing)
            {
                isResizing = false;
                Mouse.Capture(null);
            }
        }
        #endregion

        #region Save
        private void SaveFile()
        {
            try
            {
                var filePath = currentFilePath;
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    System.IO.File.WriteAllText(filePath, FileTextEditor.Text);
                }
                else
                {
                    MessageBox.Show("Đường dẫn tệp không hợp lệ.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Không có quyền ghi vào tệp: {ex.Message}");
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Lỗi khi ghi tệp: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi không xác định: {ex.Message}");
            }
        }

        #endregion
    }
}
