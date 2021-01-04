using System;

namespace DolarBot.API.Attributes
{
    /// <summary>
    /// Indicates the cryptocurrency abbreviated code.
    /// </summary>
    public class CryptoCurrencyCodeAttribute : Attribute
    {
        /// <summary>
        /// The abbreviated cryptocurrency code.
        /// </summary>
        public readonly string Code;

        /// <summary>
        /// Initializes the attribute with the defined code.
        /// </summary>
        /// <param name="order">The cryptocurrency code.</param>
        public CryptoCurrencyCodeAttribute(string code)
        {
            Code = code;
        }
    }
}
