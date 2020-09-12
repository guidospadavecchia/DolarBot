using System.IO;

namespace DolarBot.Util
{
    public static class GlobalConfiguration
    {
        #region Private constants
        private const string CONFIG_FILENAME = "appsettings.json";
        private const string LOG_CONFIG_FILENAME = "log4net.config";
        private const string GAME_STATUS = "$ayuda | $help";
        #endregion

        #region Methods

        public static string GetStatusText() => GAME_STATUS;
        public static string GetAppSettingsFileName() => CONFIG_FILENAME;
        public static string GetLogConfigFileName() => LOG_CONFIG_FILENAME;

        #endregion

        #region Images
        public static class Images
        {
            private const string IMAGE_FOLDER = ".\\Images";

            public static string GetHelpImageThumbnailUrl() => $"attachment://info32.png";
            public static string GetHelpImageThumbnailFullPath() => $"{Path.Combine(IMAGE_FOLDER, "info32.png")}";
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
