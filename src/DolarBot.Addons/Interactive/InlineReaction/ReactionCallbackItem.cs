﻿namespace Discord.Addons.Interactive.InlineReaction
{
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// The reaction callback item.
    /// </summary>
    public class ReactionCallbackItem
    {
        /// <summary>
        /// Gets the reaction.
        /// </summary>
        public IEmote Reaction { get; }

        /// <summary>
        /// Gets the callback.
        /// </summary>
        public Func<SocketCommandContext, SocketReaction, Task> Callback { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactionCallbackItem"/> class.
        /// </summary>
        /// <param name="reaction">
        /// The reaction.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        public ReactionCallbackItem(IEmote reaction, Func<SocketCommandContext, SocketReaction, Task> callback)
        {
            Reaction = reaction;
            Callback = callback;
        }
    }
}