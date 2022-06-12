namespace DolarBot.Util.Extensions
{
    public static class NumberExtensions
    {
        /// <summary>
        /// Formats a number to string using "M"/"B" format.
        /// </summary>
        /// <param name="number">The number to format.</param>
        /// <returns>A formatted number.</returns>
        public static string Format(this decimal number)
        {
            if (number > 999999999 || number < -999999999)
            {
                return number.ToString("0,,,.### B", GlobalConfiguration.GetLocalCultureInfo());
            }
            else if (number > 999999 || number < -999999)
            {
                return number.ToString("0,,.## M", GlobalConfiguration.GetLocalCultureInfo());
            }
            else
            {
                return number.ToString(GlobalConfiguration.GetLocalCultureInfo());
            }
        }
    }
}
