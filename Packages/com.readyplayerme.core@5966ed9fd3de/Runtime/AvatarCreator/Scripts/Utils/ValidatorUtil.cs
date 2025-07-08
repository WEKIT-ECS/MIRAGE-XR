using System.Text.RegularExpressions;

namespace ReadyPlayerMe
{
    public static class ValidatorUtil
    {
        private const string EMAIL_REGEX = @".+\@.+\..+";

        public static bool IsValidEmail(string email)
        {
            var regex = new Regex(EMAIL_REGEX);
            var match = regex.Match(email);
            return match.Success;
        }
    }
}
