using Discord;
using Discord.Interactions;
using Fergun.Interactive.Pagination;
using Microsoft.Extensions.Configuration;
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
        public static T GetAttribute<T, TParameter>(this CommandInfo<TParameter> command) where T : Attribute where TParameter : class, IParameterInfo
        {
            return command.Attributes.Where(a => (a as T) != null).Select(a => a as T).FirstOrDefault();
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
        /// <param name="shortenUrlFunction">A function to shorten the URL.</param>
        /// <param name="inline">Indicates wether the field must be inline or not.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object with an added share field.</returns>
        public static async Task<EmbedBuilder> AddFieldWhatsAppShare(this EmbedBuilder embedBuilder, Emoji shareEmoji, string shareText, Func<string, Task<string>> shortenUrlFunction = null, bool inline = false)
        {
            string encodedText = HttpUtility.UrlEncode($"{shareText}{Environment.NewLine}{Environment.NewLine}{"_Powered by DolarBot_"}");
            string url = $"https://api.whatsapp.com/send?text={encodedText}";
            if (shortenUrlFunction != null)
            {
                url = await shortenUrlFunction(url);
            }
            string title = "WhatsApp";
            string description = $"{shareEmoji} {Format.Url("Compartir", url)}".AppendLineBreak();

            return inline ? embedBuilder.AddInlineField(title, description) : embedBuilder.AddField(title, description);
        }

        /// <summary>
        /// Adds a new field with a clickable link.
        /// </summary>
        /// <param name="embedBuilder">The current <see cref="EmbedBuilder"/> object.</param>
        /// <param name="emoji">The emoji to use.</param>
        /// <param name="title">The title for the embed field.</param>
        /// <param name="linkText">The text to show on the link.</param>
        /// <param name="url">The URL for the link.</param>
        /// <param name="inline">Indicates wether the field is inline or not.</param>
        /// <returns>An <see cref="EmbedBuilder"/> object with an added link field.</returns>
        public static EmbedBuilder AddFieldLink(this EmbedBuilder embedBuilder, Emoji emoji, string title, string linkText, string url, bool inline)
        {
            string value = $"{emoji} {Format.Url(linkText, url)}".AppendLineBreak();
            return inline ? embedBuilder.AddInlineField(title, value) : embedBuilder.AddField(title, value);
        }

        /// <summary>
        /// Transforms a collection of <see cref="EmbedBuilder"/> objects by building them into <see cref="Embed"/> objects.
        /// </summary>
        /// <param name="embedBuilderCollection">Collection of <see cref="EmbedBuilder"/>.</param>
        /// <returns>An array of <see cref="Embed"/>.</returns>
        public static Embed[] Build(this IEnumerable<EmbedBuilder> embedBuilderCollection)
        {
            return embedBuilderCollection.Select(x => x.Build()).ToArray();
        }

        /// <summary>
        /// Configures the paginator with the default buttons.
        /// </summary>
        /// <param name="builder">The paginator builder.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        public static StaticPaginatorBuilder WithDefaultButtons(this StaticPaginatorBuilder builder, IConfiguration configuration)
        {
            var emojis = configuration.GetSection("customEmojis");
            IEmote firstEmoji = Emote.Parse(emojis["firstPage"]);
            IEmote forwardEmoji = Emote.Parse(emojis["nextPage"]);
            IEmote jumpEmoji = Emote.Parse(emojis["jumpPage"]);
            IEmote backwardEmoji = Emote.Parse(emojis["previousPage"]);
            IEmote lastEmoji = Emote.Parse(emojis["lastPage"]);

            Dictionary<IEmote, PaginatorAction> actions = new()
            {
                { firstEmoji, PaginatorAction.SkipToStart },
                { backwardEmoji, PaginatorAction.Backward },
                { jumpEmoji, PaginatorAction.Jump },
                { forwardEmoji, PaginatorAction.Forward },
                { lastEmoji, PaginatorAction.SkipToEnd }
            };

            return builder.WithOptions(actions);
        }

        /// <summary>
        /// Appends the play store link as a field into <paramref name="embed"/>.
        /// </summary>
        /// <param name="embed">The embed to be modified.</param>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        /// <param name="inline">Indicates wether the field is inline or not.</param>
        /// <returns>The modified <see cref="EmbedBuilder"/>.</returns>
        public static EmbedBuilder AddPlayStoreLink(this EmbedBuilder embed, IConfiguration configuration, bool inline = false)
        {
            var emojis = configuration.GetSection("customEmojis");
            Emoji playStoreEmoji = new(emojis["playStore"]);
            string playStoreUrl = configuration["playStoreLink"];

            if (!string.IsNullOrWhiteSpace(playStoreUrl))
            {
                return embed.AddFieldLink(playStoreEmoji, "¡Descargá la app para Android!", "Google Play Store", playStoreUrl, inline);
            }
            else
            {
                return embed;
            }
        }
    }
}
