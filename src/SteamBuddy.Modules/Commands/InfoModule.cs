using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace SteamBuddy.Modules.Commands
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("date")]
        [Summary("Displays current bot's date and time.")]
        public async Task GetDateAsync()
        {
            await ReplyAsync($"{Context.User.Mention}, current time for bot is {DateTime.Now:dd/MM/yyyy HH:mm:ss:fff}");
        }
    }
}
