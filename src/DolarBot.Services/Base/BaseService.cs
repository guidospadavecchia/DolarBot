﻿using Discord;
using DolarBot.API;
using DolarBot.Services.Banking;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;

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
        /// Appends the play store link as a field into <paramref name="embed"/>.
        /// </summary>
        /// <param name="embed">The embed to be modified.</param>
        /// <returns>The modified <see cref="EmbedBuilder"/>.</returns>
        public EmbedBuilder AddPlayStoreLink(EmbedBuilder embed)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji playStoreEmoji = new Emoji(emojis["playStore"]);
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
