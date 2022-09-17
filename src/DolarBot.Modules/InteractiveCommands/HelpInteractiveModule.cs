using Discord;
using Discord.Interactions;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Autocompletion.Help;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
using Fergun.Interactive;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModuleInfo = Discord.Interactions.ModuleInfo;
using RunMode = Discord.Interactions.RunMode;
using SummaryAttribute = Discord.Interactions.SummaryAttribute;

namespace DolarBot.Modules.InteractiveCommands
{
    /// <summary>
    /// Contains help related commands.
    /// </summary>
    public class HelpInteractiveModule : BaseInteractiveModule
    {
        #region Constants
        private const string HELP_COMMAND = "ayuda";
        private const string HELP_SUMMARY = "Muestra los comandos disponibles.";
        private const string HELP_COMMAND_SUMMARY = "Muestra información sobre un comando.";
        #endregion

        #region Vars
        /// <summary>
        /// Service which provides access to the available commands.
        /// </summary>
        private readonly InteractionService InteractionService;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates the module using the specified <see cref="IConfiguration"/>, <see cref="ILog"/> and <see cref="InteractionService"/> objects.
        /// </summary>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="logger">The log4net logger.</param>
        /// <param name="interactionService">Provides the framework for building and registering Discord Application Commands.</param>
        /// <param name="interactiveService">The interactive service.</param>
        public HelpInteractiveModule(IConfiguration configuration, ILog logger, InteractionService interactionService, InteractiveService interactiveService) : base(configuration, logger, interactiveService)
        {
            InteractionService = interactionService;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Checks whether a command exists in any module.
        /// </summary>
        /// <param name="command">The command to check.</param>
        /// <returns>True if the command exists, otherwise false.</returns>
        private bool SlashCommandExists(string command)
        {
            return !string.IsNullOrWhiteSpace(command) && InteractionService.SlashCommands.Any(c => c.Name.Equals(command, StringComparison.OrdinalIgnoreCase) && !c.Module.Name.IsEquivalentTo(typeof(HelpInteractiveModule).Name));
        }

        /// <summary>
        /// Creates an <see cref="EmbedBuilder"/> object for help regarding slash commands, using reflection and attribute values.
        /// </summary>
        /// <returns>A collection of <see cref="EmbedBuilder"/> objects ready to be built.</returns>
        private List<EmbedBuilder> GenerateEmbeddedSlashCommandsHelp()
        {
            List<EmbedBuilder> embeds = new();

            Emoji moduleBullet = new("\uD83D\uDD37");
            Emoji commandBullet = new("\uD83D\uDD39");
            string helpImageUrl = Configuration.GetSection("images").GetSection("help")["64"];

            EmbedBuilder helpEmbed = new EmbedBuilder().WithColor(GlobalConfiguration.Colors.Help)
                                                       .WithTitle(Format.Bold("Ayuda"))
                                                       .WithThumbnailUrl(helpImageUrl)
                                                       .WithCurrentTimestamp();

            string helpCommands = new StringBuilder()
                                  .AppendLine($"{commandBullet} {Format.Code($"/{HELP_COMMAND}")}")
                                  .AppendLine(Format.Italics(HELP_SUMMARY))
                                  .AppendLine(GlobalConfiguration.Constants.BLANK_SPACE)
                                  .AppendLine($"{commandBullet} {Format.Code($"/{HELP_COMMAND}")} {Format.Code("<comando>")}")
                                  .AppendLine(Format.Italics(HELP_COMMAND_SUMMARY))
                                  .AppendLine(GlobalConfiguration.Constants.BLANK_SPACE)
                                  .ToString();
            helpEmbed.AddField(GlobalConfiguration.Constants.BLANK_SPACE, helpCommands);
            embeds.Add(helpEmbed);

            Dictionary<string, List<ModuleInfo>> modules = InteractionService.Modules.Where(m => m.HasAttribute<HelpTitleAttribute>())
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
                    StringBuilder commandsBuilder = new();
                    foreach (SlashCommandInfo slashCommandInfo in m.SlashCommands)
                    {
                        string commandName = Format.Code($"/{slashCommandInfo.Name}");
                        string commandDescription = Format.Italics(slashCommandInfo.Description).AppendLineBreak();
                        commandsBuilder.AppendLine($"{commandBullet} {commandName}").AppendLine(commandDescription);
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
        /// <param name="command">The command to describe.</param>
        /// <returns>An <see cref="EmbedBuilder"/> ready to be built.</returns>
        private EmbedBuilder GenerateEmbeddedSlashCommandHelp(string command)
        {
            string helpImageUrl = Configuration.GetSection("images").GetSection("help")["64"];
            string commandTitle = Format.Code($"/{command}");

            List<ModuleInfo> modules = InteractionService.Modules.Where(m => m.HasAttribute<HelpTitleAttribute>())
                                                       .OrderBy(m => m.GetAttribute<HelpOrderAttribute>()?.Order)
                                                       .ToList();

            SlashCommandInfo slashCommandInfo = InteractionService.SlashCommands.GetSlashCommand(command);
            EmbedBuilder embed = new EmbedBuilder().WithTitle($"Comando {commandTitle}")
                                                   .WithColor(GlobalConfiguration.Colors.Help)
                                                   .WithDescription(GlobalConfiguration.Constants.BLANK_SPACE)
                                                   .WithThumbnailUrl(helpImageUrl)
                                                   .AddField(Format.Bold("Descripción"), Format.Italics(slashCommandInfo.Description));

            if (slashCommandInfo.Parameters.Count > 0)
            {
                StringBuilder parameterBuilder = new();
                foreach (SlashCommandParameterInfo parameter in slashCommandInfo.Parameters)
                {
                    parameterBuilder.AppendLine($"{Format.Code($"<{parameter.Name}>")}: {(parameter.IsRequired ? " " : $"{Format.Bold("[Opcional]")} ")}{Format.Italics(parameter.Description)}");
                }
                embed.AddField(Format.Bold("Parámetros"), parameterBuilder.ToString());
            }
            else
            {
                embed.AddField(Format.Bold("Parámetros"), Format.Italics("Ninguno."));
            }

            return embed;
        }

        #endregion

        [SlashCommand(HELP_COMMAND, HELP_SUMMARY, false, RunMode.Async)]
        public async Task SendSlashCommandsHelp(
            [Autocomplete(typeof(SlashCommandAutocompleteHandler))]
            [Summary(description: "Comando a detallar.")]
            string comando = null
        )
        {
            await DeferAsync().ContinueWith(async (task) =>
            {
                try
                {
                    if (SlashCommandExists(comando))
                    {
                        EmbedBuilder embed = GenerateEmbeddedSlashCommandHelp(comando);
                        await FollowupAsync(embed: embed.Build());
                    }
                    else
                    {
                        EmbedBuilder[] embedbuilders = GenerateEmbeddedSlashCommandsHelp().ToArray();
                        await FollowUpWithPaginatedEmbedAsync(embedbuilders);
                    }
                }
                catch (Exception ex)
                {
                    await FollowUpWithErrorResponseAsync(ex);
                }
            });
        }
    }
}
