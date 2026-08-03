using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Reflection;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for ChatBotControl.xaml
    /// </summary>
    /// 
    public class ChatMessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ChatMessage message)
            {
                switch (message.MessageType)
                {
                    case MessageType.Image: return ImageTemplate;
                    case MessageType.Video: return VideoTemplate;
                    case MessageType.File: return FileTemplate;
                    default: return TextTemplate;
                }
            }
            return TextTemplate;
        }
    }

    public partial class ChatBotControl : UserControl
    {
        private readonly string exeFilePath;
        private readonly string dataFolderPath;
        public ChatBotControl()
        {
            InitializeComponent();
            exeFilePath = Assembly.GetExecutingAssembly().Location;
            dataFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(exeFilePath), "data");
            Loaded += ChatBotControl_Loaded;
            ChatBotViewModel.Instance.ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;
            this.DataContext = UIConfigViewModel.Instance;
        }

        private void ChatMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ScrollChatListBoxToBottom();
        }

        private async void ChatBotControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeContentMenu();
            await ChatBotViewModel.Instance.LoadChatSessions();

            if (ChatBotViewModel.Instance.ChatSessions.Any())
            {
                await ChatBotViewModel.Instance.LoadChatMessages(ChatBotViewModel.Instance.ChatSessions.First().Id);
                ScrollChatListBoxToBottom();
            }
        }
        private void InitializeContentMenu()
        {
            ControlContextMenuService.Attach(MessageTextBox);
        }
        private async void ChatHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatHistory.SelectedItem is ChatSession session)
            {
                await ChatBotViewModel.Instance.LoadChatMessages(session.Id);
                ScrollChatListBoxToBottom();
            }
        }
        private void ScrollChatListBoxToBottom()
        {
            if (ChatListBox.Items.Count > 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ChatListBox.ScrollIntoView(ChatListBox.Items[ChatListBox.Items.Count - 1]);
                }), DispatcherPriority.Render);
            }
        }
        private async Task AddMessageAndGetResponse(ChatMessage message)
        {
            ChatBotViewModel.Instance.ChatMessages.Add(message);
            ChatBotViewModel.Instance.SaveMessage(message);
            var response = await ChatBotViewModel.Instance.CallAI(message.Message, message.MessageType);
            // var response = await ChatBotViewModel.Instance.CallGrok(message.Message, message.MessageType);
            var aiMessage = new ChatMessage
            {
                Sender = "AI",
                Message = response,
                MessageType = MessageType.Text,
                CreatedAt = DateTime.Now
            };
            ChatBotViewModel.Instance.ChatMessages.Add(aiMessage);
            ChatBotViewModel.Instance.SaveMessage(aiMessage);
        }
        private async Task HandlePaste()
        {
            if (Clipboard.ContainsImage())
            {
                var bitmap = Clipboard.GetImage();
                if (bitmap != null)
                {
                    string filePath = SaveClipboardImageToFile(bitmap);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var message = new ChatMessage
                        {
                            Sender = ChatBotViewModel.Instance._user,
                            Message = filePath,
                            MessageType = MessageType.Image,
                            CreatedAt = DateTime.Now
                        };
                        await AddMessageAndGetResponse(message);
                    }
                }
            }
            else if (Clipboard.ContainsText())
            {
                MessageTextBox.Text += Clipboard.GetText();
                MessageTextBox.CaretIndex = MessageTextBox.Text.Length;
            }
            else if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList();
                foreach (string file in files)
                {
                    string extension = System.IO.Path.GetExtension(file).ToLower();
                    MessageType messageType = extension switch
                    {
                        var ext when ext == ".png" || ext == ".jpg" => MessageType.Image,
                        var ext when ext == ".mp4" || ext == ".avi" => MessageType.Video,
                        _ => MessageType.File
                    };
                    string filePath = SaveFileToChatFolder(file);
                    var message = new ChatMessage
                    {
                        Sender = "User",
                        Message = messageType == MessageType.File ? $"file://{filePath}" : filePath,
                        MessageType = messageType,
                        CreatedAt = DateTime.Now
                    };
                    await AddMessageAndGetResponse(message);
                }
            }
        }
        private async void HandlePasteFromClipboard()
        {
            if (Clipboard.ContainsImage())
            {
                var bitmap = Clipboard.GetImage();
                if (bitmap != null)
                {
                    string filePath = SaveClipboardImageToFile(bitmap);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var message = new ChatMessage
                        {
                            Sender = ChatBotViewModel.Instance._user,
                            Message = filePath,
                            MessageType = MessageType.Image,
                            CreatedAt = DateTime.Now
                        };
                        await AddMessageAndGetResponse(message);
                    }
                }
            }
            else if (Clipboard.ContainsText())
            {
                MessageTextBox.Text += Clipboard.GetText();
                MessageTextBox.CaretIndex = MessageTextBox.Text.Length;
            }
            else if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList();
                foreach (string file in files)
                {
                    string extension = System.IO.Path.GetExtension(file).ToLower();
                    MessageType messageType;
                    if (extension == ".png" || extension == ".jpg")
                    {
                        messageType = MessageType.Image;
                    }
                    else if (extension == ".mp4" || extension == ".avi")
                    {
                        messageType = MessageType.Video;
                    }
                    else
                    {
                        messageType = MessageType.File;
                    }
                    var message = new ChatMessage
                    {
                        Sender = "User",
                        Message = messageType == MessageType.File ? $"file://{file}" : file,
                        MessageType = messageType,
                        CreatedAt = DateTime.Now
                    };
                    await AddMessageAndGetResponse(message);
                }
            }
        }

        #region Save
        private string SaveClipboardImageToFile(BitmapSource bitmap)
        {
            try
            {
                string chatFilesPath = System.IO.Path.Combine(dataFolderPath, "ChatFiles");
                if (!Directory.Exists(chatFilesPath))
                    Directory.CreateDirectory(chatFilesPath);
                string filePath = System.IO.Path.Combine(chatFilesPath, $"ChatBotImage_{Guid.NewGuid()}.png");
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(fileStream);
                }
                return filePath;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return null;
            }
        }
        private string SaveFileToChatFolder(string sourcePath)
        {
            string chatFilesPath = System.IO.Path.Combine(dataFolderPath, "ChatFiles");
            Directory.CreateDirectory(chatFilesPath);
            string fileName = $"{Guid.NewGuid()}{System.IO.Path.GetExtension(sourcePath)}";
            string destPath = System.IO.Path.Combine(chatFilesPath, fileName);
            File.Copy(sourcePath, destPath, true);
            return destPath;
        }
        #endregion

        #region Key
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage_Click(sender, e);
            }
        }
        private async void MessageTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                await HandlePaste();
                e.Handled = true;
            }
        }
        #endregion

        #region Button
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.CloseChatTab();
            }
        }
        private void btnChatSetting_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.OpenChatSetting();
            }
        }
        private void NewChat_Click(object sender, RoutedEventArgs e)
        {
            ChatBotViewModel.Instance.CreateNewChat();
            ChatHistory.SelectedItem = ChatBotViewModel.Instance.ChatSessions.FirstOrDefault();
        }
        private async void DeleteChat_Click(object sender, RoutedEventArgs e)
        {
            if (ChatHistory.SelectedItem is ChatSession session)
            {
                var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    $"{CMess.conDelete.ToText()} {session.Name}", new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result == 0)
                {
                    await ChatBotViewModel.Instance.DeleteChat(session.Id);
                    ScrollChatListBoxToBottom();
                }
            }
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                var message = new ChatMessage
                {
                    Sender = ChatBotViewModel.Instance._user,
                    Message = MessageTextBox.Text,
                    MessageType = MessageType.Text,
                    CreatedAt = DateTime.Now
                };
                MessageTextBox.Clear();
                await AddMessageAndGetResponse(message);
            }
        }
        private async void SendImage_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileDiaLogHelper.OpenImage();

            if (!string.IsNullOrEmpty(filePath))
            {
                string filePathChoose = SaveFileToChatFolder(filePath);
                var message = new ChatMessage
                {
                    Sender = ChatBotViewModel.Instance._user,
                    Message = filePathChoose,
                    MessageType = MessageType.Image,
                    CreatedAt = DateTime.Now
                };
                await AddMessageAndGetResponse(message);
            }
        }
        private async void SendVideo_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileDiaLogHelper.OpenVideo();

            if (!string.IsNullOrEmpty(filePath))
            {
                string filePathChoose = SaveFileToChatFolder(filePath);
                var message = new ChatMessage
                {
                    Sender = ChatBotViewModel.Instance._user,
                    Message = filePathChoose,
                    MessageType = MessageType.Video,
                    CreatedAt = DateTime.Now
                };
                await AddMessageAndGetResponse(message);
            }
        }
        private async void SendFile_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileDiaLogHelper.OpenFile();

            if (!string.IsNullOrEmpty(filePath))
            {
                string filePathChoose = SaveFileToChatFolder(filePath);
                var message = new ChatMessage
                {
                    Sender = ChatBotViewModel.Instance._user,
                    Message = $"file://{filePathChoose}",
                    MessageType = MessageType.File,
                    CreatedAt = DateTime.Now
                };
                await AddMessageAndGetResponse(message);
            }
        }
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image image && image.DataContext is ChatMessage message && message.MessageType == MessageType.Image)
            {
                string imagePath = message.Message;
                if (!string.IsNullOrWhiteSpace(imagePath) && System.IO.File.Exists(imagePath))
                {
                    var parentWindow = System.Windows.Window.GetWindow(this);
                    if (parentWindow != null)
                    {
                        var viewer = new ImageViewerWindow(imagePath);
                        viewer.Owner = parentWindow;
                        viewer.ShowDialog();
                    }
                }
                else
                {
                    ///
                }
            }
        }
        private void FileMessage_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.DataContext is ChatMessage message)
            {
                string filePath = message.Message.Replace("file://", "");
                if (File.Exists(filePath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
        }
        private void FileMessage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is ChatMessage message)
            {
                string filePath = message.Message.Replace("file://", "");
                if (File.Exists(filePath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                    }
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, CMess.fileNotExit.ToText(), new[] { CMess.ok.ToText() });
                }
            }
        }

        private void SaveChat_Click(object sender, RoutedEventArgs e)
        {
            ChatBotViewModel.Instance.SaveCurrentChat();
            ChatBotViewModel.Instance.ChatMessages.Clear();
            ChatBotViewModel.Instance.ChatMessages.Add(new ChatMessage
            {
                Sender = "AI",
                Message = "New chat started! Try asking for Card Text or Card Script.",
                MessageType = MessageType.Text,
                CreatedAt = DateTime.Now
            });
        }
        #endregion

    }
}
