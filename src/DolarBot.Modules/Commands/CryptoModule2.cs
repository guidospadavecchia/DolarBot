using Discord.Commands;
using DolarBot.API;
using DolarBot.API.Models;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Crypto;
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

        [Command("dai", RunMode = RunMode.Async)]
        [Summary("Muestra la cotización del DAI (DAI) en pesos y dólares.")]
        [HelpUsageExample(false, "$dai")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetDaiRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetDaiRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    CryptoResponse result = await CryptoService.GetDashRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }
    }
}