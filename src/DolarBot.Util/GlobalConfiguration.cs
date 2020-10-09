using System;

namespace DolarBot.Util
{
    /// <summary>
    /// Contains global application values and constants.
    /// </summary>
    public static class GlobalConfiguration
    {
        #region Global

        /// <summary>
        /// Gets the status text for the bot.
        /// </summary>
        /// <returns></returns>
        public static string GetStatusText() => "$ayuda";
        /// <summary>
        /// Gets the application settings file name.
        /// </summary>
        /// <returns></returns>
        public static string GetAppSettingsFileName() => "appsettings.json";
        /// <summary>
        /// Gets the log4net configuration file name.
        /// </summary>
        /// <returns></returns>
        public static string GetLogConfigFileName() => "log4net.config";
        /// <summary>
        /// Gets the enviromental variable name for the token.
        /// </summary>
        /// <returns></returns>
        public static string GetTokenEnvVarName() => "DOLARBOT_TOKEN";
        /// <summary>
        /// Gets the enviromental variable name for the invite URL.
        /// </summary>
        /// <returns></returns>
        public static string GetInviteLinkEnvVarName() => "DOLARBOT_INVITE";
        /// <summary>
        /// Gets the bot's timezone.
        /// </summary>
        /// <returns></returns>
        public static TimeZoneInfo GetLocalTimeZoneInfo() => TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

        #endregion

        #region Public constants
        /// <summary>
        /// Contains all the global application constants.
        /// </summary>
        public static class Constants
        {
            public const string BLANK_SPACE = "\u200B";
        }
        #endregion
    }
}
