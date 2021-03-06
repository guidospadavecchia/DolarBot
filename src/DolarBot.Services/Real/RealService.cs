﻿using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Banking;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealTypes = DolarBot.API.ApiCalls.DolarBotApi.RealTypes;

namespace DolarBot.Services.Real
{
    /// <summary>
    /// Contains several methods to process Real (Brazil) commands.
    /// </summary>
    public class RealService : BaseCurrencyService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="RealService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public RealService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override Banks[] GetValidBanks()
        {
            return new[]
            {
                Banks.Nacion,
                Banks.BBVA,
                Banks.Chaco,
                Banks.Piano,
                Banks.Ciudad,
                Banks.Supervielle
            };
        }

        /// <summary>
        /// Converts a <see cref="Banks"/> object to its <see cref="RealTypes"/> equivalent.
        /// </summary>
        /// <param name="bank">The value to convert.</param>
        /// <returns>The converted value as <see cref="RealTypes"/>.</returns>
        private RealTypes ConvertToRealType(Banks bank)
        {
            return bank switch
            {
                Banks.Nacion => RealTypes.Nacion,
                Banks.BBVA => RealTypes.BBVA,
                Banks.Chaco => RealTypes.Chaco,
                Banks.Piano => RealTypes.Piano,
                Banks.Ciudad => RealTypes.Ciudad,
                Banks.Supervielle => RealTypes.Supervielle,
                _ => throw new ArgumentException("Unsupported Real type")
            };
        }

        #region API Calls

