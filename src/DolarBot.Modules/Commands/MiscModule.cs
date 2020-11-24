using Discord;
using Discord.Commands;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Modules.Services.Dolar;
using DolarBot.Modules.Services.Quotes;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains information related commands.
    /// </summary>
    [HelpOrder(3)]
    [HelpTitle("Otros")]
    public class MiscModule : BaseInteractiveModule
    {
        #region Constructor
        public MiscModule(IConfiguration configuration) : base(configuration) { }
        #endregion

        [Command("bancos")]
        [Alias("b")]
        [Summary("Muestra la lista de bancos disponibles para obtener las cotizaciones.")]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task GetBanks()
        {
            string commandPrefix = Configuration["commandPrefix"];
            string banks = string.Join(", ", Enum.GetNames(typeof(Banks)).Select(b => Format.Bold(b)));
            await ReplyAsync($"Parámetros disponibles del comando {Format.Code($"{commandPrefix}dolar <banco>")}: {banks}.").ConfigureAwait(false);
        }

        [Command("frase", RunMode = RunMode.Async)]
        [Alias("f")]
        [Summary("Muestra una frase célebre de la economía argentina.")]
        [RateLimit(1, 2, Measure.Seconds)]
        public async Task GetRandomQuote()
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
    }
}