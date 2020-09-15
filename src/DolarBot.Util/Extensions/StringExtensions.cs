using System;
using System.Text;

namespace DolarBot.Util.Extensions
{
    public static class StringExtensions
    {
        public static bool IsEquivalentTo(this string value1, string value2)
        {
            return value1.Trim().ToUpper().Equals(value2.Trim().ToUpper());
        }

        public static string AppendLineBreak(this string value)
        {
            return new StringBuilder(value).Append(Environment.NewLine).Append(GlobalConfiguration.Constants.BLANK_SPACE).ToString();
        }
    }
}
