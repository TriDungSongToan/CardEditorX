using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Models;
using CardEditor.Constants;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.LoadData
{
    public class LoadArchiveService
    {
        /// <summary>
        /// Liệt kê các entry trong archive theo từng loại (DB, Script, Deck, Banlist).
        /// Chỉ trả về FullName (giữ cấu trúc thư mục gốc trong zip), không extract, không tạo temp.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="getListDB"></param>
        /// <param name="getListScript"></param>
        /// <param name="getListDeck"></param>
        /// <param name="getListBanlist"></param>
        /// <returns></returns>
        public static Task<(bool success, ArchiveFileList fileLists, string message)> ListsEntries(string archivePath,
            bool getListDB = true, bool getListScript = true, bool getListDeck = true, bool getListBanlist = true)
        {
            if (string.IsNullOrEmpty(archivePath) || !System.IO.File.Exists(archivePath))
                return Task.FromResult((false, (ArchiveFileList)null, CMess.fileNotExit.ToText()));

            return Task.Run(() =>
            {
                ZipArchive archive;
                try
                {
                    archive = ZipFile.OpenRead(archivePath);
                }
                catch (Exception ex)
                {
                    // Không mở được như một archive hợp lệ
                    return (false, (ArchiveFileList)null,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
                }

                using (archive)
                {
                    var fileList = new ArchiveFileList();

                    foreach (var entry in archive.Entries)
                    {
                        // entry.Name rỗng nghĩa là entry này là thư mục (directory entry), bỏ qua
                        if (string.IsNullOrEmpty(entry.Name)) continue;

                        string ext = Path.GetExtension(entry.Name);

                        if (getListBanlist && entry.Name.EndsWith(ConstantExtension.BanlistSuffix, StringComparison.OrdinalIgnoreCase))
                        {
                            fileList.BanlistEntries.Add(entry.FullName);
                            continue;
                        }
                        if (getListDB && 
                            (ConstantExtension.CardDBExtensions.Contains(ext) ||
                             ConstantExtension.ExcelExtensions.Contains(ext) ||
                             ConstantExtension.CedsExtensions.Contains(ext)))
                        {
                            fileList.DatabaseEntries.Add(entry.FullName);
                            continue;
                        }
                        if (getListScript && ConstantExtension.ScriptExtensions.Contains(ext))
                        {
                            fileList.ScriptEntries.Add(entry.FullName);
                            continue;
                        }
                        if (getListDeck && ConstantExtension.DeckExtensions.Contains(ext))
                        {
                            fileList.DeckEntries.Add(entry.FullName);
                            continue;
                        }
                    }

                    bool anyFound = fileList.DatabaseEntries.Count > 0
                        || fileList.ScriptEntries.Count > 0
                        || fileList.DeckEntries.Count > 0
                        || fileList.BanlistEntries.Count > 0;

                    string message = anyFound ? string.Empty : CMess.noValiDataFound.ToText();
                    return (true, fileList, message);
                }
            });
        }

        /// <summary>
        /// Lấy ra danh sách Fullname của các entry trong archive theo entryName.
        /// Tùy chọn lọc theo extension.
        /// </summary>
        /// <param name="archiveFilePath"></param>
        /// <param name="entryName"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static Task<(bool success, IEnumerable<string> fileLists, string message)>
            FindEntriesByName(string archiveFilePath, string entryName, string extension = null)
        {
            if (string.IsNullOrWhiteSpace(archiveFilePath) || !File.Exists(archiveFilePath))
                return Task.FromResult((false, (IEnumerable<string>)null, CMess.fileNotExit.ToText()));

            if (string.IsNullOrWhiteSpace(entryName))
                return Task.FromResult((false, (IEnumerable<string>)null, CMess.fileNotExit.ToText()));

            if (!string.IsNullOrWhiteSpace(extension))
                if (!extension.StartsWith(".")) extension = "." + extension;

            try
            {
                using var archive = ZipFile.OpenRead(archiveFilePath);

                var fileLists = archive.Entries.Where(e =>
                {
                    // Bỏ qua folder
                    if (string.IsNullOrEmpty(e.Name)) return false;

                    // Bước 1: lọc extension nếu có
                    if (!string.IsNullOrWhiteSpace(extension))
                    {
                        var ext = Path.GetExtension(e.Name);

                        if (!ext.Equals(extension, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                    // Bước 2: tìm kiếm chứa trong toàn bộ tên file

                    return e.Name.IndexOf(entryName, StringComparison.OrdinalIgnoreCase) >= 0;
                }).Select(e => e.FullName).ToList();

                return Task.FromResult((true, (IEnumerable<string>)fileLists, string.Empty));
            }
            catch (Exception ex)
            {
                return Task.FromResult((false, (IEnumerable<string>)null, ex.Message));
            }
        }

        /// <summary>
        /// Trích xuất các entry được chỉ định (theo FullName) ra thư mục Temp, giữ nguyên cấu trúc thư mục con.
        /// Entry nào không xử lý được (không tồn tại, hoặc path không hợp lệ) sẽ được liệt kê trong FailedEntries.
        /// Các entry còn lại vẫn được xử lý bình thường. Việc dọn dẹp Temp do Caller tự quản lý.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="entryFullNames"></param>
        /// <returns></returns>
        public static Task<(bool result, List<ExtractedEntry> extractedEntries, List<string> failedEntries, string message)> ExtractTempEntry
            (string archivePath, IEnumerable<string> entryFullNames)
        {
            if (string.IsNullOrWhiteSpace(archivePath) || !System.IO.File.Exists(archivePath))
                return Task.FromResult((false, (List<ExtractedEntry>)null, (List<string>)null, CMess.fileNotExit.ToText()));
            if (entryFullNames == null)
                return Task.FromResult((false, (List<ExtractedEntry>)null, (List<string>)null, CMess.fileNotExit.ToText()));

            return Task.Run(() =>
            {
                ZipArchive archive;
                try
                {
                    archive = ZipFile.OpenRead(archivePath);
                }
                catch (Exception ex)
                {
                    return (false, (List<ExtractedEntry>)null, (List<string>)null,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
                }

                using (archive)
                {
                    string tempDir = Path.Combine(Path.GetTempPath(), "CardEditorTemp", Path.GetFileNameWithoutExtension(archivePath));
                    Directory.CreateDirectory(tempDir);

                    // GetFullPath để chuẩn hoá path, dùng làm mốc so sánh chống Zip Slip
                    string tempDirFull = Path.GetFullPath(tempDir);

                    var extractedEntries = new List<ExtractedEntry>();
                    var failedEntries = new List<string>();

                    foreach (var entryFullName in entryFullNames)
                    {
                        var entry = archive.GetEntry(entryFullName);
                        if (entry == null)
                        {
                            failedEntries.Add(entryFullName);
                            continue;
                        }

                        // Giữ cấu trúc thư mục con theo FullName trong zip
                        string relativePath = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                        string destPath = Path.GetFullPath(Path.Combine(tempDirFull, relativePath));

                        // Chống Zip Slip: đảm bảo destPath thực sự nằm trong tempDir, không thoát ra ngoài qua "../"
                        if (!destPath.StartsWith(tempDirFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        {
                            failedEntries.Add(entryFullName);
                            continue;
                        }

                        try
                        {
                            string directory = Path.GetDirectoryName(destPath);
                            if (!string.IsNullOrEmpty(directory))
                                Directory.CreateDirectory(directory);
                            entry.ExtractToFile(destPath, overwrite: true);
                            extractedEntries.Add(new ExtractedEntry
                            {
                                EntryFullName = entry.FullName,
                                TempPath = destPath
                            });
                        }
                        catch
                        {
                            failedEntries.Add(entryFullName);
                        }
                    }

                    // Không tự ý gán message lỗi cụ thể vì không rõ CMess đã có key phù hợp chưa;
                    // Caller có thể tự kiểm tra failedEntries.Count > 0 để xử lý thông báo riêng.
                    return (true, extractedEntries, failedEntries, string.Empty);
                }
            });
        }

        /// <summary>
        /// Ghi các file đã chỉnh sửa trong Temp ngược lại vào archive gốc (ghi đè trực tiếp archivePath),
        /// theo cơ chế an toàn: làm việc trên 1 bản copy tạm, chỉ thay thế archive gốc khi MỌI THỨ đã thành công.
        /// Nếu có lỗi ở bất kỳ bước nào trước khi thay thế, archive gốc không hề bị đụng tới.
        /// Không hỗ trợ xoá Entry.
        /// </summary>
        /// <param name="entries">Danh sách (entryFullName, tempPath) cần ghi ngược vào archive.</param>
        /// <param name="createEntryIfMissing">
        /// Nếu entryFullName chưa tồn tại trong archive: true = tạo entry mới, false = coi là lỗi (thêm vào failedEntries).
        /// </param>
        public static Task<(bool result, List<string> failedEntries, string message)> SaveTempEntriesToArchive(
            string archivePath, List<(string entryFullName, string tempPath)> entries, bool createEntryIfMissing = true)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(archivePath))
                {
                    return (false, (List<string>)null, $"{string.Format(CMess.fileNotExit.ToText())} {archivePath}");
                }

                // Bản copy tạm PHẢI nằm CÙNG thư mục với archive gốc (cùng ổ đĩa),
                // vì File.Replace yêu cầu source/destination/backup cùng volume mới hoạt động đúng.
                string workingCopyPath = archivePath + $".{Guid.NewGuid():N}.tmp";
                File.Copy(archivePath, workingCopyPath, overwrite: true);

                var failedEntries = new List<string>();

                try
                {
                    using (var archive = ZipFile.Open(workingCopyPath, ZipArchiveMode.Update))
                    {
                        foreach (var (entryFullName, tempPath) in entries)
                        {
                            if (!File.Exists(tempPath))
                            {
                                failedEntries.Add(entryFullName);
                                continue;
                            }

                            var existingEntry = archive.GetEntry(entryFullName);
                            if (existingEntry == null && !createEntryIfMissing)
                            {
                                failedEntries.Add(entryFullName);
                                continue;
                            }

                            // Cách chuẩn để "ghi đè" nội dung 1 entry trong ZipArchiveMode.Update:
                            // xoá entry cũ (nếu có) rồi tạo lại entry mới từ file trong Temp.
                            existingEntry?.Delete();
                            archive.CreateEntryFromFile(tempPath, entryFullName, CompressionLevel.Optimal);
                        }
                    } // Dispose tại đây mới thực sự ghi mọi thay đổi xuống workingCopyPath
                }
                catch (Exception ex)
                {
                    // Ghi thất bại -> workingCopyPath có thể hỏng, xoá bỏ, KHÔNG đụng tới archive gốc
                    TryDeleteFile(workingCopyPath);
                    return (false, (List<string>)null,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
                }

                try
                {
                    // Thay thế archive gốc bằng bản đã cập nhật - gần như atomic, an toàn nếu crash giữa chừng
                    string backupPath = archivePath + ".bak";
                    File.Replace(workingCopyPath, archivePath, backupPath);
                    TryDeleteFile(backupPath);
                }
                catch (Exception ex)
                {
                    TryDeleteFile(workingCopyPath);
                    return (false, failedEntries,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
                }

                // Caller tự kiểm tra failedEntries.Count > 0 để biết có entry nào bị bỏ qua không.
                return (true, failedEntries, string.Empty);
            });
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // Bỏ qua lỗi khi dọn file tạm, không ảnh hưởng tới kết quả chính
            }
        }
        /// <summary>
        /// Tiện ích cho trường hợp chỉ cần lưu 1 entry — tái sử dụng cơ chế an toàn của SaveTempEntriesToArchive
        /// (copy tạm, chỉ thay thế archive gốc khi chắc chắn thành công) thay vì thao tác trực tiếp lên archivePath.
        /// </summary>
        public static async Task<(bool success, string message)> SaveEntryToZip(string archivePath, string entryFullName, string tempFilePath)
        {
            var (result, failedEntries, message) = await SaveTempEntriesToArchive(
                archivePath,
                new List<(string entryFullName, string tempPath)> { (entryFullName, tempFilePath) });

            if (!result) return (false, message);
            if (failedEntries.Count > 0) return (false, $"Không thể lưu entry: {entryFullName}");
            return (true, string.Empty);
        }

        public static (bool, List<(string tempPath, string entryName)>, string) ExtractDatabaseEntries(string archivePath)
        {
            ZipArchive archive;
            try
            {
                archive = ZipFile.OpenRead(archivePath);
            }
            catch (Exception ex)
            {
                // Không mở được như một archive hợp lệ
                return (false, null, $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
            }

            using (archive)
            {
                // Lọc ra những entry có đuôi nằm trong KnownDbExtensions
                var matchedEntries = archive.Entries
                    .Where(e => !string.IsNullOrEmpty(e.Name) && ConstantExtension.CardDBExtensions.Contains(Path.GetExtension(e.Name)))
                    .ToList();

                // Archive mở được, nhưng bên trong không có file nào khớp -> archiveOk = true, list rỗng
                if (matchedEntries.Count == 0)
                    return (true, new List<(string, string)>(), CMess.noValiDataFound.ToText());

                string tempDir = Path.Combine(Path.GetTempPath(), "CardEditorTemp", Path.GetFileNameWithoutExtension(archivePath));
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                // Đổi kiểu list để lưu CẢ đường dẫn tạm LẪN tên entry gốc
                var result = new List<(string tempPath, string entryName)>();
                foreach (var entry in matchedEntries)
                {
                    string destPath = Path.Combine(tempDir, entry.Name);
                    entry.ExtractToFile(destPath, overwrite: true);
                    result.Add((destPath, entry.FullName)); // lưu cặp (file tạm, tên gốc trong archive)
                }

                return (true, result, string.Empty);
            }
        }
        public static (bool, List<(string virtualPath, string entryName)>, string) ListDatabaseEntries(string archivePath)
        {
            ZipArchive archive;
            try
            {
                archive = ZipFile.OpenRead(archivePath);
            }
            catch (Exception ex)
            {
                return (false, null, $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
            }

            using (archive)
            {
                var matchedEntries = archive.Entries
                    .Where(e => !string.IsNullOrEmpty(e.Name) && ConstantExtension.CardDBExtensions.Contains(Path.GetExtension(e.Name)))
                    .ToList();

                if (matchedEntries.Count == 0)
                    return (true, new List<(string, string)>(), CMess.noValiDataFound.ToText());

                // Virtual root: cùng thư mục với archive, tên = tên archive không đuôi
                string virtualRoot = Path.Combine(
                    Path.GetDirectoryName(archivePath),
                    Path.GetFileNameWithoutExtension(archivePath));

                var result = matchedEntries.Select(entry =>
                {
                    // entry.FullName trong zip dùng '/' -> đổi sang '\' cho đúng Windows path
                    string relativePath = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                    string virtualPath = Path.Combine(virtualRoot, relativePath);
                    return (virtualPath, entry.FullName);
                }).ToList();

                return (true, result, string.Empty);
            }
        }
        public static List<(string tempPath, string entryName)> ExtractSelectedEntries(string archivePath, List<string> entryNames)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "CardEditorTemp",
                Path.GetFileNameWithoutExtension(archivePath));

            Directory.CreateDirectory(tempDir); // không xóa tempDir cũ nữa, vì giờ trích theo yêu cầu, không trích hết 1 lần

            var result = new List<(string, string)>();
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                foreach (var entryName in entryNames)
                {
                    var entry = archive.GetEntry(entryName);
                    if (entry == null) continue;

                    string destPath = Path.Combine(tempDir, entry.Name); // hoặc giữ cấu trúc thư mục nếu cần
                    entry.ExtractToFile(destPath, overwrite: true);
                    result.Add((destPath, entry.FullName));
                }
            }
            return result;
        }
    }
}
