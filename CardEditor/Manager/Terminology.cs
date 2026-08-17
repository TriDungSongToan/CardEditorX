using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data.Common;
using System.Globalization;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Diagnostics;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Manager
{
    public static class Terminology
    {
        public static async Task<(bool result, string message, string filePath)> ExtractTerminology(string dataSourcePath, string dataFolderPath)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                // Sử dụng Dictionary với key là dạng chuẩn hóa của từ để loại bỏ trùng lặp
                Dictionary<string, string> terms = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                // Danh sách các ký tự đặc biệt cho phép
                HashSet<char> allowedSpecialChars = new HashSet<char>
                {
                    '\'', '∀', '★', '☆', '♪'
                };

                await ProcessCdbFilesAsync(dataSourcePath, (content) =>
                {
                    // Tokenize: duyệt từng ký tự, chỉ giữ lại chữ, số và các ký tự trong danh sách cho phép.
                    StringBuilder tokenBuilder = new StringBuilder();
                    foreach (char c in content)
                    {
                        if (char.IsLetter(c) || char.IsDigit(c) || allowedSpecialChars.Contains(c))
                        {
                            tokenBuilder.Append(c);
                        }
                        else
                        {
                            if (tokenBuilder.Length > 0)
                            {
                                ProcessToken(tokenBuilder.ToString(), terms);
                                tokenBuilder.Clear();
                            }
                        }
                    }
                    if (tokenBuilder.Length > 0)
                    {
                        ProcessToken(tokenBuilder.ToString(), terms);
                    }
                });

                // Sắp xếp theo bảng chữ cái (theo dạng hiển thị từ)
                var sortedTerms = terms.Values.OrderBy(t => t, StringComparer.CurrentCulture).ToList();
                // Ghi kết quả vào file, mỗi từ một dòng
                string outputPath = Path.Combine(dataFolderPath, "terminology.txt");
                using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
                {
                    foreach (var term in sortedTerms)
                    {
                        await writer.WriteLineAsync(term);
                    }
                }

                return (true, $"Successfully extracted {sortedTerms.Count} terms into file: {outputPath}", outputPath);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        // Hàm xử lý từng token (từ) sau khi tách từ
        private static void ProcessToken(string token, Dictionary<string, string> terms)
        {
            // Loại bỏ các từ chỉ gồm 1 ký tự
            if (token.Length < 2) return;
            // Loại bỏ từ chỉ gồm các chữ số
            if (token.All(c => char.IsDigit(c))) return;

            bool hasLetter = token.Any(c => char.IsLetter(c));
            string normalized;

            if (hasLetter)
            {
                // Nếu tất cả các chữ cái trong token đều là chữ hoa thì giữ nguyên, ngược lại chuyển về chữ thường
                bool allLettersUpper = token.Where(c => char.IsLetter(c)).All(c => char.IsUpper(c));
                normalized = allLettersUpper ? token : token.ToLowerInvariant();
            }
            else
            {
                normalized = token; // Nếu không có chữ cái, giữ nguyên
            }
            // Thêm từ vào dictionary nếu chưa tồn tại (dùng normalized làm key)
            if (!terms.ContainsKey(normalized))
            {
                terms[normalized] = normalized;
            }
        }


        public static async Task<(bool result, string message, string filePath)> ExtractSpecialCharacters(string dataSourcePath, string dataFolderPath)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                });

                var (count, outputPath) = await Task.Run(async () =>
                {
                    HashSet<char> specialChars = new HashSet<char>();

                    HashSet<char> unwantedChars = new HashSet<char>
            {
                '\u200B', '\u200C', '\u200D', '\u200E',
                '\u200F', '\uFEFF', '\u2028', '\u2029'
            };

                    specialChars.UnionWith(new char[]
                    {
                '①', '②', '③', '④', '⑤', '⑥', '⑦', '⑧', '⑨', '⑩',
                '⑪', '⑫', '⑬', '⑭', '⑮', '⑯', '⑰', '⑱', '⑲', '⑳'
                    });

                    string outputPath = Path.Combine(
                        dataFolderPath,
                        "SpecialCharacters.txt");

                    // Đọc file cũ
                    if (File.Exists(outputPath))
                    {
                        using (var reader = new StreamReader(outputPath, Encoding.UTF8))
                        {
                            string existingContent = await reader.ReadToEndAsync();

                            foreach (char c in existingContent)
                            {
                                specialChars.Add(c);
                            }
                        }
                    }

                    // Xử lý CDB
                    await ProcessCdbFilesAsync(dataSourcePath, content =>
                    {
                        foreach (char c in content)
                        {
                            if (char.IsControl(c) ||
                                unwantedChars.Contains(c))
                                continue;

                            UnicodeCategory category = Char.GetUnicodeCategory(c);

                            if (!char.IsLetterOrDigit(c) &&
                                c != ' ' &&
                                c != '\r' &&
                                c != '\n' &&
                                c != '\t')
                            {
                                if (c < 32 || c > 126)
                                {
                                    if (category != UnicodeCategory.UppercaseLetter &&
                                        category != UnicodeCategory.LowercaseLetter &&
                                        category != UnicodeCategory.TitlecaseLetter &&
                                        category != UnicodeCategory.ModifierLetter &&
                                        category != UnicodeCategory.OtherLetter)
                                    {
                                        specialChars.Add(c);
                                    }
                                }
                            }
                        }
                    });

                    // Xử lý Lua
                    await ProcessLuaFilesAsync(dataSourcePath, content =>
                    {
                        foreach (char c in content)
                        {
                            if (char.IsControl(c) ||
                                unwantedChars.Contains(c))
                                continue;

                            UnicodeCategory category = Char.GetUnicodeCategory(c);

                            if (!char.IsLetterOrDigit(c) &&
                                c != ' ' &&
                                c != '\r' &&
                                c != '\n' &&
                                c != '\t')
                            {
                                if (c < 32 || c > 126)
                                {
                                    if (category != UnicodeCategory.UppercaseLetter &&
                                        category != UnicodeCategory.LowercaseLetter &&
                                        category != UnicodeCategory.TitlecaseLetter &&
                                        category != UnicodeCategory.ModifierLetter &&
                                        category != UnicodeCategory.OtherLetter)
                                    {
                                        specialChars.Add(c);
                                    }
                                }
                            }
                        }
                    });

                    // Loại bỏ ký tự ASCII và full-width ASCII
                    specialChars.RemoveWhere(c =>
                        (c >= 32 && c <= 126) ||
                        (c >= 0xFF01 && c <= 0xFF5E));

                    // Loại bỏ control / whitespace
                    specialChars.RemoveWhere(c =>
                        char.IsControl(c) ||
                        char.IsWhiteSpace(c));

                    // Sắp xếp
                    var sortedChars = specialChars
                        .OrderBy(c => (int)c)
                        .ToList();

                    // Ghi file
                    using (var writer = new StreamWriter(
                        outputPath,
                        false,
                        Encoding.UTF8))
                    {
                        foreach (var c in sortedChars)
                        {
                            await writer.WriteLineAsync(c.ToString());
                        }
                    }

                    return (sortedChars.Count, outputPath);
                });
                return (true, $"Successfully extracted {count} special characters.", outputPath);

                await Task.Run(async () =>
                {
                    // Tập hợp để lưu các ký tự đặc biệt, tránh trùng lặp
                    HashSet<char> specialChars = new HashSet<char>();
                    HashSet<char> unwantedChars = new HashSet<char> { '\u200B', '\u200C', '\u200D', '\u200E', '\u200F', '\uFEFF', '\u2028', '\u2029' };

                    specialChars.UnionWith(new char[]
                    {
                        '①', '②', '③', '④', '⑤', '⑥', '⑦', '⑧', '⑨', '⑩',
                        '⑪', '⑫', '⑬', '⑭', '⑮', '⑯', '⑰', '⑱', '⑲', '⑳'
                    });

                    // Đọc file kết quả cũ nếu có
                    string outputPath = Path.Combine(dataFolderPath, "SpecialCharacters.txt");

                    if (File.Exists(outputPath))
                    {
                        using (var reader = new StreamReader(outputPath, Encoding.UTF8))
                        {
                            string existingContent = await reader.ReadToEndAsync();
                            lock (specialChars)
                            {
                                foreach (char c in existingContent)
                                {
                                    specialChars.Add(c);
                                }
                            }
                        }
                    }

                    // Đọc dữ liệu từ tất cả các file .cdb
                    await ProcessCdbFilesAsync(dataSourcePath, (content) =>
                    {
                        foreach (char c in content)
                        {
                            if (char.IsControl(c) || c == '\u200B' || c == '\uFEFF' || c == '\u2028') continue;
                            UnicodeCategory category = Char.GetUnicodeCategory(c);
                            // Kiểm tra nếu là ký tự đặc biệt (không phải chữ cái hoặc chữ số)
                            if (!char.IsLetterOrDigit(c) && c != ' ' && c != '\r' && c != '\n' && c != '\t' && !unwantedChars.Contains(c))
                            {
                                if (c < 32 || c > 126)
                                {
                                    if (category != UnicodeCategory.UppercaseLetter &&  // Chữ in hoa
                                        category != UnicodeCategory.LowercaseLetter &&  // Chữ thường
                                        category != UnicodeCategory.TitlecaseLetter &&  // Chữ có dạng đặc biệt (ví dụ: "ǅ")
                                        category != UnicodeCategory.ModifierLetter &&   // Chữ có dấu
                                        category != UnicodeCategory.OtherLetter)        // Chữ từ các hệ chữ khác (Trung, Nhật, Việt...)
                                    {
                                        lock (specialChars)
                                        {
                                            specialChars.Add(c);
                                        }
                                    }
                                }
                            }
                        }
                    });

                    await ProcessLuaFilesAsync(dataSourcePath, (content) =>
                    {
                        foreach (char c in content)
                        {
                            if (char.IsControl(c) || c == '\u200B' || c == '\uFEFF' || c == '\u2028') continue;
                            UnicodeCategory category = Char.GetUnicodeCategory(c);
                            if (!char.IsLetterOrDigit(c) && c != ' ' && c != '\r' && c != '\n' && c != '\t' && !unwantedChars.Contains(c))
                            {
                                if (c < 32 || c > 126)
                                {
                                    if (category != UnicodeCategory.UppercaseLetter &&  // Chữ in hoa
                                        category != UnicodeCategory.LowercaseLetter &&  // Chữ thường
                                        category != UnicodeCategory.TitlecaseLetter &&  // Chữ có dạng đặc biệt (ví dụ: "ǅ")
                                        category != UnicodeCategory.ModifierLetter &&   // Chữ có dấu
                                        category != UnicodeCategory.OtherLetter)        // Chữ từ các hệ chữ khác (Trung, Nhật, Việt...)
                                    {
                                        lock (specialChars)
                                        {
                                            specialChars.Add(c);
                                        }
                                    }
                                }
                            }
                        }
                    });

                    // Loại bỏ các ký tự có thể gõ trên bàn phím
                    specialChars.RemoveWhere(c => (c >= 32 && c <= 126) || (c >= 0xFF01 && c <= 0xFF5E));
                    // Loại bỏ các ký tự điều khiển và khoảng trắng đặc biệt
                    specialChars.RemoveWhere(c => char.IsControl(c) || char.IsWhiteSpace(c));

                    // Sắp xếp ký tự đặc biệt theo mã Unicode
                    var sortedChars = specialChars.OrderBy(c => (int)c).ToList();
                    // Ghi kết quả vào file
                    using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
                    {
                        foreach (var term in sortedChars)
                        {
                            await writer.WriteLineAsync(term);
                        }
                    }
                    return (sortedChars.Count, outputPath);
                }).ContinueWith(task =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (task.IsFaulted)
                        {
                            CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                $"{CMess.errorOcc.ToText()} {task.Exception?.InnerException?.Message}", new[] { CMess.ok.ToText() });
                        }
                        else
                        {
                            var (count, path) = task.Result;
                            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                                $"Successfully extracted {count} special characters to file: {path}", new[] { CMess.ok.ToText() });
                        }
                    });
                }, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Mouse.OverrideCursor = null;
                });
            }
        }
        static async Task ProcessCdbFilesAsync(string rootPath, Action<string> processContentAction)
        {
            // Tìm tất cả các file .cdb trong thư mục và các thư mục con
            string[] cdbFiles = Directory.GetFiles(rootPath, "*.cdb", SearchOption.AllDirectories);

            foreach (string filePath in cdbFiles)
            {
                try
                {
                    // Tạo chuỗi kết nối SQLite
                    string connectionString = $"Data Source={filePath};Version=3;";

                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        // Truy vấn lấy dữ liệu từ bảng texts
                        string query = "SELECT name, desc FROM texts";
                        using (SQLiteCommand command = new SQLiteCommand(query, connection))
                        {
                            using (DbDataReader reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    // Xử lý cột name
                                    if (!reader.IsDBNull(0))
                                    {
                                        string name = reader.GetString(0);
                                        processContentAction(name);
                                    }

                                    // Xử lý cột desc
                                    if (!reader.IsDBNull(1))
                                    {
                                        string desc = reader.GetString(1);
                                        processContentAction(desc);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi xử lý file {filePath}: {ex.Message}");
                }
            }
        }

        static async Task ProcessLuaFilesAsync(string rootPath, Action<string> processContentAction)
        {
            // Tìm tất cả các file .lua trong thư mục và các thư mục con
            string[] luaFiles = Directory.GetFiles(rootPath, "*.lua", SearchOption.AllDirectories);
            Debug.WriteLine($"Tìm thấy {luaFiles.Length} file .lua");

            foreach (string filePath in luaFiles)
            {
                try
                {
                    Debug.WriteLine($"Đang xử lý file: {Path.GetFileName(filePath)}");
                    // Đọc nội dung file (với mã hóa UTF8)
                    using (var reader = new StreamReader(filePath, Encoding.UTF8))
                    {
                        string content = await reader.ReadToEndAsync();
                        processContentAction(content);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi xử lý file {filePath}: {ex.Message}");
                }
            }
        }

        public static async Task ExtractUnicodeAsync(string inputPath, string outputPath, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;

            using (var reader = new StreamReader(inputPath, encoding))
            using (var writer = new StreamWriter(outputPath, false, encoding))
            {
                string line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.Length == 0)
                    {
                        await writer.WriteLineAsync("EMPTY_LINE");
                        continue;
                    }

                    int codePoint;

                    if (line.Length >= 2 && char.IsSurrogatePair(line[0], line[1]))
                    {
                        // Unicode ngoài BMP (emoji, symbol...)
                        codePoint = char.ConvertToUtf32(line[0], line[1]);
                    }
                    else
                    {
                        // BMP
                        codePoint = line[0];
                    }

                    await writer.WriteLineAsync($"U+{codePoint:X4}");
                }
            }
        }
    }
}
