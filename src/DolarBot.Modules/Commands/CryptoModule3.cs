using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Crypto;
using DolarBot.Util;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains cryptocurrency related commands.
    /// </summary>
    [HelpOrder(7)]
    [HelpTitle("Crypto")]
    public class CryptoModule3 : BaseCryptoModule
    {
        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public CryptoModule3(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger, api) { }
        #endregion

        [Command("ripple", RunMode = RunMode.Async)]
        [Alias("xrp")]
        [Summary("Muestra la cotización del Ripple (XRP) en pesos y dólares.")]
        [HelpUsageExample(false, "$ripple", "$xrp")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetRippleRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetRippleRate().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(result);
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
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("stellar", RunMode = RunMode.Async)]
        [Alias("xlm")]
        [Summary("Muestra la cotización del Stellar (XLM) en pesos y dólares.")]
        [HelpUsageExample(false, "$stellar", "$xlm")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetStellarRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetStellarRate().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(result);
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
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("tether", RunMode = RunMode.Async)]
        [Alias("usdt")]
        [Summary("Muestra la cotización del Tether (USDT) en pesos y dólares.")]
        [HelpUsageExample(false, "$tether", "$usdt")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetTetherRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetTetherRate().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(result);
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
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("theta", RunMode = RunMode.Async)]
        [Summary("Muestra la cotización del Theta (THETA) en pesos y dólares.")]
        [HelpUsageExample(false, "$theta")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetThetaRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetThetaRate().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(result);
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
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("uniswap", RunMode = RunMode.Async)]
        [Alias("uni")]
        [Summary("Muestra la cotización del Uniswap (UNI) en pesos y dólares.")]
        [HelpUsageExample(false, "$uniswap", "$uni")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetUniswapRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetUniswapRate().ConfigureAwait(false);
                    if (result != null)
                    {
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(result);
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
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }
    }
}