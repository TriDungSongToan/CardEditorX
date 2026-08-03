using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CardEditor.Manager
{
    public sealed class SingleInstanceManager : IDisposable
    {
        // Đặt tên duy nhất, nên gắn thêm GUID cố định để tránh trùng với app khác
        private const string MutexName = "CardEditor_SingleInstance_Mutex_9F1A2B3C";
        private const string PipeName = "CardEditor_SingleInstance_Pipe_9F1A2B3C";

        private Mutex _mutex;
        private bool _isFirstInstance;
        private CancellationTokenSource _serverCts;

        /// <summary>
        /// Được gọi khi instance đầu tiên nhận được đường dẫn file từ instance khác.
        /// Chạy trên background thread, cần tự Dispatcher.Invoke nếu đụng UI.
        /// </summary>
        public event Action<string> FileReceived;

        /// <summary>
        /// Thử giành quyền làm instance đầu tiên.
        /// Trả về true nếu là instance đầu tiên (nên tiếp tục khởi động app bình thường).
        /// Trả về false nếu đã có instance khác chạy (đã forward args, nên Shutdown ngay).
        /// </summary>
        public bool TryStart(string[] args)
        {
            _mutex = new Mutex(true, MutexName, out _isFirstInstance);

            if (_isFirstInstance)
            {
                StartPipeServer();
                return true;
            }
            else
            {
                // Đã có instance khác đang chạy -> gửi file path sang đó rồi để caller tự Shutdown
                if (args.Length > 0)
                {
                    SendToRunningInstance(args[0]);
                }
                else
                {
                    // Không có file, chỉ đơn giản là muốn "activate" cửa sổ đang chạy
                    SendToRunningInstance(string.Empty);
                }
                return false;
            }
        }

        private void StartPipeServer()
        {
            _serverCts = new CancellationTokenSource();
            var token = _serverCts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        using var server = new NamedPipeServerStream(
                            PipeName, PipeDirection.In, 1,
                            PipeTransmissionMode.Message, PipeOptions.Asynchronous);

                        await server.WaitForConnectionAsync(token);

                        using var reader = new StreamReader(server, Encoding.UTF8, false, 1024, leaveOpen: true);
                        string message = await reader.ReadLineAsync() ?? string.Empty;

                        FileReceived?.Invoke(message);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (IOException)
                    {
                        // Pipe bị đóng đột ngột, thử lại vòng lặp
                    }
                    catch
                    {
                        // Nuốt lỗi để pipe server không chết hẳn vì 1 lần lỗi
                    }
                }
            }, token);
        }

        private static void SendToRunningInstance(string filePath)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(2000); // timeout 2s

                using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
                writer.WriteLine(filePath);
            }
            catch
            {
                // Nếu không kết nối được (instance kia vừa thoát chẳng hạn),
                // đơn giản là bỏ qua - có thể log lại nếu cần.
            }
        }

        public void Dispose()
        {
            _serverCts?.Cancel();
            _serverCts?.Dispose();

            if (_isFirstInstance)
            {
                _mutex?.ReleaseMutex();
            }
            _mutex?.Dispose();
        }
    }
}
