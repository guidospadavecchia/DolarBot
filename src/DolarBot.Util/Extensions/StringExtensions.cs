using System;
using System.Text;

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
    }
}
