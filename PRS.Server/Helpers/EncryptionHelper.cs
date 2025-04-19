using PRS.Server.Helpers.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PRS.Server.Helpers
{
    public class EncryptionHelper : IEncryptionHelper
    {
        private readonly byte[] _key;

        public EncryptionHelper(IConfiguration config)
        {
            var keyString = config["Encryption:Key"];

            if (string.IsNullOrWhiteSpace(keyString))
                throw new Exception("Encryption key is missing from configuration.");

            _key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cs))
                writer.Write(plainText);

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = fullCipher[..(aes.BlockSize / 8)];

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(fullCipher, aes.BlockSize / 8, fullCipher.Length - (aes.BlockSize / 8));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);

            return reader.ReadToEnd();
        }
    }
}
