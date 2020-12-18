using Newtonsoft.Json;

namespace DolarBot.Util.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Creates a deep copy of the current object. Object must be serializable.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="instance">The instance of the object.</param>
        /// <returns>A copy of the current object instance.</returns>
        public static T DeepClone<T>(this T instance) where T : class
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(instance));
        }
    }
}
