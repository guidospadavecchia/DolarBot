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
        /// Color for the embed messages.
        /// </summary>
        private readonly Color helpEmbedColor = Color.Blue;

        /// <summary>
        /// Service which provides access to the available commands.
        /// </summary>
        private readonly CommandService Commands;

        /// <summary>
        /// The log4net logger.
        /// </summary>
        private readonly ILog Logger;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the specified <see cref="IConfiguration"/>, <see cref="ILog"/> and <see cref="CommandService"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="api">Provides access to the different APIs.</param>
        /// <param name="logger">The log4net logger.</param>
        public HelpModule(IConfiguration configuration, ILog logger, CommandService commands) : base(configuration)
        {
            Logger = logger;
            Commands = commands;
        }
        #endregion

        [Command(HELP_COMMAND, RunMode = RunMode.Async)]
        [Alias(HELP_ALIAS)]
        [Summary(HELP_SUMMARY)]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task SendHelp(string command = null)
        {
            try
            {
                if (CommandExists(command))
                {
                    EmbedBuilder embed = GenerateEmbeddedHelpCommand(command);
                    await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    await SendPagedHelpReplyAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        [Command(HELP_COMMAND_DM, RunMode = RunMode.Async)]
        [Alias(HELP_ALIAS_DM)]
        [Summary(HELP_SUMMARY_DM)]
        [RateLimit(1, 5, Measure.Seconds)]
        public async Task SendHelpDM(string command = null)
        {
            try
            {
                EmbedBuilder embed = CommandExists(command) ? GenerateEmbeddedHelpCommand(command) : GenerateEmbeddedHelp();

                var reply = ReplyAsync($"{Context.User.Mention}, se envió la Ayuda por mensaje privado.");
                var dm = Context.User.SendMessageAsync(embed: embed.Build());
                await Task.WhenAll(reply, dm).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ReplyAsync(GlobalConfiguration.GetGenericErrorMessage(Configuration["supportServerUrl"])).ConfigureAwait(false);
                Logger.Error("Error al ejecutar comando.", ex);
            }
        }

        #region Methods

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for help using reflection and attribute values.
        /// </summary>
        /// <returns>An <see cref="EmbedBuilder"/> object ready to be built.</returns>
        private EmbedBuilder GenerateEmbeddedHelp()
        {
            Emoji moduleBullet = new Emoji("\uD83D\uDD37");
            Emoji commandBullet = new Emoji("\uD83D\uDD39");
            string helpImageUrl = Configuration.GetSection("images").GetSection("help")["32"];
            string commandPrefix = Configuration["commandPrefix"];

            List<ModuleInfo> modules = Commands.Modules.Where(m => m.HasAttribute<HelpTitleAttribute>())
                                                       .OrderBy(m => (m.GetAttribute<HelpOrderAttribute>()?.Order))
                                                       .ToList();

            EmbedBuilder embed = new EmbedBuilder().WithColor(helpEmbedColor)
                                                   .WithTitle(Format.Bold("Comandos Disponibles"))
                                                   .WithDescription(GlobalConfiguration.Constants.BLANK_SPACE)
                                                   .WithThumbnailUrl(helpImageUrl);

            string helpCommands = new StringBuilder()
                                  .AppendLine(GlobalConfiguration.Constants.BLANK_SPACE)
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
            embed.AddField(Format.Bold($"{moduleBullet} Ayuda"), helpCommands);

            foreach (ModuleInfo module in modules)
            {
                string moduleTitle = module.GetAttribute<HelpTitleAttribute>()?.Title;
                if (!string.IsNullOrWhiteSpace(moduleTitle))
                {
                    StringBuilder commandsBuilder = new StringBuilder().AppendLine(GlobalConfiguration.Constants.BLANK_SPACE);
                    foreach (CommandInfo commandInfo in module.Commands)
                    {
                        string commandSummary = Format.Italics(commandInfo.Summary);
                        string aliases = string.Join(" | ", commandInfo.Aliases.Select(a => Format.Code($"{commandPrefix}{a}")));
                        commandsBuilder.AppendLine($"{commandBullet} {aliases}").AppendLine(commandSummary.AppendLineBreak());
                    }
                    embed.AddField($"{moduleBullet} {Format.Bold(moduleTitle)}", commandsBuilder.ToString());
                }
            }

            return embed;
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
            await PagedReplyAsync(pager, reactions).ConfigureAwait(false);
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