using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DolarBot.Modules.Attributes;
using DolarBot.Modules.InteractiveCommands.Base;
using DolarBot.Util;
using DolarBot.Util.Extensions;
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
        private const string HELP_SUMMARY = "Muestra ayuda acerca de los comandos disponibles.";
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
        public HelpInteractiveModule(IConfiguration configuration, ILog logger, InteractionService interactionService) : base(configuration, logger)
        {
            InteractionService = interactionService;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Initializes the Help module registering the slash command with dynamically generated choices.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> instance.</param>
        /// <param name="interactionService">The <see cref="InteractionService"/> instance.</param>
        /// <param name="configuration">Provides access to application settings.</param>
        /// <param name="isDebug">Indicates wether the current instance is running in a debug state.</param>
        public static void RegisterSlashCommand(DiscordSocketClient client, InteractionService interactionService, IConfiguration configuration, bool isDebug)
        {
            SlashCommandOptionBuilder slashCommandOption = new SlashCommandOptionBuilder()
                                                           .WithName("comando")
                                                           .WithDescription("Comando a detallar.")
                                                           .WithRequired(false)
                                                           .WithType(ApplicationCommandOptionType.String);
            List<SlashCommandInfo> slashCommands = interactionService.SlashCommands.Where(x => !x.Name.Equals(HELP_COMMAND, StringComparison.OrdinalIgnoreCase)).ToList();
            for (int i = 1; i < slashCommands.Count; i++)
            {
                SlashCommandInfo slashCommand = slashCommands.ElementAt(i);
                slashCommandOption.AddChoice($"/{slashCommand.Name}", slashCommand.Name);
            }
            SlashCommandBuilder helpCommand = new SlashCommandBuilder()
                                              .WithName(HELP_COMMAND)
                                              .WithDescription(HELP_SUMMARY)
                                              .AddOption(slashCommandOption);

            if (isDebug)
            {
                bool testGuildConfigured = ulong.TryParse(configuration["testServerId"], out ulong testServerId);
                if (testGuildConfigured)
                {
                    client.Rest.CreateGuildCommand(helpCommand.Build(), testServerId);
                }
            }
            else
            {
                client.Rest.CreateGlobalCommand(helpCommand.Build());
            }
        }

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
        public async Task SendSlashCommandsHelp([Summary(description: "Comando a detallar.")] string comando = null)
        {
            await DeferAsync(true).ContinueWith(async (task) =>
            {
                try
                {
                    if (SlashCommandExists(comando))
                    {
                        EmbedBuilder embed = GenerateEmbeddedSlashCommandHelp(comando);
                        await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embed = embed.Build());
                    }
                    else
                    {
                        List<EmbedBuilder> embeds = GenerateEmbeddedSlashCommandsHelp();
                        await Context.Interaction.ModifyOriginalResponseAsync((MessageProperties messageProperties) => messageProperties.Embeds = new Optional<Embed[]>(embeds.Select(e => e.Build()).ToArray()));
                    }
                }
                catch (Exception ex)
                {
                    await SendDeferredErrorResponse(Context.Interaction, ex);
                }
            });
        }
    }
}
