// ReSharper disable StyleCop.SA1600

namespace Discord.Addons.Interactive.Paginator
{
    using Discord.Addons.Interactive.Criteria;
    using Discord.Commands;
    using Discord.WebSocket;
    using System.Threading.Tasks;

    public class EnsureIsIntegerCriterion : ICriterion<SocketMessage>
    {
        /// <summary>
        /// Ensures the input number is an integer
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
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {
            bool ok = int.TryParse(parameter.Content, out _);
            return Task.FromResult(ok);
        }
    }
}
