using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.Commands.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParameterInfo = Discord.Commands.ParameterInfo;
using EmbedPage = Discord.Addons.Interactive.PaginatedMessage.Page;

namespace DolarBot.Modules.Commands
{
    /// <summary>
    /// Contains help related commands.
    /// </summary>
    public class HelpModule : BaseInteractiveModule
    {
        #region Constants
        private const string HELP_COMMAND = "ayuda";
        private const string HELP_ALIAS = "a";
        private const string HELP_COMMAND_DM = "ayudadm";
        private const string HELP_ALIAS_DM = "adm";
        private const string HELP_SUMMARY = "Muestra los comandos disponibles.";
        private const string HELP_COMMAND_SUMMARY = "Muestra información sobre un comando.";
        private const string HELP_SUMMARY_DM = "Envía la ayuda por mensaje privado.";
        private const string HELP_COMMAND_SUMMARY_DM = "Envía la ayuda del comando por mensaje privado.";
        #endregion

        #region Vars
        /// <summary>
        /// Service which provides access to the available commands.
        /// </summary>
        private readonly CommandService Commands;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the specified <see cref="IConfiguration"/>, <see cref="ILog"/> and <see cref="CommandService"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public HelpModule(IConfiguration configuration, ILog logger, CommandService commands) : base(configuration, logger)
        {
            Commands = commands;
        }
        #endregion

        [Command(HELP_COMMAND, RunMode = RunMode.Async)]
        [Alias(HELP_ALIAS)]
        [Summary(HELP_SUMMARY)]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task SendHelp(string command = null)
        {
            try
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
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        [Command(HELP_COMMAND_DM, RunMode = RunMode.Async)]
        [Alias(HELP_ALIAS_DM)]
        [Summary(HELP_SUMMARY_DM)]
        [RateLimit(1, 3, Measure.Seconds)]
        public async Task SendHelpDM(string command = null)
        {
            try
            {
                List<EmbedBuilder> embeds = CommandExists(command) ? new List<EmbedBuilder>() { GenerateEmbeddedHelpCommand(command) } : GenerateEmbeddedHelp();
                await ReplyAsync($"{Context.User.Mention}, se envió la ayuda por mensaje privado.");
                foreach (var embed in embeds)
                {
                    await Context.User.SendMessageAsync(embed: embed.Build());
                }
            }
            catch (Exception ex)
            {
                await SendErrorReply(ex);
            }
        }

        #region Methods

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for help using reflection and attribute values.
        /// </summary>
        /// <returns>A collection of <see cref="EmbedBuilder"/> objects ready to be built.</returns>
        private List<EmbedBuilder> GenerateEmbeddedHelp()
        {
            List<EmbedBuilder> embeds = new List<EmbedBuilder>();

            Emoji moduleBullet = new Emoji("\uD83D\uDD37");
            Emoji commandBullet = new Emoji("\uD83D\uDD39");
            string helpImageUrl = Configuration.GetSection("images").GetSection("help")["64"];
            string commandPrefix = Configuration["commandPrefix"];

            EmbedBuilder helpEmbed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Help)
                                                       .WithTitle(Format.Bold("Ayuda"))
                                                       .WithThumbnailUrl(helpImageUrl)
                                                       .WithCurrentTimestamp();

            string helpCommands = new StringBuilder()
                                  .AppendLine($"{commandBullet} {Format.Code($"{commandPrefix}{HELP_COMMAND}")} | {Format.Code($"{commandPrefix}{HELP_ALIAS}")}")
                                  .AppendLine(Format.Italics(HELP_SUMMARY))
                                  .AppendLine(GlobalConfiguration.Constants.BLANK_SPACE)
                                  .AppendLine($"{commandBullet} {Format.Code($"{commandPrefix}{HELP_COMMAND_DM}")} | {Format.Code($"{commandPrefix}{HELP_ALIAS_DM}")}")
                                  .AppendLine(Format.Italics(HELP_SUMMARY_DM))
                                  .AppendLine(GlobalConfiguration.Constants.BLANK_SPACE)
                                  .AppendLine($"{commandBullet} {Format.Code($"{commandPrefix}{HELP_COMMAND}")} | {Format.Code($"{commandPrefix}{HELP_ALIAS}")} {Format.Code("<comando>")}")
                                  .AppendLine(Format.Italics(HELP_COMMAND_SUMMARY))
                                  .AppendLine(GlobalConfiguration.Constants.BLANK_SPACE)
                                  .AppendLine($"{commandBullet} {Format.Code($"{commandPrefix}{HELP_COMMAND_DM}")} | {Format.Code($"{commandPrefix}{HELP_ALIAS_DM}")} {Format.Code("<comando>")}")
                                  .AppendLine(Format.Italics(HELP_COMMAND_SUMMARY_DM))
                                  .AppendLine(GlobalConfiguration.Constants.BLANK_SPACE)
                                  .ToString();
            helpEmbed.AddField(GlobalConfiguration.Constants.BLANK_SPACE, helpCommands);
            embeds.Add(helpEmbed);

            Dictionary<string, List<ModuleInfo>> modules = Commands.Modules.Where(m => m.HasAttribute<HelpTitleAttribute>())
                                                               .OrderBy(m => (m.GetAttribute<HelpOrderAttribute>()?.Order))
                                                               .GroupBy(m => m.GetAttribute<HelpTitleAttribute>()?.Title)
                                                               .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                                                               .ToDictionary(x => x.Key, x => x.ToList());
            foreach (var module in modules)
            {
                for (int i = 0; i < module.Value.Count; i++)
                {
                    EmbedBuilder embed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Help)
                                        .WithTitle(module.Value.Count > 1 ? $"{Format.Bold(module.Key)} ({i + 1}/{module.Value.Count})" : Format.Bold(module.Key))
                                        .WithThumbnailUrl(helpImageUrl)
                                        .WithCurrentTimestamp();

                    ModuleInfo m = module.Value.ElementAt(i);
                    StringBuilder commandsBuilder = new StringBuilder();
                    foreach (CommandInfo commandInfo in m.Commands)
                    {
                        string commandSummary = Format.Italics(commandInfo.Summary);
                        string aliases = string.Join(" | ", commandInfo.Aliases.Select(a => Format.Code($"{commandPrefix}{a}")));
                        commandsBuilder.AppendLine($"{commandBullet} {aliases}").AppendLine(commandSummary.AppendLineBreak());
                    }

                    embed.AddField(GlobalConfiguration.Constants.BLANK_SPACE, commandsBuilder.ToString());
                    embeds.Add(embed);
                }
            }

