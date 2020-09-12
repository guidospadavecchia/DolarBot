using System.IO;

namespace DolarBot.Util
{
    public static class GlobalConfiguration
    {
        #region Global

        public static string GetStatusText() => "$ayuda | $help";
        public static string GetAppSettingsFileName() => "appsettings.json";
        public static string GetLogConfigFileName() => "log4net.config";

        #endregion

        #region Images
        public static class Images
        {
            private const string IMAGE_FOLDER = ".\\Images";

            public static string GetLocalHelpImageThumbnailUrl() => $"attachment://info32.png";
            public static string GetLocalHelpImageThumbnailFullPath() => $"{Path.Combine(IMAGE_FOLDER, "info32.png")}";
            public static string GetHelpImageThumbnailUrl() => $"https://i.imgur.com/p8MZ5zz.png";
        }
        #endregion

        #region Public constants
        public static class Constants
        {
            public const string BLANK_SPACE = "\u200B";
        }
        #endregion
    }
}
