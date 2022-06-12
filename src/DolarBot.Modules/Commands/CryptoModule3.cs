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
    [HelpOrder(8)]
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
                    CryptoResponse result = await CryptoService.GetCryptoRateByCode("polkadot");
                    await SendCryptoReply(result, "Polkadot");
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    CryptoResponse result = await CryptoService.GetCryptoRateByCode("ripple");
                    await SendCryptoReply(result, "Ripple");
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    CryptoResponse result = await CryptoService.GetCryptoRateByCode("stellar");
                    await SendCryptoReply(result, "Stellar");
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    CryptoResponse result = await CryptoService.GetCryptoRateByCode("tether");
                    await SendCryptoReply(result, "Tether");
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    CryptoResponse result = await CryptoService.GetCryptoRateByCode("theta-token");
                    await SendCryptoReply(result, "Theta");
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    CryptoResponse result = await CryptoService.GetCryptoRateByCode("uniswap");
                    await SendCryptoReply(result, "Uniswap");
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }
    }
}