using Discord;
using Discord.Commands;
using DolarBot.API;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Services.Banking;
using DolarBot.Services.Banking.Interfaces;
using DolarBot.Services.Currencies;
using DolarBot.Services.Dolar;
using DolarBot.Services.Euro;
using DolarBot.Services.Quotes;
using DolarBot.Services.Real;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains information related commands.
    /// </summary>
    [HelpOrder(98)]
    [HelpTitle("Otros")]
    public class MiscModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// The log4net logger.
        /// </summary>
        private readonly ILog Logger;
        /// <summary>
        /// Provides several methods to interact with the different APIs.
        /// </summary>
        private readonly ApiCalls Api;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ILog"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="logger">Provides access to the different APIs.</param>
        public MiscModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration)
        {
            Logger = logger;
            Api = api;
        }
        #endregion

        [Command("monedas")]
        [Alias("m")]
        [Summary("Muestra la lista de monedas soportadas.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetCurrencies()
        {
            try
            {
                string commandPrefix = Configuration["commandPrefix"];
                string currencies = string.Join(", ", Enum.GetValues(typeof(Currencies)).Cast<Currencies>().Select(x => Format.Bold(x.GetDescription())));
                await ReplyAsync($"Monedas disponibles: {currencies}.");
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("bancos")]
        [Alias("b")]
        [Summary("Muestra la lista de bancos disponibles para cada moneda.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetBanks(
            [Summary("Opcional. Indica la moneda para listar sus bancos disponibles. Los valores posibles son aquellos devueltos por el comando `$monedas`.")]
            string moneda = null)
        {
            try
            {
                string commandPrefix = Configuration["commandPrefix"];
                string currencyCommand = GetType().GetMethod(nameof(GetCurrencies)).GetCustomAttributes(true).OfType<CommandAttribute>().First().Text;

                if (moneda != null)
                {
                    string userInput = Format.Sanitize(moneda).RemoveFormat(true);
                    if (Enum.TryParse(userInput, true, out Currencies currency))
                    {
                        IBankCurrencyService currencyService = GetCurrencyService(currency);
                        Banks[] banks = currencyService.GetValidBanks();
                        string bankList = string.Join(", ", banks.Select(x => Format.Code(x.ToString())));
                        await ReplyAsync($"Parametros disponibles para utilizar en comandos de {Format.Bold(currency.GetDescription())}:".AppendLineBreak().AppendLineBreak() + $"{bankList}.");
                    }
                    else
                    {
                        //Unknown parameter
                        await ReplyAsync($"Moneda '{Format.Bold(userInput)}' inexistente. Verifique las monedas disponibles con {Format.Code($"{commandPrefix}{currencyCommand}")}.");
                    }
                }
                else
                {
                    //Parameter not specified
                    Emoji bankEmoji = new Emoji(":bank:");
                    string message = $"Parametros de {Format.Bold("bancos disponibles por moneda")}:".AppendLineBreak().AppendLineBreak();
                    Currencies[] currencies = Enum.GetValues(typeof(Currencies)).Cast<Currencies>().ToArray();
                    foreach (Currencies currency in currencies)
                    {
                        IBankCurrencyService currencyService = GetCurrencyService(currency);
                        Banks[] banks = currencyService.GetValidBanks();
                        string bankList = string.Join(", ", banks.Select(x => Format.Code(x.ToString())));
                        message += $"{bankEmoji} {Format.Bold(currency.GetDescription())}: {bankList}.".AppendLineBreak();
                    }

                    await ReplyAsync(message);
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command("frase", RunMode = RunMode.Async)]
        [Alias("f")]
        [Summary("Muestra una frase célebre de la economía argentina.")]
        [RateLimit(1, 2, Measure.Seconds)]
        public async Task GetRandomQuote()
        {
            try
            {
                Quote quote = QuoteService.GetRandomQuote();
                if (quote != null && !string.IsNullOrWhiteSpace(quote.Text))
                {
                    await ReplyAsync($"{Format.Italics($"\"{quote.Text}\"")} -{Format.Bold(quote.Author)}.");
                }
                else
                {
                    await ReplyAsync($"{Format.Bold("Error")}. No se puede acceder a las frases célebres en este momento. Intentá nuevamente más tarde.");
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"]));
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        /// <summary>
        /// Creates a new instance of the corresponding currency service according to <paramref name="currency"/>.
        /// </summary>
        /// <param name="currency">The currency type.</param>
        /// <returns>An instantiated service.</returns>
        private IBankCurrencyService GetCurrencyService(Currencies currency)
        {
            return currency switch
            {
                Currencies.Dolar => new DolarService(Configuration, Api),
                Currencies.Euro => new EuroService(Configuration, Api),
                Currencies.Real => new RealService(Configuration, Api),
                _ => throw new ArgumentException("Invalid currency.")
            };
        }
    }
}