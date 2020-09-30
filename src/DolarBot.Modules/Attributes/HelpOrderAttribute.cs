using System;

namespace DolarBot.Modules.Attributes
{
    /// <summary>
    /// Indicates the order in which the module must be shown within the help command.
    /// </summary>
    public class HelpOrderAttribute : Attribute
    {
        /// <summary>
        /// Order of the module and it's commands inside the Help command.
        /// </summary>
        public readonly int Order;

        /// <summary>
        /// Initializes the attribute with the defined order.
        /// </summary>
        /// <param name="order">The module's order.</param>
        public HelpOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
