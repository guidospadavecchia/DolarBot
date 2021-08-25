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
    [HelpOrder(6)]
    [HelpTitle("Crypto")]
    public class CryptoModule : BaseCryptoModule
    {
        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ApiCalls"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public CryptoModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger, api) { }
        #endregion

        [Command("binance", RunMode = RunMode.Async)]
        [Alias("bnb")]
        [Summary("Muestra la cotización del Binance Coin (BNB) en pesos y dólares.")]
        [HelpUsageExample(false, "$binance", "$bnb")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetBinanceCoinRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetBinanceCoinRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

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
                    CryptoResponse result = await CryptoService.GetBitcoinRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    CryptoResponse result = await CryptoService.GetBitcoinCashRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("cardano", RunMode = RunMode.Async)]
        [Alias("ada")]
        [Summary("Muestra la cotización del Cardano (ADA) en pesos y dólares.")]
        [HelpUsageExample(false, "$cardano", "$ada")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetCardanoRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetCardanoRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command("chainlink", RunMode = RunMode.Async)]
        [Alias("link")]
        [Summary("Muestra la cotización del Chainlink (LINK) en pesos y dólares.")]
        [HelpUsageExample(false, "$chainlink", "$link")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetChainlinkRatesAsync()
        {
            try
            {
                using (Context.Channel.EnterTypingState())
                {
                    CryptoResponse result = await CryptoService.GetChainlinkRate();
                    await SendCryptoReply(result);
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

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
    }
}