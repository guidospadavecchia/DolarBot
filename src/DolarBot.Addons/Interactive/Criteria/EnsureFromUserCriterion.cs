﻿// ReSharper disable StyleCop.SA1600
// ReSharper disable StyleCop.SA1614
namespace Discord.Addons.Interactive
{
    using System.Threading.Tasks;

    using Discord.Commands;
    using Discord.WebSocket;

    public class EnsureFromUserCriterion : ICriterion<SocketMessage>
    {
        private readonly ulong id;

        public EnsureFromUserCriterion(ulong id) => this.id = id;

        /// <summary>
        /// Ensures the user is the author
        /// </summary>
        /// <param name="sourceContext"></param>
        /// <param name="parameter"></param>
        /// <returns>
        /// True if user is author
        /// </returns>
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {
            bool ok = id == parameter.Author.Id;
            return Task.FromResult(ok);
        }
    }
}
