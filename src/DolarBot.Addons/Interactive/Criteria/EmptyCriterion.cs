﻿// ReSharper disable StyleCop.SA1600

namespace Discord.Addons.Interactive.Criteria
{
    using Discord.Commands;
    using System.Threading.Tasks;

    public class EmptyCriterion<T> : ICriterion<T>
    {
        /// <summary>
        /// The judge async.
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
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, T parameter)
            => Task.FromResult(true);
    }
}
