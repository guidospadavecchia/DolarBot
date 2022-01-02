﻿using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Bcra;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains the BCRA (Argentine Republic Central Bank) related commands.
    /// </summary>
    [HelpOrder(9)]
    [HelpTitle("Indicadores BCRA")]
    public class BcraModule : BaseModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about BCRA rates and values.
        /// </summary>
        private readonly BcraService BcraService;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public BcraModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger)
        {
            BcraService = new BcraService(configuration, api);
        }
        #endregion

        [Command("riesgopais", RunMode = RunMode.Async)]
        [Alias("rp")]
        [Summary("Muestra el valor del riesgo país.")]
        [HelpUsageExample(false, "$riesgopais", "$rp")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetRiesgoPaisValueAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CountryRiskResponse result = await BcraService.GetCountryRisk();
                    if (result != null)
                    {
                        EmbedBuilder embed = await BcraService.CreateCountryRiskEmbedAsync(result);
                        await ReplyAsync(embed: embed.Build());
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE);
                    }
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("reservas", RunMode = RunMode.Async)]
        [Alias("rs")]
        [Summary("Muestra las reservas de dólares del Banco Central a la fecha.")]
        [HelpUsageExample(false, "$reservas", "$rs")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetReservasAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    BcraResponse result = await BcraService.GetReserves();
                    if (result != null)
                    {
                        EmbedBuilder embed = await BcraService.CreateReservesEmbedAsync(result);
                        await ReplyAsync(embed: embed.Build());
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE);
                    }
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("circulante", RunMode = RunMode.Async)]
        [Alias("c")]
        [Summary("Muestra la cantidad total aproximada de pesos en circulación a la fecha.")]
        [HelpUsageExample(false, "$circulante", "$c")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetCirculanteAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    BcraResponse result = await BcraService.GetCirculatingMoney();
                    if (result != null)
                    {
                        EmbedBuilder embed = await BcraService.CreateCirculatingMoneyEmbedAsync(result);
                        await ReplyAsync(embed: embed.Build());
                    }
                    else
                    {
                        await ReplyAsync(REQUEST_ERROR_MESSAGE);
                    }
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }
    }
}