using Discord;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Metal = DolarBot.API.ApiCalls.DolarBotApi.Metals;

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
            return await Api.DolarBot.GetMetalPrice(Metal.Gold).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the silver price.
        /// </summary>
        /// <returns>A <see cref="MetalResponse"/> object.</returns>
        public async Task<MetalResponse> GetSilverPrice()
        {
            return await Api.DolarBot.GetMetalPrice(Metal.Silver).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches the copper price.
        /// </summary>
        /// <returns>A <see cref="MetalResponse"/> object.</returns>
        public async Task<MetalResponse> GetCopperPrice()
        {
            return await Api.DolarBot.GetMetalPrice(Metal.Copper).ConfigureAwait(false);
        }

        #endregion

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for a <see cref="MetalResponse"/>.
        /// </summary>
        /// <param name="metalResponse">The metal response.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateMetalEmbed(MetalResponse metalResponse)
        {
            Emoji metalEmoji = GetEmoji(metalResponse.Type);
            string thumbnailUrl = GetThumbnailUrl(metalResponse.Type);
            string footerImageUrl = Configuration.GetSection("images").GetSection("clock")["32"];
            string value = decimal.TryParse(metalResponse?.Valor, NumberStyles.Any, Api.DolarBot.GetApiCulture(), out decimal valor) ? Format.Bold($"US$ {valor.ToString("F2", GlobalConfiguration.GetLocalCultureInfo())} / {metalResponse.Unidad.ToLower()}") : "No informado";
            string title = $"Cotización {(metalResponse.Type != Metal.Silver ? "del" : "de la")} {GetName(metalResponse.Type).Capitalize()}";
            string description = $"Valor internacional {(metalResponse.Type != Metal.Silver ? "del" : "de la")} {Format.Bold(GetName(metalResponse.Type).ToLower())} expresado en {Format.Bold("dólares")} por {Format.Bold(metalResponse.Unidad.ToLower())}.";

            EmbedBuilder embed = new EmbedBuilder().WithColor(GetColor(metalResponse.Type))
                                                   .WithTitle(title)
                                                   .WithDescription(description.AppendLineBreak())
                                                   .WithThumbnailUrl(thumbnailUrl)
                                                   .WithFooter(new EmbedFooterBuilder()
                                                   {
                                                       Text = $"Ultima actualización: {TimeZoneInfo.ConvertTimeFromUtc(metalResponse.Fecha, GlobalConfiguration.GetLocalTimeZoneInfo()):dd/MM/yyyy - HH:mm}",
                                                       IconUrl = footerImageUrl
                                                   })
                                                   .AddField($"Valor", $"{metalEmoji} {GlobalConfiguration.Constants.BLANK_SPACE} {value}".AppendLineBreak());
            return embed;
        }

        /// <summary>
        /// Returns the corresponding thumbnail URL associated to the <paramref name="metal"/> parameter.
        /// </summary>
        /// <param name="metal">The type of precious metal.</param>
        /// <returns>The corresponding thumbnail URL.</returns>
        private string GetThumbnailUrl(Metal metal)
        {
            var images = Configuration.GetSection("images");
            return metal switch
            {
                Metal.Gold => images.GetSection("gold")["64"],
                Metal.Silver => images.GetSection("silver")["64"],
                Metal.Copper => images.GetSection("copper")["64"],
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Returns the corresponding emoji associated to the <paramref name="metal"/> parameter.
        /// </summary>
        /// <param name="metal">The type of precious metal.</param>
        /// <returns>The corresponding <see cref="Emoji"/> object.</returns>
        private Emoji GetEmoji(Metal metal)
        {
            var emojis = Configuration.GetSection("customEmojis");
            return metal switch
            {
                Metal.Gold => new Emoji(emojis["goldCoin"]),
                Metal.Silver => new Emoji(emojis["silverCoin"]),
                Metal.Copper => new Emoji(emojis["copperCoin"]),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Returns the corresponding name associated to the <paramref name="metal"/> parameter.
        /// </summary>
        /// <param name="metal">The type of precious metal.</param>
        /// <returns>The corresponding name.</returns>
        private string GetName(Metal metal)
        {
            return metal switch
            {
                Metal.Gold => "Oro",
                Metal.Silver => "Plata",
                Metal.Copper => "Cobre",
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Returns the corresponding color associated to the <paramref name="metal"/> parameter.
        /// </summary>
        /// <param name="metal">The type of precious metal.</param>
        /// <returns>The corresponding color.</returns>
        private Color GetColor(Metal metal)
        {
            return metal switch
            {
                Metal.Gold => GlobalConfiguration.Colors.Gold,
                Metal.Silver => GlobalConfiguration.Colors.Silver,
                Metal.Copper => GlobalConfiguration.Colors.Copper,
                _ => throw new NotImplementedException()
            };
        }

        #endregion

        #endregion
    }
}