        /// <summary>
        /// Fetches a single rate by bank. Only accepts banks returned by <see cref="GetValidBanks"/>.
        /// </summary>
        /// <param name="bank">The bank who's rate is to be retrieved.</param>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealByBank(Banks bank)
        {
            RealTypes realType = ConvertToRealType(bank);
            return await Api.DolarBot.GetRealRate(realType).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all available Real prices.
        /// </summary>
        /// <returns>An array of <see cref="RealResponse"/> objects.</returns>
        public async Task<RealResponse[]> GetAllRealRates()
        {
            List<Task<RealResponse>> tasks = new List<Task<RealResponse>>();
            foreach (RealTypes realType in Enum.GetValues(typeof(RealTypes)).Cast<RealTypes>())
            {
                tasks.Add(Api.DolarBot.GetRealRate(realType));
            }
            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Real from bank Nacion.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealNacion()
        {
            return await Api.DolarBot.GetRealRate(RealTypes.Nacion).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Real from bank BBVA.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealBBVA()
        {
            return await Api.DolarBot.GetRealRate(RealTypes.BBVA).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the price for Real from Nuevo Banco del Chaco.
        /// </summary>
        /// <returns>A single <see cref="RealResponse"/>.</returns>
        public async Task<RealResponse> GetRealChaco()
        {
            return await Api.DolarBot.GetRealRate(RealTypes.Chaco).ConfigureAwait(false);
        }

        #endregion

        #region Embed

        /// <summary>
        /// Returns the title depending on the response type.
        /// </summary>
        /// <param name="realResponse">The real response.</param>
        /// <returns>The corresponding title.</returns>
        private string GetTitle(RealResponse realResponse)
        {
            string realType = realResponse.Type.ToString();
            if (Enum.TryParse(realType, out Banks bankType))
            {
                return bankType.GetDescription();
            }
            else
            {
                throw new ArgumentException("Unsupported Bank");
            }
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for multiple Real responses specifying a custom description and thumbnail URL.
        /// </summary>
        /// <param name="realResponses">The Real responses to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="thumbnailUrl">The URL of the embed's thumbnail image.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateRealEmbed(RealResponse[] realResponses, string description, string thumbnailUrl)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji realEmoji = new Emoji(emojis["real"]);
            Emoji clockEmoji = new Emoji("\u23F0");
            Emoji buyEmoji = new Emoji(emojis["buyYellow"]);
            Emoji sellEmoji = new Emoji(emojis["sellYellow"]);
            Emoji sellWithTaxesEmoji = new Emoji(emojis["sellWithTaxesYellow"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Real)
                                                   .WithTitle("Cotizaciones del Real")
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter($" C = Compra | V = Venta | A = Venta con impuestos | {clockEmoji} = Last updated (UTC {utcOffset})");

            for (int i = 0; i < realResponses.Length; i++)
            {
                RealResponse response = realResponses[i];
                string blankSpace = GlobalConfiguration.Constants.BLANK_SPACE;
                string title = GetTitle(response);
                string lastUpdated = response.Fecha.ToString("dd/MM - HH:mm");
                string buyPrice = decimal.TryParse(response?.Compra, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellPrice = decimal.TryParse(response?.Venta, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellWithTaxesPrice = response?.VentaAhorro != null ? (decimal.TryParse(response?.VentaAhorro, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal ventaAhorro) ? ventaAhorro.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?") : null;

                if (buyPrice != "?" || sellPrice != "?" || (sellWithTaxesPrice != null && sellWithTaxesPrice != "?"))
                {
                    StringBuilder sbField = new StringBuilder()
                                            .AppendLine($"{realEmoji} {blankSpace} {buyEmoji} {Format.Bold($"$ {buyPrice}")}")
                                            .AppendLine($"{realEmoji} {blankSpace} {sellEmoji} {Format.Bold($"$ {sellPrice}")}");
                    if (sellWithTaxesPrice != null)
                    {
                        sbField.AppendLine($"{realEmoji} {blankSpace} {sellWithTaxesEmoji} {Format.Bold($"$ {sellWithTaxesPrice}")}");
                    }
                    sbField.AppendLine($"{clockEmoji} {blankSpace} {lastUpdated}");

                    embed.AddInlineField(title, sbField.AppendLineBreak().ToString());
                }
            }

            embed = AddPlayStoreLink(embed);

            return embed;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a single Real response specifying a custom description, title and thumbnail URL.
        /// </summary>
        /// <param name="realResponse">>The Real response to show.</param>
        /// <param name="description">The embed's description.</param>
        /// <param name="title">Optional. The embed's title.</param>
        /// <param name="thumbnailUrl">Optional. The embed's thumbnail URL.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateRealEmbedAsync(RealResponse realResponse, string description, string title = null, string thumbnailUrl = null)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji realEmoji = new Emoji(emojis["real"]);
            Emoji whatsappEmoji = new Emoji(emojis["whatsapp"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            string realImageUrl = thumbnailUrl ?? Configuration.GetSection("images").GetSection("real")["64"];
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string embedTitle = title ?? GetTitle(realResponse);
            string lastUpdated = realResponse.Fecha.ToString(realResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string buyPrice = decimal.TryParse(realResponse?.Compra, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal compra) ? compra.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
            string sellPrice = decimal.TryParse(realResponse?.Venta, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal venta) ? venta.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";

            string buyInlineField = Format.Bold($"{realEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {buyPrice}");
            string sellInlineField = Format.Bold($"{realEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPrice}");
            string shareText = $"*{(realResponse.VentaAhorro != null ? $"Real - {embedTitle}" : embedTitle)}*{Environment.NewLine}{Environment.NewLine}Compra: \t\t$ *{buyPrice}*{Environment.NewLine}Venta: \t\t$ *{sellPrice}*";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Real)
                                                   .WithTitle(embedTitle)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(realImageUrl)
                                                   .WithFooter($"Ultima actualización: {lastUpdated} (UTC {utcOffset})", footerImageUrl)
                                                   .AddInlineField("Compra", buyInlineField)
                                                   .AddInlineField("Venta", sellInlineField);
            if (realResponse.VentaAhorro != null)
            {
                string sellPriceWithTaxes = decimal.TryParse(realResponse?.VentaAhorro, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal ventaAhorro) ? ventaAhorro.ToString("N2", GlobalConfiguration.GetLocalCultureInfo()) : "?";
                string sellWithTaxesInlineField = Format.Bold($"{realEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} $ {sellPriceWithTaxes}");
                embed.AddInlineField("Venta con Impuestos", sellWithTaxesInlineField);
                shareText += $"{Environment.NewLine}Venta c/imp: \t$ *{sellPriceWithTaxes}*";
            }

            shareText += $"{Environment.NewLine}Hora: \t\t{lastUpdated} (UTC {utcOffset})";
            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText, Api.Cuttly.ShortenUrl);
            embed = AddPlayStoreLink(embed);

            return embed;
        }

        #endregion

        #endregion
    }
}
