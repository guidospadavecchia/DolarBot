// ReSharper disable StyleCop.SA1600

namespace Discord.Addons.Interactive.Callbacks
{
    using Discord.Addons.Interactive.Criteria;
    using Discord.Commands;
    using Discord.WebSocket;
    using System;
    using System.Threading.Tasks;

    public interface IReactionCallback
    {
        RunMode RunMode { get; }

        ICriterion<SocketReaction> Criterion { get; }

        TimeSpan? Timeout { get; }

        SocketCommandContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}
