using System;

namespace DolarBot.Util
{
    public static class GlobalConfiguration
    {
        #region Global

        public static string GetStatusText() => "$ayuda";
        public static string GetAppSettingsFileName() => "appsettings.json";
        public static string GetLogConfigFileName() => "log4net.config";
        public static string GetTokenEnvVarName() => "DOLARBOT_TOKEN";
        public static string GetInviteLinkEnvVarName() => "DOLARBOT_INVITE";
        public static TimeZoneInfo GetLocalTimeZoneInfo() => TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

        #endregion

        #region Public constants
        public static class Constants
        {
            public const string BLANK_SPACE = "\u200B";
        }
        #endregion
    }
}
