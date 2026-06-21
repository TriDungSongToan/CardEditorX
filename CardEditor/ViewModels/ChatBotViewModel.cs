using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Data.SQLite;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Services;
using CMess = CardEditor.Localization.Language;
using System.Media;
using CardEditor.Localization;

namespace CardEditor.ViewModels
{
    public class ChatBotViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<ChatBotViewModel> _instance = new Lazy<ChatBotViewModel>(() => new ChatBotViewModel());
        public static ChatBotViewModel Instance => _instance.Value;

        public string _user = "User";
        private string _dbPath;
        private string apiEndpoint = string.Empty;
        private string promptTemplate = string.Empty;
        private string apiKey = string.Empty;
        private string ExeFilePath => CardEditor.Models.AppContext.Instance.ExeFilePath;
        private string DataFolderPath => CardEditor.Models.AppContext.Instance.DataFolderPath;
        private string _chatHeader = string.Empty;
        private SQLiteConnection _connection;
        private static readonly HttpClient _httpClient = new HttpClient();
        public ObservableCollection<ChatMessage> ChatMessages { get; set; } = new ObservableCollection<ChatMessage>();
        public ObservableCollection<ChatSession> ChatSessions { get; set; } = new ObservableCollection<ChatSession>();
        public string ChatHeader
        {
            get => _chatHeader;
            set
            {
                _chatHeader = value;
                OnPropertyChanged();
            }
        }
        private ChatBotViewModel()
        {
            LoadChatConfig();

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                ChatSessions.Add(new ChatSession { Id = 0, Name = "Sample Chat", CreatedAt = DateTime.Now });
                ChatMessages.Add(new ChatMessage
                {
                    Sender = "AI",
                    Message = "Designer Mode",
                    MessageType = MessageType.Text,
                    CreatedAt = DateTime.Now
                });
                return;
            }
            else
            {
                InitializeDatabase();
            }
        }
        public void LoadChatConfig()
        {
            _user = ConfigViewModel.Instance.userSetting.UserName;
            apiEndpoint = ConfigViewModel.Instance.userSetting.ApiEndpoint;

            string AIPromptDefault = ConfigurationManager.AppSettings["AIPromptDefault"];
            string AIPrompt1 = ConfigurationManager.AppSettings["AIPrompt1"];
            string AIPrompt2 = ConfigurationManager.AppSettings["AIPrompt2"];
            string AIPrompt3 = ConfigurationManager.AppSettings["AIPrompt3"];
            string AIPrompt4 = ConfigurationManager.AppSettings["AIPrompt4"];
            string AIPrompt5 = ConfigurationManager.AppSettings["AIPrompt5"];
            string AIPrompt6 = ConfigurationManager.AppSettings["AIPrompt6"];
            promptTemplate = $"{AIPromptDefault}\n{AIPrompt1}\n{AIPrompt2}\n{AIPrompt3}\n{AIPrompt4}\n{AIPrompt5}\n{AIPrompt6}";

            apiKey = ConfigViewModel.Instance.userSetting.ApiKey;
        }
        /*
        public async Task CacheFilesAsync()
        {
            await Task.Run(() => CacheFiles());
        }
        public void CacheFiles()
        {
            _fileCache.Clear();
            try
            {
                // Duyệt đệ quy tất cả file *.cdb và *.lua
                var files = Directory.EnumerateFiles(_dataSourcePath, "*.*", SearchOption.AllDirectories)
                    .Where(f => f.EndsWith(".cdb", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".lua", StringComparison.OrdinalIgnoreCase));
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    _fileCache[fileName] = file; // Lưu tên file và đường dẫn đầy đủ
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                System.Diagnostics.Debug.WriteLine($"Error caching files: {ex.Message}");
            }
        }
        */
        private void InitializeDatabase()
        {
            string chatsFolder = Path.Combine(DataFolderPath, "Chats");
            if (!Directory.Exists(chatsFolder))
            {
                Directory.CreateDirectory(chatsFolder);
            }

            string dbFilePath = Path.Combine(chatsFolder, "Chat.db");
            if (File.Exists(dbFilePath))
            {
                if (_connection != null) return;
                _dbPath = dbFilePath;
                _connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                _connection.Open();
            }
            else
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(chatsFolder, "Chat.db")))
                {
                    int result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                        $"{CMess.filealreadyExit.ToText()} {CMess.QuestOverwrite.ToText()}", new[] { CMess.yes.ToText(), CMess.no.ToText() });
                    if (result != 0) return;
                }
                var (resultCreate, createdDbPath) = CreateFileServices.CreateChatDatabase(chatsFolder, "Chat.db");
                if (resultCreate)
                {
                    if (!string.IsNullOrEmpty(createdDbPath))
                    {
                        _dbPath = createdDbPath;
                        _connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                        _connection.Open();
                    }
                    else
                    {
                        CMSG.Show("Error", CMSG.MessageBoxIconType.Error,
                            "Error creating Chat Database.", new[] { "Ok" });
                    }
                }
                else
                {
                    CMSG.Show("Error", CMSG.MessageBoxIconType.Error,
                        $"An error occurred: {createdDbPath}", new[] { CMess.ok.ToText() });
                }
            }
        }
        /*
        public async Task CreateCardIndexAsync()
        {
            await Task.Run(() => CreateCardIndex());
        }
        private void CreateCardIndex()
        {
            try
            {
                using var indexDb = new SQLiteConnection($"Data Source={_indexDbPath};Version=3;");
                indexDb.Open();
                using (var cmd = indexDb.CreateCommand())
                {
                    // Tạo bảng cards nếu chưa tồn tại
                    cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS cards (
                        id INTEGER,
                        name TEXT,
                        file TEXT,
                        PRIMARY KEY (id, file)
                    )";
                    cmd.ExecuteNonQuery();
                }

                // Xóa dữ liệu cũ để cập nhật index
                using (var cmd = indexDb.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM cards";
                    cmd.ExecuteNonQuery();
                }

                // Duyệt các file *.cdb từ cache
                var cdbFiles = _fileCache.Where(f => f.Key.EndsWith(".cdb", StringComparison.OrdinalIgnoreCase))
                                        .Select(f => f.Value);
                foreach (string cdbFile in cdbFiles)
                {
                    try
                    {
                        using var connection = new SQLiteConnection($"Data Source={cdbFile};Version=3;");
                        connection.Open();
                        using var command = connection.CreateCommand();
                        command.CommandText = "SELECT d.id, t.name FROM datas d JOIN texts t ON d.id = t.id";
                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            using var insertCmd = indexDb.CreateCommand();
                            insertCmd.CommandText = "INSERT OR REPLACE INTO cards (id, name, file) VALUES ($id, $name, $file)";
                            insertCmd.Parameters.AddWithValue("$id", reader.GetInt32(0));
                            insertCmd.Parameters.AddWithValue("$name", reader.GetString(1));
                            insertCmd.Parameters.AddWithValue("$file", cdbFile);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error indexing {cdbFile}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating index: {ex.Message}");
            }
        }
        
        private string GetChatContext(int chatId)
        {
            var context = new StringBuilder();
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT sender, message FROM Messages WHERE chat_id = $chatId ORDER BY created_at";
            command.Parameters.AddWithValue("$chatId", chatId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                context.AppendLine($"{reader.GetString(0)}: {reader.GetString(1)}");
            }
            return context.ToString();
        }
        
        private string GetCardContext(string input)
        {
            var context = new StringBuilder();
            int? targetId = null;
            string targetName = null;
            string targetDbFile = input.ToLower().Contains("cards.cdb") ? "cards.delta.cdb" : null;

            // Phân tích input
            if (System.Text.RegularExpressions.Regex.Match(input, @"card id (\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase) is { Success: true } idMatch)
            {
                targetId = int.Parse(idMatch.Groups[1].Value);
            }
            if (System.Text.RegularExpressions.Regex.Match(input, @"card (?:name|text|script) ""?([^""]+)""?", System.Text.RegularExpressions.RegexOptions.IgnoreCase) is { Success: true } nameMatch)
            {
                targetName = nameMatch.Groups[1].Value;
            }

            var cdbFiles = new List<string>();
            try
            {
                using var indexDb = new SQLiteConnection($"Data Source={_indexDbPath};Version=3;");
                indexDb.Open();
                using var command = indexDb.CreateCommand();
                command.CommandText = "SELECT DISTINCT file FROM cards";
                if (targetId.HasValue || !string.IsNullOrEmpty(targetName))
                {
                    command.CommandText += " WHERE ";
                    if (targetId.HasValue)
                    {
                        command.CommandText += "id = $id";
                        command.Parameters.AddWithValue("$id", targetId.Value);
                    }
                    if (!string.IsNullOrEmpty(targetName))
                    {
                        if (targetId.HasValue) command.CommandText += " OR ";
                        command.CommandText += "name LIKE $name";
                        command.Parameters.AddWithValue("$name", $"%{targetName}%");
                    }
                }
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string file = reader.GetString(0);
                    if (targetDbFile == null || Path.GetFileName(file) == targetDbFile)
                        cdbFiles.Add(file);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error querying index: {ex.Message}");
                // Fallback: Sử dụng _fileCache nếu index lỗi
                cdbFiles = _fileCache.Where(f => f.Key.EndsWith(".cdb", StringComparison.OrdinalIgnoreCase))
                                     .Select(f => f.Value).ToList();
            }
            // Xử lý các file *.cdb
            foreach (string cdbFile in cdbFiles)
            {
                try
                {
                    using var connection = new SQLiteConnection($"Data Source={cdbFile};Version=3;");
                    connection.Open();
                    using var command = connection.CreateCommand();
                    command.CommandText = @"
                    SELECT d.id, d.atk, d.def, d.type, t.name, t.desc
                    FROM datas d
                    JOIN texts t ON d.id = t.id";
                    if (targetId.HasValue || !string.IsNullOrEmpty(targetName))
                    {
                        command.CommandText += " WHERE ";
                        if (targetId.HasValue)
                        {
                            command.CommandText += "d.id = $id";
                            command.Parameters.AddWithValue("$id", targetId.Value);
                        }
                        if (!string.IsNullOrEmpty(targetName))
                        {
                            if (targetId.HasValue) command.CommandText += " OR ";
                            command.CommandText += "t.name LIKE $name";
                            command.Parameters.AddWithValue("$name", $"%{targetName}%");
                        }
                    }
                    else
                    {
                        command.CommandText += " LIMIT 10";
                    }

                    using var reader = command.ExecuteReader();
                    context.AppendLine($"File: {Path.GetFileName(cdbFile)}");
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        context.AppendLine($"Card ID: {id}, Name: {reader.GetString(4)}, Desc: {reader.GetString(5)}, ATK: {reader.GetInt32(1)}, DEF: {reader.GetInt32(2)}, Type: {reader.GetInt32(3)}");

                        if (input.ToLower().Contains("card script") || targetId.HasValue || !string.IsNullOrEmpty(targetName))
                        {
                            string luaKey = $"c{id}.lua";
                            if (_fileCache.TryGetValue(luaKey, out string luaFile))
                            {
                                context.AppendLine($"Script c{luaKey}:\n{File.ReadAllText(luaFile)}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    context.AppendLine($"Error reading {cdbFile}: {ex.Message}");
                }
            }


            //// Duyệt đệ quy các file *.cdb trong _dataSourcePath
            //var cdbFiles = _fileCache.Where(f => f.Key.EndsWith(".cdb", StringComparison.OrdinalIgnoreCase))
            //                     .Select(f => f.Value);

            //foreach (string cdbFile in cdbFiles)
            //{
            //    if (targetDbFile != null && Path.GetFileName(cdbFile) != targetDbFile) continue;

            //    string dbPath = Path.Combine(_dataSourcePath, Path.GetFileName(cdbFile));
            //    try
            //    {
            //        using var connection = new SqliteConnection($"Data Source={dbPath}");
            //        connection.Open();
            //        using var command = connection.CreateCommand();
            //        command.CommandText = @"
            //    SELECT d.id, d.atk, d.def, d.type, t.name, t.desc
            //    FROM datas d
            //    JOIN texts t ON d.id = t.id";
            //        if (targetId.HasValue || !string.IsNullOrEmpty(targetName))
            //        {
            //            command.CommandText += " WHERE ";
            //            if (targetId.HasValue)
            //            {
            //                command.CommandText += "d.id = $id";
            //                command.Parameters.AddWithValue("$id", targetId.Value);
            //            }
            //            if (!string.IsNullOrEmpty(targetName))
            //            {
            //                if (targetId.HasValue) command.CommandText += " OR ";
            //                command.CommandText += "t.name LIKE $name";
            //                command.Parameters.AddWithValue("$name", $"%{targetName}%");
            //            }
            //        }
            //        else
            //        {
            //            // Giới hạn số lượng thẻ để tránh context quá lớn
            //            command.CommandText += " LIMIT 50";
            //        }

            //        using var reader = command.ExecuteReader();
            //        context.AppendLine($"File: {Path.GetFileName(cdbFile)}");
            //        while (reader.Read())
            //        {
            //            int id = reader.GetInt32(0);
            //            context.AppendLine($"Card ID: {id}, Name: {reader.GetString(4)}, Desc: {reader.GetString(5)}, ATK: {reader.GetInt32(1)}, DEF: {reader.GetInt32(2)}, Type: {reader.GetInt32(3)}");

            //            // Chỉ đọc file Lua nếu yêu cầu card script hoặc tìm kiếm cụ thể
            //            if (input.ToLower().Contains("card script") || targetId.HasValue || !string.IsNullOrEmpty(targetName))
            //            {
            //                string luaFile = Directory.EnumerateFiles(_dataSourcePath, $"c{id}.lua", SearchOption.AllDirectories).FirstOrDefault();
            //                if (luaFile != null)
            //                {
            //                    context.AppendLine($"Script c{id}.lua:\n{File.ReadAllText(luaFile)}");
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        context.AppendLine($"Error reading {cdbFile}: {ex.Message}");
            //    }
            //}
            // Thêm file Lua không phải card
            if (input.ToLower().Contains("script") && !input.ToLower().Contains("card script"))
            {
                var luaFiles = _fileCache.Where(f => f.Key.EndsWith(".lua", StringComparison.OrdinalIgnoreCase) &&
                                                   !System.Text.RegularExpressions.Regex.IsMatch(f.Key, @"c\d+\.lua"))
                                        .Select(f => f.Value);
                foreach (string luaFile in luaFiles)
                {
                    context.AppendLine($"Non-card script {Path.GetFileName(luaFile)}:\n{File.ReadAllText(luaFile)}");
                }
            }
            return context.ToString();
        }
        
        private int SaveCardToDatabase(string name, string desc, string script, string cdbFile)
        {
            string dbPath = Path.Combine(_dataSourcePath, cdbFile);
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                using var cmdDatas = connection.CreateCommand();
                cmdDatas.CommandText = @"
                    INSERT INTO datas (id, ot, alias, setcode, type, atk, def, level, race, attribute, category)
                    VALUES ($id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    SELECT last_insert_rowid();";
                int id = new Random().Next(100000, 999999); // Giả lập ID
                cmdDatas.Parameters.AddWithValue("$id", id);
                id = Convert.ToInt32(cmdDatas.ExecuteScalar());

                using var cmdTexts = connection.CreateCommand();
                cmdTexts.CommandText = @"
                    INSERT INTO texts (id, name, desc, str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12, str13, str14, str15, str16)
                    VALUES ($id, $name, $desc, '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '')";
                cmdTexts.Parameters.AddWithValue("$id", id);
                cmdTexts.Parameters.AddWithValue("$name", name);
                cmdTexts.Parameters.AddWithValue("$desc", desc ?? "");
                cmdTexts.ExecuteNonQuery();

                if (script != null)
                {
                    string luaPath = Path.Combine(_dataSourcePath, $"c{id}.lua");
                    File.WriteAllText(luaPath, script);
                }

                transaction.Commit();
                return id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        */
        public async Task<string> CallAI(string input, MessageType messageType)
        {
            try
            {
                if (messageType != MessageType.Text)
                {
                    return $"Received {messageType}, processing not implemented yet.";
                }

                string prompt = string.Format(promptTemplate, input);

                if (string.IsNullOrEmpty(apiEndpoint) || string.IsNullOrEmpty(apiKey))
                {
                    System.Diagnostics.Debug.WriteLine("API Endpoint or API Key is missing in Settings");
                    return CMess.apiKeyNotConfig.ToText();
                }
                var requestBody = new { inputs = prompt };
                System.Diagnostics.Debug.WriteLine($"Prompt: {prompt}");
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var response = await _httpClient.PostAsync(apiEndpoint, content);
                System.Diagnostics.Debug.WriteLine($"API Status Code: {response.StatusCode}");
                string responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"API Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var responseObj = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(responseContent);
                    string generatedText = responseObj[0]["generated_text"].Trim();
                    generatedText = ValidateResponse(generatedText);
                    // Lấy phần trả lời sau "Assistant:"
                    int assistantIndex = generatedText.IndexOf("Assistant:");
                    if (assistantIndex != -1)
                    {
                        return generatedText.Substring(assistantIndex + "Assistant:".Length).Trim();
                    }
                    // Fallback
                    return generatedText;
                }
                return $"AI Error: {response.StatusCode} - {responseContent}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                return $"{CMess.errorOcc.ToText()} {ex.Message}";
            }
        }
        public async Task<string> CallGrok(string input, MessageType messageType)
        {
            try
            {
                if (messageType != MessageType.Text)
                {
                    return $"Received {messageType}, processing not implemented yet.";
                }

                string prompt = string.Format(promptTemplate, input);

                if (string.IsNullOrEmpty(apiEndpoint) || string.IsNullOrEmpty(apiKey))
                {
                    System.Diagnostics.Debug.WriteLine("API Endpoint or API Key is missing in Settings");
                    return CMess.apiKeyNotConfig.ToText();
                }

                var requestBody = new
                {
                    model = "grok-1", // Thay bằng mô hình cụ thể (grok-1, grok-beta, v.v.)
                    prompt = prompt,
                    max_tokens = 200 // Giới hạn độ dài phản hồi
                };

                System.Diagnostics.Debug.WriteLine($"Prompt: {prompt}");
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var response = await _httpClient.PostAsync(apiEndpoint, content);
                System.Diagnostics.Debug.WriteLine($"API Status Code: {response.StatusCode}");
                string responseContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"API Response Content: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"API Response Content: {responseContent}");
                    //var responseObj = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    //string generatedText = responseObj["choices"]?[0]?["text"]?.ToString().Trim() ?? "No response";
                    //generatedText = ValidateResponse(generatedText);
                    return responseContent;
                }
                return $"Grok Error: {response.StatusCode} - {responseContent}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                return $"{CMess.errorOcc.ToText()} {ex.Message}";
            }
        }
        private string ValidateResponse(string response)
        {
            var validDomains = new[] { "yugipedia.com", "db.ygoresources.com", "github.com/ProjectIgnis" };
            var uriRegex = new Regex(@"(https?://[^\s]+)");
            var matches = uriRegex.Matches(response);
            foreach (Match match in matches)
            {
                try
                {
                    var uri = new Uri(match.Value);
                    if (!validDomains.Any(domain => uri.Host.Contains(domain)))
                    {
                        response = response.Replace(match.Value, "[Invalid URL removed]");
                    }
                }
                catch
                {
                    response = response.Replace(match.Value, "[Invalid URL removed]");
                }
            }
            return response;
        }
        public void SaveMessage(ChatMessage message)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            InitializeDatabase();

            if (!ChatSessions.Any())
            {
                CreateNewChat();
            }

            using var command = _connection.CreateCommand();
            command.CommandText = "INSERT INTO Messages (chat_id, sender, message, message_type, created_at) VALUES ($chatId, $sender, $message, $messageType, $createdAt)";
            command.Parameters.AddWithValue("$chatId", ChatSessions.FirstOrDefault()?.Id ?? throw new InvalidOperationException("No chat session exists"));
            command.Parameters.AddWithValue("$sender", message.Sender);
            command.Parameters.AddWithValue("$message", message.Message);
            command.Parameters.AddWithValue("$messageType", (int)message.MessageType);
            command.Parameters.AddWithValue("$createdAt", message.CreatedAt);
            command.ExecuteNonQuery();
        }
        public async Task LoadChatSessions()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            InitializeDatabase();
            ChatSessions.Clear();
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT id, name, created_at FROM Chats ORDER BY created_at DESC";
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ChatSessions.Add(new ChatSession
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2)
                });
            }
        }
        public async Task LoadChatMessages(int chatId)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            InitializeDatabase();
            ChatMessages.Clear();
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT sender, message, message_type, created_at FROM Messages WHERE chat_id = $chatId ORDER BY created_at";
            command.Parameters.AddWithValue("$chatId", chatId);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ChatMessages.Add(new ChatMessage
                {
                    Sender = reader.GetString(0),
                    Message = reader.GetString(1),
                    MessageType = (MessageType)reader.GetInt32(2),
                    CreatedAt = reader.GetDateTime(3)
                });
            }
            ChatHeader = ChatSessions.FirstOrDefault(s => s.Id == chatId)?.Name ?? string.Empty;
        }
        public void CreateNewChat()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            InitializeDatabase();
            string chatName = $"Chat {DateTime.Now:yyyy-MM-dd HH:mm}";
            int chatId;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Chats (name, created_at) VALUES ($name, $createdAt); SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("$name", chatName);
                command.Parameters.AddWithValue("$createdAt", DateTime.Now);
                chatId = Convert.ToInt32(command.ExecuteScalar());
            }

            ChatSessions.Insert(0, new ChatSession
            {
                Id = chatId,
                Name = chatName,
                CreatedAt = DateTime.Now
            });
            ChatMessages.Clear();
        }

        public async Task DeleteChat(int chatId)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            InitializeDatabase();
            using var command = _connection.CreateCommand();
            command.CommandText = "DELETE FROM Chats WHERE id = $chatId";
            command.Parameters.AddWithValue("$chatId", chatId);
            command.ExecuteNonQuery();
            ChatSessions.Remove(ChatSessions.FirstOrDefault(s => s.Id == chatId));
            if (ChatSessions.Any())
            {
                await LoadChatMessages(ChatSessions.First().Id);
            }
            else
            {
                ChatMessages.Clear();
            }
        }
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();

                ChatMessages.Clear();
                ChatSessions.Clear();
                ChatMessages = null;
                ChatSessions = null;
            }
        }

        public void SaveCurrentChat()
        {
            if (!ChatMessages.Any()) return;

            // Tạo đoạn chat mới
            string chatName = $"Chat {DateTime.Now:yyyy-MM-dd HH:mm}";
            int chatId;
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Chats (name, createdAt) VALUES ($name, $createdAt); SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("$name", chatName);
                command.Parameters.AddWithValue("$createdAt", DateTime.Now);
                chatId = Convert.ToInt32(command.ExecuteScalar());
            }

            // Lưu tin nhắn
            using (var command = _connection.CreateCommand())
            {
                foreach (var message in ChatMessages)
                {
                    command.CommandText = "INSERT INTO Messages (chat_id, sender, message, message_type, created_at) VALUES ($chatId, $sender, $message, $messageType, $createdAt)";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("$chatId", chatId);
                    command.Parameters.AddWithValue("$sender", message.Sender);
                    command.Parameters.AddWithValue("$message", message.Message);
                    command.Parameters.AddWithValue("$messageType", message.MessageType.ToString());
                    command.Parameters.AddWithValue("$createdAt", message.CreatedAt);
                    command.ExecuteNonQuery();
                }
            }

            // Cập nhật ChatSessions
            ChatSessions.Add(new ChatSession { Id = chatId, Name = chatName, CreatedAt = DateTime.Now });
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
