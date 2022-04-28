﻿// ReSharper disable StyleCop.SA1600
namespace Discord.Addons.Interactive
{
    using Discord.Commands;
    using Discord.WebSocket;
    using System.Threading.Tasks;

    public class EnsureReactionFromSourceUserCriterion : ICriterion<SocketReaction>
    {
        /// <summary>
        /// Returns true if the user is the source user.
        /// </summary>
        /// <param name="sourceContext">
        /// The source context.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketReaction parameter)
        {
            bool ok = parameter.UserId == sourceContext.User.Id;
            return Task.FromResult(ok);
        }
    }
}
