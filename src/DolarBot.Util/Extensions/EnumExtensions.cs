using System;
using System.ComponentModel;
using System.Reflection;

namespace DolarBot.Util.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieves the current enum's description and, if found, returns it.
        /// </summary>
        /// <typeparam name="T">The enum's type.</typeparam>
        /// <param name="source">The current enum's value.</param>
        /// <returns>The enum's description if found, otherwise null.</returns>
        public static string GetDescription<T>(this T source) where T : Enum
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi?.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return source.ToString();
            }
        }

        /// <summary>
        /// Retrieves the current enum's specified attribute and, if found, returns it.
        /// </summary>
        /// <typeparam name="T">The attribute's type.</typeparam>
        /// <param name="source">The current enum's value.</param>
        /// <returns>The enum's attribute if found, otherwise null.</returns>
        public static T GetAttribute<T>(this Enum source) where T : Attribute
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());
            return fi.GetCustomAttribute<T>(false);
        }
    }
}
