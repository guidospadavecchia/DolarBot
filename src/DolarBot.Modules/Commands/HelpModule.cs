using Discord.Commands;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Alias("ayuda")]
        [Summary("Muestra los comandos disponibles.")]
        public async Task SendHelp()
        {
            //TODO
        }
    }
}