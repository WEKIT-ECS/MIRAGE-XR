using System;
using System.Text.RegularExpressions;

namespace ReadyPlayerMe.Core.Editor
{
    public static class StringExtensions
    {
        private const string SHORT_CODE_REGEX = "^[A-Z0-9]{6}$";
        
        public static bool IsUrlShortcodeValid(this string urlString)
        {
            return !string.IsNullOrEmpty(urlString) &&
                   (Regex.Match(urlString, SHORT_CODE_REGEX).Length > 0 || Uri.IsWellFormedUriString(urlString, UriKind.Absolute) && urlString.EndsWith(".glb"));
        }
    }
}
