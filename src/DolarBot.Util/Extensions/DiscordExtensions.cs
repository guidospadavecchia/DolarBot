using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static bool HasAttribute<T>(this ModuleInfo module) where T : Attribute
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
        /// Gets the attribute, if found, from the current module.
        /// </summary>
        /// <typeparam name="T">The attribute's type.</typeparam>
        /// <param name="module">The current module.</param>
        /// <returns>The attribute if found, otherwise null.</returns>
        public static T GetAttribute<T>(this ModuleInfo module) where T : Attribute
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
    }
}
