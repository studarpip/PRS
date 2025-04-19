using System.Text.RegularExpressions;

namespace PRS.Server.Helpers
{
    public static class EmailHelper
    {
        private static readonly Regex _emailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return _emailRegex.IsMatch(email);
        }
    }
}
