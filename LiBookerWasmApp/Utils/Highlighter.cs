using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace LiBookerWasmApp.Utils
{
    public static class Highlighter
    {
        /// <summary>
        /// Highlights all occurrences of searchTerm in text by wrapping them in <strong> tags.
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static MarkupString Highlight(string searchTerm, string text)
        {
            if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(text))
                return new MarkupString(text);
            var pattern = Regex.Escape(searchTerm);
            var result = Regex.Replace(text,
                            $"({pattern})", "<strong>$1</strong>",
                            RegexOptions.IgnoreCase);
            return new MarkupString(result);
        }
    }
}
