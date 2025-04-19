using System.Security.Cryptography;
using System.Text;

namespace PRS.Server.Helpers
{
    public static class HashHelper
    {
        public static string HashString(this string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        public static bool IsStringHashedMatch(this string input, string hashed)
        {
            var hashOfInput = HashString(input);
            return hashOfInput == hashed;
        }
    }
}
