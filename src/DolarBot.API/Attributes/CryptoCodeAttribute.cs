using System;

namespace DolarBot.API.Attributes
{
    /// <summary>
    /// Indicates the cryptocurrency code.
    /// </summary>
    public class CryptoCodeAttribute : Attribute
    {
        /// <summary>
        /// The cryptocurrency code.
        /// </summary>
        public readonly string Code;
        /// <summary>
        /// The cryptocurrency symbol.
        /// </summary>
        public readonly string Symbol;

        /// <summary>
        /// Initializes the attribute with the defined code.
        /// </summary>
        /// <param name="code">The cryptocurrency code.</param>
        /// <param name="symbol">The cryptocurrency symbol.</param>
        public CryptoCodeAttribute(string code, string symbol)
        {
            Code = code;
            Symbol = symbol;
        }
    }
}
