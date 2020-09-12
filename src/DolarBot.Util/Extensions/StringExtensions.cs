namespace DolarBot.Util.Extensions
{
    public static class StringExtensions
    {
        public static bool IsEquivalentTo(this string value1, string value2)
        {
            return value1.Trim().ToUpper().Equals(value2.Trim().ToUpper());
        }
    }
}
