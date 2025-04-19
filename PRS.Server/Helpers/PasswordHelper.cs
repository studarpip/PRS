using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PRS.Server.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(this string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        public static bool Match(this string password, string hashed)
        {
            var hashOfInput = Hash(password);
            return hashOfInput == hashed;
        }

        public static bool IsStrong(this string password)
        {
            if (string.IsNullOrWhiteSpace(password)) 
                return false;

            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$");
            return regex.IsMatch(password);
        }
    }
}
