﻿// ReSharper disable StyleCop.SA1600
namespace Discord.Addons.Interactive
{
    using System.Threading.Tasks;

    using Discord.Commands;

    public interface ICriterion<in T>
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
        Task<bool> JudgeAsync(SocketCommandContext sourceContext, T parameter);
    }
}
