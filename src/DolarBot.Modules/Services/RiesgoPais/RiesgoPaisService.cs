using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;

namespace DolarBot.Modules.Services.Dolar
{
    /// <summary>
    /// Contains several methods to process country risk commands.
    /// </summary>
    public class RiesgoPaisService
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

        /// <summary>
        /// Color for the embed messages.
        /// </summary>
        private readonly Color EmbedColor;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="DolarService"/> object with the provided configuration, api object and embed color.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="embedColor">The color to use for embed messages.</param>
        public RiesgoPaisService(IConfiguration configuration, ApiCalls api, Color embedColor)
        {
            Configuration = configuration;
            EmbedColor = embedColor;
            Api = api;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="RiesgoPaisResponse"/>.
        /// </summary>
        /// <param name="riesgoPaisResponse">The Riesgo Pais response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateRiesgoPaisEmbed(RiesgoPaisResponse riesgoPaisResponse)
        {
            Emoji chartEmoji = new Emoji("\uD83D\uDCC8");
            string chartImageUrl = Configuration.GetSection("images").GetSection("chart")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string value = decimal.TryParse(riesgoPaisResponse?.Valor, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out decimal valor) ? ((int)Math.Round(valor * 1000, MidpointRounding.AwayFromZero)).ToString() : "No informado";

            EmbedBuilder embed = new EmbedBuilder().WithColor(EmbedColor)
                                                   .WithTitle("Riesgo País")
                                                   .WithDescription($"Valor del {Format.Bold("riesgo país")} argentino.".AppendLineBreak())
                                                   .WithThumbnailUrl(chartImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(riesgoPaisResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy - HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", $"{Format.Bold($"{chartEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {value}")} puntos".AppendLineBreak());
            return embed;
        }

        #endregion
    }
}
