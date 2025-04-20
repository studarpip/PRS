using System.Text.RegularExpressions;

namespace PRS.Server.Helpers
{
    public static class PasswordHelper
    {
        public static bool IsStrongPassword(this string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$");
            return regex.IsMatch(password);
        }
    }
}
