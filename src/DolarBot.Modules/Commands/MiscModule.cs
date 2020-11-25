using Discord;
using Discord.Commands;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Modules.Services.Banking;
using DolarBot.Modules.Services.Quotes;
using DolarBot.Util;
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
    [HelpOrder(5)]
    [HelpTitle("Otros")]
    public class MiscModule : BaseInteractiveModule
    {
        #region Vars
        /// <summary>
        /// The log4net logger.
        /// </summary>
        private readonly ILog Logger;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the <see cref="IConfiguration"/> and <see cref="ILog"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="logger">Provides access to the different APIs.</param>
        public MiscModule(IConfiguration configuration, ILog logger) : base(configuration)
        {
            Logger = logger;
        }
        #endregion

        [Command("bancos")]
        [Alias("b")]
        [Summary("Muestra la lista de bancos disponibles para obtener las cotizaciones.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetBanks()
        {
            try
            {
                string commandPrefix = Configuration["commandPrefix"];
                string banks = string.Join(", ", Enum.GetNames(typeof(Banks)).Select(b => Format.Bold(b)));
                await ReplyAsync($"Parámetros disponibles del comando {Format.Code($"{commandPrefix}dolar <banco>")}: {banks}.").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
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
                    await ReplyAsync($"{Format.Italics($"\"{quote.Text}\"")} -{Format.Bold(quote.Author)}.").ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync($"{Format.Bold("Error")}. No se puede acceder a las frases célebres en este momento. Intentá nuevamente más tarde.").ConfigureAwait(false);
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