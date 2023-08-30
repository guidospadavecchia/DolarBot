using Discord;
using DolarBot.API;
using DolarBot.API.Enums;
using DolarBot.API.Models;
using DolarBot.API.Services.DolarBotApi;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace DolarBot.Services.Metals
{
    /// <summary>
    /// Contains several methods to process precious metal related commands.
    /// </summary>
    public class MetalService : BaseService
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="MetalService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public MetalService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        #region API Calls

        /// <summary>
        /// Fetches the gold price.
        /// </summary>
        /// <returns>A <see cref="MetalResponse"/> object.</returns>
        public async Task<MetalResponse> GetGoldPrice()
        {
            return await Api.DolarBot.GetMetalRate(MetalEndpoints.Gold);
        }

        /// <summary>
        /// Fetches the silver price.
        /// </summary>
        /// <returns>A <see cref="MetalResponse"/> object.</returns>
        public async Task<MetalResponse> GetSilverPrice()
        {
            return await Api.DolarBot.GetMetalRate(MetalEndpoints.Silver);
        }

        /// <summary>
        /// Fetches the copper price.
        /// </summary>
        /// <returns>A <see cref="MetalResponse"/> object.</returns>
        public async Task<MetalResponse> GetCopperPrice()
        {
            return await Api.DolarBot.GetMetalRate(MetalEndpoints.Copper);
        }

        #endregion

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="MetalResponse"/>.
        /// </summary>
        /// <param name="metalResponse">The metal response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public async Task<EmbedBuilder> CreateMetalEmbedAsync(MetalResponse metalResponse)
        {
            var emojis = Configuration.GetSection("customEmojis");
            Emoji metalEmoji = GetEmoji(metalResponse.Type);
            Emoji whatsappEmoji = new(emojis["whatsapp"]);

            TimeZoneInfo localTimeZone = GlobalConfiguration.GetLocalTimeZoneInfo();
            int utcOffset = localTimeZone.GetUtcOffset(DateTime.UtcNow).Hours;

            string thumbnailUrl = GetThumbnailUrl(metalResponse.Type);
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            decimal value = decimal.TryParse(metalResponse?.Valor, NumberStyles.Any, DolarBotApiService.GetApiCulture(), out decimal valor) ? valor : 0;
            string valueText = value > 0 ? Format.Bold($"US$ {valor.ToString("N2", GlobalConfiguration.GetLocalCultureInfo())} / {metalResponse.Unidad.ToLower()}") : "No informado";
            string title = $"Cotización {(metalResponse.Type != MetalEndpoints.Silver ? "del" : "de la")} {GetName(metalResponse.Type).Capitalize()}";
            string description = $"Valor internacional {(metalResponse.Type != MetalEndpoints.Silver ? "del" : "de la")} {Format.Bold(GetName(metalResponse.Type).ToLower())} expresado en {Format.Bold("dólares")} por {Format.Bold(metalResponse.Unidad.ToLower())}.";
            string lastUpdated = metalResponse.Fecha.ToString(metalResponse.Fecha.Date == TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone).Date ? "HH:mm" : "dd/MM/yyyy - HH:mm");
            string shareText = $"*{title}*{Environment.NewLine}{Environment.NewLine}US$ *{value.ToString("N2", GlobalConfiguration.GetLocalCultureInfo())} / {metalResponse.Unidad.ToLower()}*{Environment.NewLine}Hora: {lastUpdated} (UTC {utcOffset})";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GetColor(metalResponse.Type))
                                                   .WithTitle(title)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {lastUpdated} (UTC {utcOffset})",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddField($"Valor", $"{metalEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {valueText}".AppendLineBreak());

            await embed.AddFieldWhatsAppShare(whatsappEmoji, shareText);
            return embed.AddPlayStoreLink(Configuration, true)
                        .AddDonationLink(Configuration, true);
        }

        /// <summary>
        /// Returns the corresponding thumbnail URL associated to the <paramref name="metal"/> parameter.
        /// </summary>
        /// <param name="metal">The type of precious metal.</param>
        /// <returns>The corresponding thumbnail URL.</returns>
        private string GetThumbnailUrl(MetalEndpoints metal)
        {
            var images = Configuration.GetSection("images");
            return metal switch
            {
                MetalEndpoints.Gold => images.GetSection("gold")["64"],
                MetalEndpoints.Silver => images.GetSection("silver")["64"],
                MetalEndpoints.Copper => images.GetSection("copper")["64"],
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Returns the corresponding emoji associated to the <paramref name="metal"/> parameter.
        /// </summary>
        /// <param name="metal">The type of precious metal.</param>
        /// <returns>The corresponding <see cref="Emoji"/> object.</returns>
        private Emoji GetEmoji(MetalEndpoints metal)
        {
            var emojis = Configuration.GetSection("customEmojis");
            return metal switch
            {
                MetalEndpoints.Gold => new Emoji(emojis["goldCoin"]),
                MetalEndpoints.Silver => new Emoji(emojis["silverCoin"]),
                MetalEndpoints.Copper => new Emoji(emojis["copperCoin"]),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Returns the corresponding name associated to the <paramref name="metal"/> parameter.
        /// </summary>
        /// <param name="metal">The type of precious metal.</param>
        /// <returns>The corresponding name.</returns>
        private static string GetName(MetalEndpoints metal)
        {
            return metal switch
            {
                MetalEndpoints.Gold => "Oro",
                MetalEndpoints.Silver => "Plata",
                MetalEndpoints.Copper => "Cobre",
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Returns the corresponding color associated to the <paramref name="metal"/> parameter.
        /// </summary>
        /// <param name="metal">The type of precious metal.</param>
        /// <returns>The corresponding color.</returns>
        private static Color GetColor(MetalEndpoints metal)
        {
            return metal switch
            {
                MetalEndpoints.Gold => GlobalConfiguration.Colors.Gold,
                MetalEndpoints.Silver => GlobalConfiguration.Colors.Silver,
                MetalEndpoints.Copper => GlobalConfiguration.Colors.Copper,
                _ => throw new NotImplementedException()
            };
        }

        #endregion

        #endregion
    }
}
