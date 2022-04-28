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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmbedPage = Discord.Addons.Interactive.PaginatedMessage.Page;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains information related commands.
    /// </summary>
    [HelpOrder(98)]
    [HelpTitle("Otros")]
    public class MiscModule : BaseModule
    {
        #region Vars
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
        public MiscModule(IConfiguration configuration, ILog logger, ApiCalls api) : base(configuration, logger)
        {
            Api = api;
        }
        #endregion

        [Command("bancos", RunMode = RunMode.Async)]
        [Alias("b")]
        [Summary("Muestra la lista de bancos disponibles para cada una de las monedas principales.")]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task GetBanks()
        {
            try
            {
                string commandPrefix = Configuration["commandPrefix"];
                Emoji bankEmoji = new(":bank:");
                string bankImageUrl = Configuration.GetSection("images").GetSection("bank")["64"];

                List<EmbedBuilder> embeds = new();
                List<EmbedPage> pages = new();
                int pageCount = 0;

                Currencies[] currencies = Enum.GetValues(typeof(Currencies)).Cast<Currencies>().ToArray();
                foreach (Currencies currency in currencies)
                {
                    IBankCurrencyService currencyService = GetCurrencyService(currency);
                    Banks[] banks = currencyService.GetValidBanks();
                    string bankList = string.Join(Environment.NewLine, banks.Select(x => $"{bankEmoji} {Format.Code(x.ToString().ToLower())}: {Format.Italics(x.GetDescription())}.")).AppendLineBreak();

                    embeds.Add(new EmbedBuilder().AddField($"{Format.Bold(currency.GetDescription())} ({Format.Code($"{commandPrefix}{currency.ToString().ToLower()}")})", bankList));
                }

                foreach (EmbedBuilder embed in embeds)
                {
                    embed.AddCommandDeprecationNotice(Configuration);
                    pages.Add(new EmbedPage
                    {
                        Description = $"Parámetros disponibles para utilizar en los comandos de las monedas principales ({string.Join(", ", currencies.Select(x => $"{Format.Code($"{commandPrefix}{x.ToString().ToLower()}")}"))}).".AppendLineBreak(),
                        Title = "Bancos disponibles por moneda",
                        Fields = embed.Fields,
                        Color = GlobalConfiguration.Colors.Currency,
                        FooterOverride = new EmbedFooterBuilder
                        {
                            Text = $"Página {++pageCount} de {embeds.Count}"
                        },
                        ThumbnailUrl = bankImageUrl
                    });
                }

                await SendPagedReplyAsync(pages);
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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
                    await ReplyAsync($"{Format.Bold("Error")}. No se puede acceder a la información solicitada en este momento. Por favor, intentá nuevamente más tarde.");
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
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