using Discord;
using Discord.Commands;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Modules.Services.Quotes;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains information related commands.
    /// </summary>
    [HelpOrder(2)]
    [HelpTitle("Otros")]
    public class MiscModule : BaseInteractiveModule
    {
        #region Constructor
        public MiscModule(IConfiguration configuration) : base(configuration) { }
        #endregion

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