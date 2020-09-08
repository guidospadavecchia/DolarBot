using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SteamBuddy.Modules.Commands
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private readonly Color infoEmbedColor = new Color(23, 99, 154);

        [Command("date")]
        [Summary("Displays current bot's date and time.")]
        public async Task GetDateAsync()
        {
            string timestamp = Format.Bold(DateTime.Now.ToString("yyyy/MM/dd - HH:mm:ss"));
            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("Bot's Server Date")
                                 .WithColor(infoEmbedColor)
                                 .WithDescription($"Current date and time for bot's server is {timestamp}");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("sid")]
        [Summary("Displays the current server Discord ID")]
        public async Task GetServerId()
        {
            string sid = Format.Bold(Context.Guild.Id.ToString());
            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("Server ID")
                                 .WithColor(infoEmbedColor)
                                 .WithDescription($"The current server ID is {sid}");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("ping")]
        [Summary("Pings the bot showing the estimated round-trip latency to the gateway server.")]
        public async Task Ping()
        {
            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("Pinging...")
                                 .WithColor(infoEmbedColor);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var message = await ReplyAsync(embed: embed.Build());
            sw.Stop();

            string responseTime = Format.Bold($"{Context.Client.Latency} ms");
            string latency = Format.Bold($"{sw.ElapsedMilliseconds} ms");
            await message.ModifyAsync(x =>
            {
                embed.WithTitle("Ping Result")
                     .WithDescription(
                        new StringBuilder()
                        .AppendLine($"Response time: {responseTime}")
                        .AppendLine($"Gateway latency: {latency}")
                        .ToString()
                     );
                x.Embed = embed.Build();
            });
        }
    }
}