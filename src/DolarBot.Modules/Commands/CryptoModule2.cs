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
    [HelpOrder(6)]
    [HelpTitle("Crypto")]
    public class CryptoModule2 : BaseCryptoModule
    {
        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public CryptoModule2(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger, api) { }
        #endregion

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
                    CryptoResponse result = await CryptoService.GetDashRate();
                    if (result != null)
                    {
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(result);
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
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("dogecoin", RunMode = RunMode.Async)]
        [Alias("doge")]
        [Summary("Muestra la cotización del Dogecoin (DOGE) en pesos y dólares.")]
        [HelpUsageExample(false, "$dogecoin", "$doge")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetDogecoinRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetDogecoinRate();
                    if (result != null)
                    {
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(result);
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
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
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
                    CryptoResponse result = await CryptoService.GetEthereumRate();
                    if (result != null)
                    {
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(result);
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
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
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
                    CryptoResponse result = await CryptoService.GetLitecoinRate();
                    if (result != null)
                    {
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(result);
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
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
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
                    CryptoResponse result = await CryptoService.GetMoneroRate();
                    if (result != null)
                    {
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(result);
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
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("polkadot", RunMode = RunMode.Async)]
        [Alias("dot")]
        [Summary("Muestra la cotización del Polkadot (DOT) en pesos y dólares.")]
        [HelpUsageExample(false, "$polkadot", "$dot")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetPolkadotRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetPolkadotRate();
                    if (result != null)
                    {
                        EmbedBuilder embed = await CryptoService.CreateCryptoEmbedAsync(result);
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
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }
    }
}