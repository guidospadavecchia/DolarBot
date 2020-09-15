using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DolarBot.Modules.Attributes;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParameterInfo = Discord.Commands.ParameterInfo;

namespace DolarBot.Modules.Commands
{
    public class HelpModule : InteractiveBase<SocketCommandContext>
    {
        #region Constants
        private const string HELP_COMMAND = "help";
        private const string HELP_ALIAS = "ayuda";
        private const string HELP_COMMAND_DM = "helpdm";
        private const string HELP_ALIAS_DM = "ayudadm";
        private const string HELP_SUMMARY = "Muestra los comandos disponibles.";
        private const string HELP_COMMAND_SUMMARY = "Muestra información sobre un comando.";
        private const string HELP_SUMMARY_DM = "Envía la ayuda por mensaje privado.";
        private const string HELP_COMMAND_SUMMARY_DM = "Envía el comando por privado.";
        #endregion

        private readonly Color helpEmbedColor = Color.Blue;
        private readonly CommandService commands;
        private readonly IConfiguration configuration;

        public HelpModule(CommandService commands, IConfiguration configuration)
        {
            this.commands = commands;
            this.configuration = configuration;
        }

        [Command(HELP_COMMAND)]
        [Alias(HELP_ALIAS)]
        [Summary(HELP_SUMMARY)]
        public async Task SendHelp(string command = null)
        {
            if (CommandExists(command))
            {
                EmbedBuilder embed = GenerateEmbeddedHelpCommand(command);
                await ReplyAsync(embed: embed.Build());
            }
            else
            {
                await SendPagedHelpReplyAsync();
            }
        }

        [Command(HELP_COMMAND_DM)]
        [Alias(HELP_ALIAS_DM)]
        [Summary(HELP_SUMMARY_DM)]
        public async Task SendHelpDM(string command = null)
        {
            EmbedBuilder embed = CommandExists(command) ? GenerateEmbeddedHelpCommand(command) : GenerateEmbeddedHelp();

            var reply = ReplyAsync($"{Context.User.Mention}, se envió la Ayuda por mensaje privado.");
            var dm = Context.User.SendMessageAsync(embed: embed.Build());
            await Task.WhenAll(reply, dm);
        }

        #region Methods

        private EmbedBuilder GenerateEmbeddedHelp()
        {
            Emoji moduleBullet = new Emoji("\uD83D\uDD37");
            Emoji commandBullet = new Emoji("\uD83D\uDD39");
            string helpImageUrl = configuration.GetSection("images")?.GetSection("help")?["32"];
            string commandPrefix = configuration["commandPrefix"];

            List<ModuleInfo> modules = commands.Modules.Where(m => m.HasAttribute<HelpTitleAttribute>())
                                                       .OrderBy(m => (m.GetAttribute<HelpOrderAttribute>()?.Order))
                                                       .ToList();

            EmbedBuilder embed = new EmbedBuilder().WithColor(helpEmbedColor)
                                                   .WithTitle(Format.Bold("Comandos Disponibles"))
                                                   .WithDescription(GlobalConfiguration.Constants.BLANK_SPACE)
                                                   .WithThumbnailUrl(helpImageUrl);

            string helpCommands = new StringBuilder()
                                  .AppendLine($"{commandBullet} {Format.Code($"{commandPrefix}{HELP_COMMAND}")} | {Format.Code($"{commandPrefix}{HELP_ALIAS}")}: {Format.Italics(HELP_SUMMARY)}")
                                  .AppendLine($"{commandBullet} {Format.Code($"{commandPrefix}{HELP_COMMAND_DM}")} | {Format.Code($"{commandPrefix}{HELP_ALIAS_DM}")}: {Format.Italics(HELP_SUMMARY_DM)}")
                                  .AppendLine($"{commandBullet} {Format.Code($"{commandPrefix}{HELP_COMMAND}")} | {Format.Code($"{commandPrefix}{HELP_ALIAS}")} {Format.Code("<comando>")}: {Format.Italics(HELP_COMMAND_SUMMARY)}")
                                  .AppendLine($"{commandBullet} {Format.Code($"{commandPrefix}{HELP_COMMAND_DM}")} | {Format.Code($"{commandPrefix}{HELP_ALIAS_DM}")} {Format.Code("<comando>")}: {Format.Italics(HELP_COMMAND_SUMMARY_DM)}")
                                  .AppendLine(GlobalConfiguration.Constants.BLANK_SPACE)
                                  .ToString();
            embed.AddField(Format.Bold($"{moduleBullet} Ayuda"), helpCommands);

            foreach (ModuleInfo module in modules)
            {
                string moduleTitle = module.GetAttribute<HelpTitleAttribute>()?.Title;
                if (!string.IsNullOrWhiteSpace(moduleTitle))
                {
                    StringBuilder commandsBuilder = new StringBuilder();
                    foreach (CommandInfo commandInfo in module.Commands)
                    {
                        string commandSummary = Format.Italics(commandInfo.Summary);
                        string aliases = string.Join(" | ", commandInfo.Aliases.Select(a => Format.Code($"{commandPrefix}{a}")));
                        commandsBuilder.AppendLine($"{commandBullet} {aliases}: {commandSummary}");
                    }
                    string commands = commandsBuilder.AppendLine(GlobalConfiguration.Constants.BLANK_SPACE).ToString();
                    embed.AddField($"{moduleBullet} {Format.Bold(moduleTitle)}", commands);
                }
            }

            return embed;
        }

