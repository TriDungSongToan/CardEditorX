using System.IO;
using System.Security.Cryptography;

namespace CardEditor.Manager
{
    public static class SecretManager
    {
        private static readonly byte[] EncryptedPass = new byte[]
        {
            239, 30, 170, 147, 218, 139, 28, 32, 207, 254, 199, 90, 30, 143, 237, 172
        };
        public static string GetSecret(string keyFilePath, string ivFilePath)
        {
            if (!File.Exists(keyFilePath) || !File.Exists(ivFilePath))
                return null;
            try
            {
                byte[] key = File.ReadAllBytes(keyFilePath);
                byte[] iv = File.ReadAllBytes(ivFilePath);

                using Aes aesAlg = Aes.Create();
                aesAlg.Key = key;
                aesAlg.IV = iv;

                using var msDecrypt = new MemoryStream(EncryptedPass);
                using var csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                return srDecrypt.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }
    }
}
