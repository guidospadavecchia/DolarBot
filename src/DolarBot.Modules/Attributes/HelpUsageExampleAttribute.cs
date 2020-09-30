using System;

namespace DolarBot.Modules.Attributes
{
    /// <summary>
    /// Provides descriptions to be shown as examples for the module inside the help command.
    /// </summary>
    public class HelpUsageExampleAttribute : Attribute
    {
        /// <summary>
        /// Examples for the module.
        /// </summary>
        public readonly string[] Examples;
        /// <summary>
        /// Indicates if the sample texts are preformatted.
        /// </summary>
        public readonly bool IsPreformatted;

        /// <summary>
        /// Initializes the attribute with the defined examples.
        /// </summary>
        /// <param name="isPreformatted">Indicates if the sample texts are preformatted.</param>
        /// <param name="exampleText">Examples for the module.</param>
        public HelpUsageExampleAttribute(bool isPreformatted, params string[] exampleText)
        {
            Examples = exampleText;
            IsPreformatted = isPreformatted;
        }
    }
}
