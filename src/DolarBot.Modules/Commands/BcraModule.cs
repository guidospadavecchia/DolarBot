using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Modules.Services.Dolar;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains the BCRA (Argentine Republic Central Bank) related commands.
    /// </summary>
    [HelpOrder(2)]
    [HelpTitle("Indicadores BCRA")]
    public class BcraModule : BaseInteractiveModule
    {
        #region Constants
        private const string REQUEST_ERROR_MESSAGE = "Error: No se pudo obtener el valor solicitado. Intente nuevamente en más tarde.";
        #endregion

        #region Vars
        /// <summary>
        /// Color for the embed messages.
        /// </summary>
        private readonly Color mainEmbedColor = new Color(67, 181, 129);

        /// <summary>
        /// Provides access to the different APIs.
        /// </summary>
        protected readonly ApiCalls Api;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        public BcraModule(IConfiguration configuration, ApiCalls api) : base(configuration)
        {
            Api = api;
        }
        #endregion

        [Command("riesgopais", RunMode = RunMode.Async)]
        [Alias("rp")]
        [Summary("Muestra el valor del riesgo país.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetRiesgoPaisValueAsync()
        {
            using (Context.Channel.EnterTypingState())
            {
                RiesgoPaisResponse result = await Api.DolarArgentina.GetRiesgoPais().ConfigureAwait(false);
                if (result != null)
                {
                    RiesgoPaisService riesgoPaisService = new RiesgoPaisService(Configuration, Api, mainEmbedColor);
                    EmbedBuilder embed = riesgoPaisService.CreateRiesgoPaisEmbed(result);
                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                }
            }
        }
    }
}