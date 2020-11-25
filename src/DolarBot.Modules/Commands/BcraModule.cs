using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Modules.Services.Dolar;
using DolarBot.Util;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using BcraValues = DolarBot.API.ApiCalls.DolarArgentinaApi.BcraValues;

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

        /// <summary>
        /// The log4net logger.
        /// </summary>
        private readonly ILog Logger;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public BcraModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            Api = api;
        }
        #endregion

        [Command("riesgopais", RunMode = RunMode.Async)]
        [Alias("rp")]
        [Summary("Muestra el valor del riesgo país.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetRiesgoPaisValueAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    RiesgoPaisResponse result = await Api.DolarArgentina.GetRiesgoPais().ConfigureAwait(false);
                    if (result != null)
                    {
                        BcraService bcraService = new BcraService(Configuration, Api, mainEmbedColor);
                        EmbedBuilder embed = bcraService.CreateRiesgoPaisEmbed(result);
                        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("reservas", RunMode = RunMode.Async)]
        [Alias("r")]
        [Summary("Muestra las reservas de dólares del Banco Central a la fecha.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetReservasAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    BcraResponse result = await Api.DolarArgentina.GetBcraValue(BcraValues.Reservas).ConfigureAwait(false);
                    if (result != null)
                    {
                        BcraService bcraService = new BcraService(Configuration, Api, mainEmbedColor);
                        EmbedBuilder embed = bcraService.CreateReservasEmbed(result);
                        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }


        [Command("circulante", RunMode = RunMode.Async)]
        [Alias("c")]
        [Summary("Muestra la cantidad total de pesos en circulación a la fecha.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetCirculanteAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    BcraResponse result = await Api.DolarArgentina.GetBcraValue(BcraValues.Circulante).ConfigureAwait(false);
                    if (result != null)
                    {
                        BcraService bcraService = new BcraService(Configuration, Api, mainEmbedColor);
                        EmbedBuilder embed = bcraService.CreateCirculanteEmbed(result);
                        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }
    }
}