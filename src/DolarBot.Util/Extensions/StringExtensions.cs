using System;
using System.Text;
using Discord;

namespace DolarBot.Util.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns true if the current string is equal to another, ignoring case sensitivity and spaces.
        /// </summary>
        /// <param name="value1">The current string.</param>
        /// <param name="value2">The string to compare.</param>
        /// <returns>True if equivalent, otherwise false.</returns>
        public static bool IsEquivalentTo(this string value1, string value2)
        {
            return value1.Trim().ToUpper().Equals(value2.Trim().ToUpper());
        }

        /// <summary>
        /// Appends a line break with an invisible character to the current string.
        /// </summary>
        /// <param name="value">The current string.</param>
        /// <returns>A string with the empty line break added.</returns>
        public static string AppendLineBreak(this string value)
        {
            return new StringBuilder(value).Append(Environment.NewLine).Append(GlobalConfiguration.Constants.BLANK_SPACE).ToString();
        }

        /// <summary>
        /// Appends a line break with an invisible character to the current <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="stringBuilder">The current <see cref="StringBuilder"/>.</param>
        /// <returns>A string with the empty line break added.</returns>
        public static StringBuilder AppendLineBreak(this StringBuilder stringBuilder, bool isEmbed = true)
        {
            if (isEmbed)
            {
                return stringBuilder.AppendLine(GlobalConfiguration.Constants.BLANK_SPACE);
            }
            else
            {
                return stringBuilder.AppendLine(Environment.NewLine);
            }
        }

        /// <summary>
        /// Capitalizes the first letter of the current string.
        /// </summary>
        /// <param name="text">The current string.</param>
        /// <returns>The same string with the first letter capitalized.</returns>
        public static string Capitalize(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            else if (text.Length == 1)
            {
                return text.ToUpper();
            }
            else
            {
                return $"{char.ToUpper(text[0])}{text.Substring(1)}";
            }
        }

        /// <summary>
        /// Removes Bold, Italic, Strikethrough, Code and Spoiler formatting.
        /// </summary>
        /// <param name="text">The current string.</param>
        /// <param name="sanitized">Indicates whether the <paramref name="text"/> has been sanitized using <see cref="Format.Sanitize"/>.</param>
        /// <returns>The input text without any format.</returns>
        public static string RemoveFormat(this string text, bool sanitized = false)
        {
            string[] formatWrappers = new[] { "*", "**", "~~", "`", "||" };
            foreach (string formatWrapper in formatWrappers)
            {
                string textToReplace = sanitized ? $"\\{formatWrapper}" : formatWrapper;
                text = text.Replace(textToReplace, string.Empty);
            }
            return text;
        }

        /// <summary>
        /// Tests if the current <see cref="text"/> is a number.
        /// </summary>
        /// <param name="text">The current string.</param>
        /// <returns>True if the text is a number, otherwise false.</returns>
        public static bool IsNumeric(this string text)
        {
            return long.TryParse(text, out long _) || double.TryParse(text, out double _);
        }
    }
}