            return embeds;
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for help for a particular command using reflection and attribute values.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private EmbedBuilder GenerateEmbeddedHelpCommand(string command)
        {
            string helpImageUrl = Configuration.GetSection("images").GetSection("help")["64"];
            string commandPrefix = Configuration["commandPrefix"];
            string commandTitle = Format.Code($"{commandPrefix}{command}");

            List<ModuleInfo> modules = Commands.Modules.Where(m => m.HasAttribute<HelpTitleAttribute>())
                                                       .OrderBy(m => m.GetAttribute<HelpOrderAttribute>()?.Order)
                                                       .ToList();

            CommandInfo commandInfo = Commands.Commands.GetCommand(command);
            EmbedBuilder embed = new EmbedBuilder().WithTitle($"Comando {commandTitle}")
                                                   .WithColor(GlobalConfiguration.Colors.Help)
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

            if (commandInfo.HasAttribute<HelpUsageExampleAttribute>())
            {
                HelpUsageExampleAttribute attribute = commandInfo.GetAttribute<HelpUsageExampleAttribute>();
                IEnumerable<string> examples = attribute.Examples;
                StringBuilder exampleBuilder = new StringBuilder();
                foreach (string example in examples)
                {
                    string exampleText = attribute.IsPreformatted ? example : Format.Code(example);
                    exampleBuilder.AppendLine(exampleText);
                }
                embed.AddField(Format.Bold("Ejemplos"), exampleBuilder.ToString());
            }

            return embed;
        }

        /// <summary>
        /// Creates and sends a paginated message as reply, containing the full description about all commands.
        /// </summary>
        /// <returns>A completed task.</returns>
        private async Task SendPagedHelpReplyAsync()
        {
            List<EmbedBuilder> embeds = GenerateEmbeddedHelp();
            List<EmbedPage> pages = new List<EmbedPage>();
            int pageCount = 0;
            int totalPages = embeds.Select(x => x.Fields.Count).Sum();

            foreach (EmbedBuilder embed in embeds)
            {
                foreach (EmbedFieldBuilder embedField in embed.Fields)
                {
                    pages.Add(new EmbedPage
                    {
                        Description = embed.Description,
                        Title = embed.Title,
                        Fields = new List<EmbedFieldBuilder>() { embedField },
                        ImageUrl = embed.ImageUrl,
                        Color = embed.Color,
                        FooterOverride = new EmbedFooterBuilder
                        {
                            Text = $"Página {++pageCount} de {totalPages}"
                        },
                        ThumbnailUrl = embed.ThumbnailUrl
                    });
                }
            }

            await SendPagedReplyAsync(pages);
        }

        /// <summary>
        /// Checks whether a command exists in any module.
        /// </summary>
        /// <param name="command">The command to check.</param>
        /// <returns>True if the command exists, otherwise false.</returns>
        private bool CommandExists(string command)
        {
            return !string.IsNullOrWhiteSpace(command) && Commands.Commands.Any(c => c.Aliases.Select(a => a.ToUpper().Trim()).Contains(command.ToUpper().Trim()) && !c.Module.Name.IsEquivalentTo(typeof(HelpModule).Name));
        }

        #endregion
    }
}