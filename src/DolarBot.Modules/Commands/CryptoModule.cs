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
    [HelpOrder(5)]
    [HelpTitle("Crypto")]
    public class CryptoModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// Provides methods to retrieve information about cryptocurrencies rates and values.
        /// </summary>
        private readonly CryptoService CryptoService;

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
        public CryptoModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            CryptoService = new CryptoService(configuration, api);
        }
        #endregion

        [Command("bitcoin", RunMode = RunMode.Async)]
        [Alias("btc")]
        [Summary("Muestra la cotización del Bitcoin (BTC) en pesos y dólares.")]
        [HelpUsageExample(false, "$bitcoin", "$btc")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetBitcoinRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetBitcoinRate().ConfigureAwait(false);
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

        [Command("bitcoincash", RunMode = RunMode.Async)]
        [Alias("bch")]
        [Summary("Muestra la cotización del Bitcoin Cash (BCH) en pesos y dólares.")]
        [HelpUsageExample(false, "$bitcoincash", "$bch")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetBitcoinCashRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetBitcoinCashRate().ConfigureAwait(false);
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

        [Command("ethereum", RunMode = RunMode.Async)]
        [Alias("eth")]
        [Summary("Muestra la cotización del Ethereum (ETH) en pesos y dólares.")]
        [HelpUsageExample(false, "$ethereum", "$eth")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetEthereumRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetEthereumRate().ConfigureAwait(false);
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

        [Command("dash", RunMode = RunMode.Async)]
        [Summary("Muestra la cotización del Dash (DASH) en pesos y dólares.")]
        [HelpUsageExample(false, "$dash")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetDashRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetDashRate().ConfigureAwait(false);
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

        [Command("litecoin", RunMode = RunMode.Async)]
        [Alias("ltc")]
        [Summary("Muestra la cotización del Litecoin (LTC) en pesos y dólares.")]
        [HelpUsageExample(false, "$litecoin", "$ltc")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetLitecoinRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetLitecoinRate().ConfigureAwait(false);
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

        [Command("monero", RunMode = RunMode.Async)]
        [Alias("xmr")]
        [Summary("Muestra la cotización del Monero (XMR) en pesos y dólares.")]
        [HelpUsageExample(false, "$monero", "$xmr")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetMoneroRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetMoneroRate().ConfigureAwait(false);
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
    }
}