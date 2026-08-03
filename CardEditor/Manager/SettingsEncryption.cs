using System;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

namespace CardEditor.Manager
{
    public static class SettingsEncryption
    {
        private static byte[] GetEntropy()
        {
            string entropySource = $"CardEditorX-{Environment.MachineName}-{Environment.UserName}";
            return Encoding.UTF8.GetBytes(entropySource);
        }

        public static string EncryptBoolSetting(bool value)
        {
            try
            {
                // Chuyển đổi giá trị bool thành byte[]
                byte[] valueBytes = BitConverter.GetBytes(value);
                byte[] entropy = GetEntropy();

                // Mã hóa bằng DPAPI
                byte[] encryptedData = ProtectedData.Protect(
                    valueBytes,
                    entropy, // Entropy có thể thêm vào để tăng cường bảo mật
                    DataProtectionScope.CurrentUser);

                // Chuyển đổi thành chuỗi Base64 để lưu trữ
                return Convert.ToBase64String(encryptedData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi mã hóa: {ex.Message}");
                return null;
            }
        }

        public static bool DecryptBoolSetting(string encryptedValue)
        {
            try
            {
                // Kiểm tra giá trị null hoặc rỗng
                if (string.IsNullOrEmpty(encryptedValue))
                    return false; // Giá trị mặc định

                // Chuyển đổi từ Base64 thành byte[]
                byte[] encryptedData = Convert.FromBase64String(encryptedValue);
                byte[] entropy = GetEntropy();

                // Giải mã bằng DPAPI
                byte[] decryptedData = ProtectedData.Unprotect(
                    encryptedData,
                    entropy, // Entropy (phải giống với khi mã hóa)
                    DataProtectionScope.CurrentUser);

                // Chuyển đổi trở lại thành bool
                return BitConverter.ToBoolean(decryptedData, 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi giải mã: {ex.Message}");
                return false; // Giá trị mặc định khi có lỗi
            }
        }
    }
}
