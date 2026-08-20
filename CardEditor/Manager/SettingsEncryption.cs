using System;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

namespace CardEditor.Manager
{
    public static class SettingsEncryption
    {
        private static byte[] GetEntropy()
            => Encoding.UTF8.GetBytes($"CardEditorX-{Environment.MachineName}-{Environment.UserName}");

        public static string EncryptString(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            byte[] data = Encoding.UTF8.GetBytes(value);
            byte[] encrypted = ProtectedData.Protect(data, GetEntropy(), DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }
        public static string DecryptString(string encryptedValue)
        {
            if (string.IsNullOrEmpty(encryptedValue)) return null;
            try
            {
                byte[] encrypted = Convert.FromBase64String(encryptedValue);
                byte[] data = ProtectedData.Unprotect(encrypted, GetEntropy(), DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return null;
            }
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

    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }

            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }
        public static bool Verify(string password, string stored)
        {
            var parts = stored.Split('.');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            byte[] actualHash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                actualHash = pbkdf2.GetBytes(expectedHash.Length);
            }

            return FixedTimeEquals(actualHash, expectedHash);
        }
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
