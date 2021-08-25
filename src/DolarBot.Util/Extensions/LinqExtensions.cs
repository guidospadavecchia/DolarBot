using System.Collections.Generic;
using System.Linq;

namespace DolarBot.Util.Extensions
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Splits the current collection into <paramref name="chunkSize"/> pages.
        /// </summary>
        /// <typeparam name="T">Type of the object contained in the collection.</typeparam>
        /// <param name="source">The current collection.</param>
        /// <param name="chunkSize">The size of each chunk.</param>
        /// <returns>A collection of <paramref name="chunkSize"/> collections of <typeparamref name="T"/> objects.</returns>
        public static List<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value))
                .ToList();
        }
    }
}