        private EmbedBuilder GenerateEmbeddedHelpCommand(string command)
        {
            string helpImageUrl = configuration.GetSection("images")?.GetSection("help")?["32"];
            string commandPrefix = configuration["commandPrefix"];
            string commandTitle = Format.Code($"{commandPrefix}{command}");

            List<ModuleInfo> modules = commands.Modules.Where(m => m.HasAttribute<HelpTitleAttribute>())
                                                       .OrderBy(m => m.GetAttribute<HelpOrderAttribute>()?.Order)
                                                       .ToList();

            CommandInfo commandInfo = commands.Commands.GetCommand(command);
            EmbedBuilder embed = new EmbedBuilder().WithTitle($"Comando {commandTitle}")
                                                   .WithColor(helpEmbedColor)
                                                   .WithDescription(GlobalConfiguration.Constants.BLANK_SPACE)
                                                   .WithThumbnailUrl(helpImageUrl)
                                                   .AddField(Format.Bold("Descripción"), Format.Italics(commandInfo.Summary));

            if (commandInfo.Parameters.Count > 0)
            {
                StringBuilder parameterBuilder = new StringBuilder();
                foreach (ParameterInfo parameter in commandInfo.Parameters)
                {
                    parameterBuilder.AppendLine($"{Format.Code($"<{parameter.Name}>")}: {Format.Italics(parameter.Summary)}");
                }
                string parameters = parameterBuilder.ToString();
                embed.AddField(Format.Bold("Parametros"), parameters);
            }
            else
            {
                embed.AddField(Format.Bold("Parametros"), Format.Italics("Ninguno."));
            }

            if (commandInfo.Aliases.Count > 1)
            {
                IEnumerable<string> otherAliases = commandInfo.Aliases.Where(a => !a.IsEquivalentTo(command));
                string aliases = string.Join(", ", otherAliases.Select(a => Format.Code($"{commandPrefix}{a}")));
                embed.AddField(Format.Bold("Otros Alias"), aliases);
            }

            return embed;
        }

        private async Task SendPagedHelpReplyAsync()
        {
            EmbedBuilder embed = GenerateEmbeddedHelp();
            List<PaginatedMessage.Page> pages = new List<PaginatedMessage.Page>();
            int pageCount = 0;
            foreach (EmbedFieldBuilder embedField in embed.Fields)
            {
                pages.Add(new PaginatedMessage.Page
                {
                    Description = embed.Description,
                    Title = embed.Title,
                    Fields = new List<EmbedFieldBuilder>() { embedField },
                    ImageUrl = embed.ImageUrl,
                    Color = embed.Color,
                    FooterOverride = new EmbedFooterBuilder
                    {
                        Text = $"Página {++pageCount} de {embed.Fields.Count}"
                    },
                    ThumbnailUrl = embed.ThumbnailUrl
                });
            }

            PaginatedMessage pager = new PaginatedMessage
            {
                Pages = pages,
                ThumbnailUrl = pages.First().ThumbnailUrl
            };
            ReactionList reactions = new ReactionList
            {
                Forward = true,
                Backward = true,
                First = true,
                Last = true,
                Info = false,
                Jump = false,
                Trash = false
            };
            await PagedReplyAsync(pager, reactions);
        }

        private bool CommandExists(string command)
        {
            return !string.IsNullOrWhiteSpace(command) && commands.Commands.Any(c => c.Aliases.Select(a => a.ToUpper().Trim()).Contains(command.ToUpper().Trim()) && !c.Module.Name.IsEquivalentTo(typeof(HelpModule).Name));
        }

        #endregion
    }
}