using Discord;
using Discord.Commands;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DolarBot.Util.Extensions
{
    public static class DiscordExtensions
    {
        /// <summary>
        /// Returns true if the current module contains the <typeparamref name="T"/> typed attribute, otherwise false.
        /// </summary>
        /// <typeparam name="T">The attribute's type.</typeparam>
        /// <param name="module">The current module.</param>
        /// <returns>True if the module has the attribute, otherwise false.</returns>
        public static bool HasAttribute<T>(this Discord.Commands.ModuleInfo module) where T : Attribute
        {
            return module.Attributes.Any(a => (a as T) != null);
        }

        /// <summary>
        /// Returns true if the current module contains the <typeparamref name="T"/> typed attribute, otherwise false.
        /// </summary>
        /// <typeparam name="T">The attribute's type.</typeparam>
        /// <param name="module">The current module.</param>
        /// <returns>True if the module has the attribute, otherwise false.</returns>
        public static bool HasAttribute<T>(this Discord.Interactions.ModuleInfo module) where T : Attribute
        {
            return module.Attributes.Any(a => (a as T) != null);
        }

        /// <summary>
        /// Returns true if the current command contains the <typeparamref name="T"/> typed attribute, otherwise false.
        /// </summary>
        /// <typeparam name="T">The attribute's type.</typeparam>
        /// <param name="command">The current command.</param>
        /// <returns>True if the module has the attribute, otherwise false.</returns>
        public static bool HasAttribute<T>(this CommandInfo command) where T : Attribute
        {
            return command.Attributes.Any(a => (a as T) != null);
        }

        /// <summary>
        /// Returns true if the current command contains the <typeparamref name="T"/> typed attribute, otherwise false.
        /// </summary>
        /// <typeparam name="T">The attribute's type.</typeparam>
        /// <param name="command">The current command.</param>
        /// <returns>True if the module has the attribute, otherwise false.</returns>
        public static bool HasAttribute<T, TParameter>(this CommandInfo<TParameter> command) where T : Attribute where TParameter : class, IParameterInfo
        {
            return command.Attributes.Any(a => (a as T) != null);
        }

        /// <summary>
        /// Gets the attribute, if found, from the current module.
        /// </summary>
        /// <typeparam name="T">The attribute's type.</typeparam>
        /// <param name="module">The current module.</param>
        /// <returns>The attribute if found, otherwise null.</returns>
        public static T GetAttribute<T>(this Discord.Commands.ModuleInfo module) where T : Attribute
        {
            return module.Attributes.Where(a => (a as T) != null).Select(a => a as T).FirstOrDefault();
        }

        /// <summary>
        /// Gets the attribute, if found, from the current module.
        /// </summary>
        /// <typeparam name="T">The attribute's type.</typeparam>
        /// <param name="module">The current module.</param>
        /// <returns>The attribute if found, otherwise null.</returns>
        public static T GetAttribute<T>(this Discord.Interactions.ModuleInfo module) where T : Attribute
        {
            return module.Attributes.Where(a => (a as T) != null).Select(a => a as T).FirstOrDefault();
        }

        /// <summary>
        /// Gets the attribute, if found, from the current command.
        /// </summary>
        /// <typeparam name="T">The attribute's type.</typeparam>
        /// <param name="command">>The current command.</param>
        /// <returns>The attribute if found, otherwise null.</returns>
        public static T GetAttribute<T>(this CommandInfo command) where T : Attribute
        {
            return command.Attributes.Where(a => (a as T) != null).Select(a => a as T).FirstOrDefault();
        }

        /// <summary>
        /// Gets the attribute, if found, from the current command.
        /// </summary>
        /// <typeparam name="T">The attribute's type.</typeparam>
        /// <param name="command">>The current command.</param>
        /// <returns>The attribute if found, otherwise null.</returns>
        public static T GetAttribute<T, TParameter>(this CommandInfo<TParameter> command) where T : Attribute where TParameter : class, IParameterInfo
        {
            return command.Attributes.Where(a => (a as T) != null).Select(a => a as T).FirstOrDefault();
        }

        /// <summary>
        /// Finds and returns a particular command from an <see cref="IEnumerable"/> commands collection.
        /// </summary>
        /// <param name="commands">The current collection of commands.</param>
        /// <param name="command">The command's name to find.</param>
        /// <returns>The <see cref="CommandInfo"/> object if found, otherwise null.</returns>
        public static CommandInfo GetCommand(this IEnumerable<CommandInfo> commands, string command)
        {
            return commands.FirstOrDefault(c => c.Aliases.Select(a => a.ToUpper().Trim()).Contains(command.ToUpper().Trim()));
        }

        /// <summary>
        /// Finds and returns a particular slash command from an <see cref="IEnumerable"/> slash commands collection.
        /// </summary>
        /// <param name="commands">The current collection of commands.</param>
        /// <param name="command">The command's name to find.</param>
        /// <returns>The <see cref="CommandInfo"/> object if found, otherwise null.</returns>
        public static SlashCommandInfo GetSlashCommand(this IEnumerable<SlashCommandInfo> commands, string command)
        {
            return commands.FirstOrDefault(c => c.Name.Equals(command, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Adds an inline field. This is a shortcut for <see cref="EmbedBuilder.AddField(string, object, bool)"/>.
        /// </summary>
        /// <param name="embedBuilder">The current <see cref="EmbedBuilder"/> object.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object with the added inline field.</returns>
        public static EmbedBuilder AddInlineField(this EmbedBuilder embedBuilder, string name, string value)
        {
            return embedBuilder.AddField(name, value, true);
        }

        /// <summary>
        /// Adds an empty inline field to the current <see cref="EmbedBuilder"/> object.
        /// </summary>
        /// <param name="embedBuilder">The current <see cref="EmbedBuilder"/> object.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object with an added empty inline field.</returns>
        public static EmbedBuilder AddInlineField(this EmbedBuilder embedBuilder)
        {
            return embedBuilder.AddInlineField(GlobalConfiguration.Constants.BLANK_SPACE, GlobalConfiguration.Constants.BLANK_SPACE);
        }

        /// <summary>
        /// Adds an empty line break to the current <see cref="EmbedBuilder"/> object.
        /// </summary>
        /// <param name="embedBuilder">The current <see cref="EmbedBuilder"/> object.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object with an added empty line.</returns>
        public static EmbedBuilder AddEmptyLine(this EmbedBuilder embedBuilder)
        {
            return embedBuilder.AddField(GlobalConfiguration.Constants.BLANK_SPACE, GlobalConfiguration.Constants.BLANK_SPACE);
        }

        /// <summary>
        /// Adds a new field with a link to share the specified <paramref name="shareText"/> on WhatsApp.
        /// </summary>
        /// <param name="embedBuilder">The current <see cref="EmbedBuilder"/> object.</param>
        /// <param name="shareEmoji">The emoji to use.</param>
        /// <param name="shareText">The text to share.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object with an added share field.</returns>
        public static async Task<EmbedBuilder> AddFieldWhatsAppShare(this EmbedBuilder embedBuilder, Emoji shareEmoji, string shareText, Func<string, Task<string>> shortenUrlFunction = null)
        {
            string encodedText = HttpUtility.UrlEncode($"{shareText}{Environment.NewLine}{Environment.NewLine}{"_Powered by DolarBot_"}");
            string url = $"https://api.whatsapp.com/send?text={encodedText}";

            if (shortenUrlFunction != null)
            {
                url = await shortenUrlFunction(url);
            }

            return embedBuilder.AddField(GlobalConfiguration.Constants.BLANK_SPACE, $"{shareEmoji} {Format.Url("Compartir", url)}".AppendLineBreak());
        }

        /// <summary>
        /// Adds a new field with a clickable link.
        /// </summary>
        /// <param name="embedBuilder">The current <see cref="EmbedBuilder"/> object.</param>
        /// <param name="emoji">The emoji to use.</param>
        /// <param name="title">The title for the embed field.</param>
        /// <param name="linkText">The text to show on the link.</param>
        /// <param name="url">The URL for the link.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object with an added link field.</returns>
        public static EmbedBuilder AddFieldLink(this EmbedBuilder embedBuilder, Emoji emoji, string title, string linkText, string url)
        {
            return embedBuilder.AddField(title, $"{emoji} {Format.Url(linkText, url)}".AppendLineBreak());
        }
    }
}
