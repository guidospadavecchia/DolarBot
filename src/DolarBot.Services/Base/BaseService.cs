using Discord;
using DolarBot.API;
using DolarBot.Services.Banking;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;

namespace DolarBot.Services.Base
{
    public abstract class BaseService
    {
        #region Vars

        /// <summary>
        /// Provides access to application settings.
        /// </summary>
        protected readonly IConfiguration Configuration;

        /// <summary>
        /// Provides access to the different APIs.
        /// </summary>
        protected readonly ApiCalls Api;

        #endregion

        #region Constructors

        /// <summary>
        /// Base constructor for all services.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public BaseService(IConfiguration configuration, ApiCalls api)
        {
            Configuration = configuration;
            Api = api;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns an array of valid date formats for command parameters.
        /// </summary>
        /// <returns></returns>
        public string[] GetValidDateFormats()
        {
            return new[]
            {
                "yyyy/M/d",
                "yyyy-M-d",
                "yyyy/M",
                "yyyy-M",
                "d/M/yyyy",
                "d-M-yyyy",
                "M/yyyy",
                "M-yyyy",
                "yyyy"
            };
        }

        /// <summary>
        /// Parses a date from a string input assuming standard date formats.
        /// </summary>
        /// <param name="input">The date as a string.</param>
        /// <param name="date">The parsed date.</param>
        /// <returns>A boolean value indicating whether the parsing was successful.</returns>
        public bool ParseDate(string input, out DateTime? date)
        {
            DateTime result = default;
            bool validDate = false;
            foreach (string format in GetValidDateFormats())
            {
                validDate = DateTime.TryParseExact(input, format, GlobalConfiguration.GetLocalCultureInfo(), DateTimeStyles.None, out result);
                if (validDate)
                {
                    break;
                }
            }

            if(!validDate && input.Equals("hoy", StringComparison.OrdinalIgnoreCase))
            {
                result = DateTime.Now.Date;
                validDate = true;
            }

            date = result;
            return validDate;
        }

        /// <summary>
        /// Appends the play store link as a field into <paramref name="embed"/>.
        /// </summary>
        /// <param name="embed">The embed to be modified.</param>
        /// <returns>The modified <see cref="EmbedBuilder"/>.</returns>
        public EmbedBuilder AddPlayStoreLink(EmbedBuilder embed)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji playStoreEmoji = new(emojis["playStore"]);
            string playStoreUrl = Configuration["playStoreLink"];

            if (!string.IsNullOrWhiteSpace(playStoreUrl))
            {
                return embed.AddFieldLink(playStoreEmoji, "¡Descargá la app para Android!", "Google Play Store", playStoreUrl);
            }
            else
            {
                return embed;
            }
        }

        /// <summary>
        /// Retrieves the thumbnail URL for a particular <see cref="Banks"/> object.
        /// </summary>
        /// <param name="bank">The value to retrieve.</param>
        /// <returns>The banks thumbnail URL.</returns>
        public string GetBankThumbnailUrl(Banks bank)
        {
            var banksSection = Configuration.GetSection("images").GetSection("banks");
            return bank switch
            {
                Banks.Nacion => banksSection["nacion"],
                Banks.BBVA => banksSection["bbva"],
                Banks.Piano => banksSection["piano"],
                Banks.Hipotecario => banksSection["hipotecario"],
                Banks.Galicia => banksSection["galicia"],
                Banks.Santander => banksSection["santander"],
                Banks.Ciudad => banksSection["ciudad"],
                Banks.Supervielle => banksSection["supervielle"],
                Banks.Patagonia => banksSection["patagonia"],
                Banks.Comafi => banksSection["comafi"],
                Banks.Bancor => banksSection["bancor"],
                Banks.Chaco => banksSection["chaco"],
                Banks.Pampa => banksSection["pampa"],
                Banks.ICBC => banksSection["icbc"],
                Banks.Provincia => banksSection["provincia"],
                Banks.Reba => banksSection["reba"],
                Banks.Roela => banksSection["roela"],
                _ => string.Empty,
            };
        }

        #endregion
    }
}
