using Discord.Commands;
using System.Threading.Tasks;

namespace SteamBuddy.Modules.Commands
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Summary("Sends a DM with the bot's command list.")]
        public async Task SendHelp(int help1, int help2)
        {
            //TODO
        }
    }
}