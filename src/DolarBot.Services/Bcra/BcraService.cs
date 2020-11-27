using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Text;

namespace DolarBot.Services.Bcra
{
    /// <summary>
    /// Contains several methods to process Argentine Central Bank related commands.
    /// </summary>
    public class BcraService
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
        /// Creates a new <see cref="BcraService"/> object with the provided configuration, api object and embed color.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public BcraService(IConfiguration configuration, ApiCalls api)
        {
            Configuration = configuration;
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
            string riskImageUrl = Configuration.GetSection("images").GetSection("risk")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            bool isNumber = double.TryParse(riesgoPaisResponse?.Valor, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out double valor);
            string value;
            if (isNumber)
            {
                int convertedValue = (int)Math.Round(valor, MidpointRounding.AwayFromZero);
                value = convertedValue.ToString("n0", GlobalConfiguration.GetLocalCultureInfo());
            }
            else
            {
                value = "No informado";
            }

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Riesgo País")
                                                   .WithDescription($"Valor del {Format.Bold("riesgo país")} argentino.".AppendLineBreak())
                                                   .WithThumbnailUrl(riskImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(riesgoPaisResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy - HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", $"{Format.Bold($"{chartEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {value}")} puntos".AppendLineBreak());
            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="BcraResponse"/>.
        /// </summary>
        /// <param name="bcraResponse">The BCRA response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateReservasEmbed(BcraResponse bcraResponse)
        {
            Emoji moneyBagEmoji = new Emoji(":moneybag:");
            string reservesImageUrl = Configuration.GetSection("images").GetSection("reserves")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];

            bool isNumber = double.TryParse(bcraResponse?.Valor, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out double valor);
            string text;
            if (isNumber)
            {
                long convertedValue = (long)Math.Round(valor, MidpointRounding.AwayFromZero);
                string value = convertedValue.ToString("n0", GlobalConfiguration.GetLocalCultureInfo());
                text = $"{Format.Bold($"{moneyBagEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} US$ {value}")}";
            }
            else
            {
                text = $"{Format.Bold($"{moneyBagEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} No informado")}";
            }

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Reservas BCRA")
                                                   .WithDescription($"Reservas totales del {Format.Bold("BCRA (Banco Central de la República Argentina)")} expresado en {Format.Bold("dólares estadounidenses")}.".AppendLineBreak())
                                                   .WithThumbnailUrl(reservesImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(bcraResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy - HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", text.AppendLineBreak());
            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="BcraResponse"/>.
        /// </summary>
        /// <param name="bcraResponse">The BCRA response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateCirculanteEmbed(BcraResponse bcraResponse)
        {
            Emoji circulatingMoneyEmoji = new Emoji(":money_with_wings:");
            string reservesImageUrl = Configuration.GetSection("images").GetSection("money")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];

            bool isNumber = double.TryParse(bcraResponse?.Valor, NumberStyles.Any, Api.DolarArgentina.GetApiCulture(), out double valor);
            string text;
            if (isNumber)
            {
                long convertedValue = (long)Math.Round(valor, MidpointRounding.AwayFromZero);
                string value = convertedValue.ToString("n0", GlobalConfiguration.GetLocalCultureInfo());
                text = $"{Format.Bold($"{circulatingMoneyEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {value}")}";
            }
            else
            {
                text = $"{Format.Bold($"{circulatingMoneyEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} No informado")}";
            }

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Main)
                                                   .WithTitle("Pesos en circulación")
                                                   .WithDescription($"Cantidad total de {Format.Bold("pesos argentinos")} en circulación.".AppendLineBreak())
                                                   .WithThumbnailUrl(reservesImageUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(bcraResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy - HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddInlineField($"Valor", text.AppendLineBreak());
            return embed;
        }

        #endregion
    }
}
