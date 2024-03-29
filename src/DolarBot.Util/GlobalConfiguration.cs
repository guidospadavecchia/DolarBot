﻿using Discord;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;

namespace DolarBot.Util
{
    /// <summary>
    /// Contains global application values and constants.
    /// </summary>
    public static class GlobalConfiguration
    {
        #region Vars
        private static DateTime _bootTime;
        #endregion

        #region Global

        /// <summary>
        /// Gets the status text for the bot.
        /// </summary>
        /// <returns></returns>
        public static string GetStatusText() => $"/ayuda";
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
        /// Gets the enviromental variable name for the token.
        /// </summary>
        /// <returns></returns>
        public static string GetDblTokenEnvVarName() => "DOLARBOT_DBL_TOKEN";
        /// <summary>
        /// Gets the enviromental variable name for the invite URL.
        /// </summary>
        /// <returns></returns>
        public static string GetInviteLinkEnvVarName() => "DOLARBOT_INVITE";
        /// <summary>
        /// Gets the bot's timezone.
        /// </summary>
        /// <returns></returns>
        public static TimeZoneInfo GetLocalTimeZoneInfo()
        {
            const string WINDOWS_TIMEZONE = "Argentina Standard Time";
            const string IANA_TIMEZONE = "America/Argentina/Buenos_Aires";
            string timeZoneId = TimeZoneInfo.GetSystemTimeZones().Any(x => x.Id.Equals(WINDOWS_TIMEZONE, StringComparison.OrdinalIgnoreCase)) ? WINDOWS_TIMEZONE : IANA_TIMEZONE;
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        /// <summary>
        /// Gets the localized culture format.
        /// </summary>
        /// <returns></returns>
        public static CultureInfo GetLocalCultureInfo() => CultureInfo.GetCultureInfo("es-AR");
        /// <summary>
        /// Gets the date and time for application's boot.
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetUptime() => DateTime.Now - _bootTime;
        /// <summary>
        /// Sets the date and time for application's boot.
        /// </summary>
        public static void Initialize()
        {
            _bootTime = DateTime.Now;
        }

        /// <summary>
        /// Returns the standarized message for unhandled errors.
        /// </summary>
        /// <param name="supportServerUrl">Support server URL.</param>
        /// <returns></returns>
        public static string GetGenericErrorMessage(string supportServerUrl)
        {
            if (string.IsNullOrEmpty(supportServerUrl))
            {
                return $"{Format.Bold("Oops!")} Ocurrió un error inesperado. Si el problema persiste, contactate con {Format.Bold("Svenjörn#9806")}.";
            }
            else
            {
                return $"{Format.Bold("Oops!")} Ocurrió un error inesperado. Si el problema persiste, reportalo en el servidor de soporte (<{supportServerUrl}>) o contactate con {Format.Bold("svenjorn")}.";
            }
        }

        /// <summary>
        /// Retrieves the bot's token from the application settings or operating system's enviromental variable.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> object to access application settings.</param>
        /// <returns>The retrieved token.</returns>
        public static string GetToken(IConfiguration configuration)
        {
            string token = configuration["token"];
            if (string.IsNullOrWhiteSpace(token))
            {
                token = Environment.GetEnvironmentVariable(GetTokenEnvVarName());
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new SystemException("Missing token");
                }
            }

            return token;
        }

        /// <summary>
        /// Retrieves the bot's DBL token (top.gg) from the application settings or operating system's enviromental variable.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> object to access application settings.</param>
        /// <returns>The retrieved token.</returns>
        public static string GetDblToken(IConfiguration configuration)
        {
            string dblToken = configuration["dblToken"];
            if (string.IsNullOrWhiteSpace(dblToken))
            {
                dblToken = Environment.GetEnvironmentVariable(GetDblTokenEnvVarName());
            }
            return dblToken;
        }

        #endregion

        #region Public constants
        /// <summary>
        /// Contains all the global application constants.
        /// </summary>
        public static class Constants
        {
            public const string BLANK_SPACE = "\u200B";
        }

        /// <summary>
        /// Contains the color palette for each module.
        /// </summary>
        public static class Colors
        {
            /// <summary>
            /// The main color for commands.
            /// </summary>
            public static readonly Color Main = new(40, 150, 75);
            /// <summary>
            /// The color for help-related commands.
            /// </summary>
            public static readonly Color Help = Color.Blue;
            /// <summary>
            /// The color for information-related commands.
            /// </summary>
            public static readonly Color Info = new(23, 99, 154);
            /// <summary>
            /// The color for misc commands.
            /// </summary>
            public static readonly Color Currency = Color.DarkGrey;
            /// <summary>
            /// The color for Euro.
            /// </summary>
            public static readonly Color Euro = new(43, 71, 157);
            /// <summary>
            /// The color for Real.
            /// </summary>
            public static readonly Color Real = new(255, 218, 68);
            /// <summary>
            /// The color for Gold.
            /// </summary>
            public static readonly Color Gold = new(255, 193, 7);
            /// <summary>
            /// The color for Silver.
            /// </summary>
            public static readonly Color Silver = new(197, 197, 197);
            /// <summary>
            /// The color for Copper.
            /// </summary>
            public static readonly Color Copper = new(172, 124, 2);
            /// <summary>
            /// The color for Crypto.
            /// </summary>
            public static readonly Color Crypto = new(131, 131, 131);
            /// <summary>
            /// The color for Venezuela rates.
            /// </summary>
            public static readonly Color Venezuela = new(127, 23, 52);
        }
        #endregion
    }
}
