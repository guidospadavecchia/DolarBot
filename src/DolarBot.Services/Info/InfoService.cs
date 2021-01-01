using Discord;
using DolarBot.API;
using DolarBot.Services.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace DolarBot.Services.Info
{
    /// <summary>
    /// Contains several methods to process information related commands.
    /// </summary>
    public class InfoService : BaseService
    {
        #region Constants
        private const string API_STATUS_OK = "OK";
        private const string API_STATUS_ERROR = "Error";
        private const string DISCORD_LATENCY_OK = "OK";
        private const string DISCORD_LATENCY_HIGH = "Delayed";
        private const int DISCORD_MAX_ACCEPTABLE_LATENCY = 200;
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="BcraService"/> object with the provided configuration and API object.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public InfoService(IConfiguration configuration, ApiCalls api) : base(configuration, api) { }

        #endregion

        #region Methods

        #region API Calls

        /// <summary>
        /// Returns a string containing the current API status.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetApiStatus()
        {
            HttpStatusCode? httpStatusCode = await Api.DolarBot.GetApiStatus();
            if (httpStatusCode != null)
            {
                int statusCode = (int)httpStatusCode;
                return statusCode >= 200 && statusCode < 300 ? API_STATUS_OK : API_STATUS_ERROR;
            }
            else
            {
                return API_STATUS_ERROR;
            }
        }

        #endregion

        #region Embeds

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object containing the bot's current status.
        /// </summary>
        /// <param name="apiStatus">The current status.</param>
        /// <param name="discordLatency">The current latency to Discord gateway.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        public EmbedBuilder CreateStatusEmbed(string apiStatus, int discordLatency)
        {
            Emoji okEmoji = new Emoji(":white_check_mark:");
            Emoji warningEmoji = new Emoji(":warning:");
            Emoji errorEmoji = new Emoji(":red_circle:");
            Emoji apiStatusEmoji = apiStatus == API_STATUS_OK ? okEmoji : errorEmoji;
            Emoji discordStatusEmoji = discordLatency < DISCORD_MAX_ACCEPTABLE_LATENCY ? okEmoji : warningEmoji;

            string infoImageUrl = Configuration.GetSection("images")?.GetSection("info")?["64"];
            string discordStatus = discordLatency < DISCORD_MAX_ACCEPTABLE_LATENCY ? DISCORD_LATENCY_OK : DISCORD_LATENCY_HIGH;

            EmbedBuilder embed = new EmbedBuilder()
                     .WithTitle("Status")
                     .WithColor(GlobalConfiguration.Colors.Info)
                     .WithThumbnailUrl(infoImageUrl)
                     .WithDescription($"Estado general del bot".AppendLineBreak())
                     .AddField("DolarBot", $"{okEmoji} {Format.Bold(API_STATUS_OK)}".AppendLineBreak())
                     .AddField("DolarBot API", $"{apiStatusEmoji} {Format.Bold(apiStatus)}".AppendLineBreak())
                     .AddField("Discord Gateway", $"{discordStatusEmoji} {Format.Bold(discordStatus)}")
                     .WithCurrentTimestamp();

            return embed;
        }

        #endregion

        #endregion
    }
}
