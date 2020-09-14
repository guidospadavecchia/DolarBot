using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DolarBot.Modules.Attributes;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace DolarBot.Modules.Commands
{
    [HelpOrder(2)]
    [HelpTitle("Información")]
    public class InfoModule : InteractiveBase<SocketCommandContext>
    {
        private readonly Color infoEmbedColor = new Color(23, 99, 154);
        private readonly IConfiguration configuration;

        public InfoModule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [Command("date")]        
        [Summary("Muestra la fecha y hora del servidor donde se encuentra el bot.")]
        public async Task GetDateAsync()
        {
            string timestamp = Format.Bold(DateTime.Now.ToString("yyyy/MM/dd - HH:mm:ss"));
            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("Fecha y Hora")
                                 .WithColor(infoEmbedColor)
                                 .WithDescription($"La fecha y hora del servidor es {timestamp}");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("sid")]
        [Summary("Muestra el ID del servidor de Discord actual.")]
        public async Task GetServerId()
        {
            string sid = Format.Bold(Context.Guild.Id.ToString());
            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("Server ID")
                                 .WithColor(infoEmbedColor)
                                 .WithDescription($"El ID del servidor es {sid}");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("ping")]
        [Summary("Muestra la latencia del bot de Discord.")]
        public async Task Ping()
        {
            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("Procesando...")
                                 .WithColor(infoEmbedColor);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var message = await ReplyAsync(embed: embed.Build());
            sw.Stop();

            string responseTime = Format.Bold($"{Context.Client.Latency} ms");
            string latency = Format.Bold($"{sw.ElapsedMilliseconds} ms");
            await message.ModifyAsync(x =>
            {
                embed.WithTitle("Resultado del Ping")
                     .WithDescription(
                        new StringBuilder()
                        .AppendLine($"Tiempo de respuesta del bot: {responseTime}")
                        .AppendLine($"Latencia de la puerta de enlace: {latency}")
                        .ToString()
                     );
                x.Embed = embed.Build();
            });
        }

        [Command("invite")]
        [Alias("invitar")]
        [Summary("Devuelve el link de invitación del bot en Discord.")]
        public async Task GetInviteLink()
        {
            EmbedBuilder embed = new EmbedBuilder()
                                 .WithTitle("DolarBot")
                                 .WithColor(infoEmbedColor)
                                 .WithDescription($"Invita al bot utilizando el siguiente {Format.Url("link", configuration["inviteLink"])}");

            await ReplyAsync(embed: embed.Build());
        }
    }
}