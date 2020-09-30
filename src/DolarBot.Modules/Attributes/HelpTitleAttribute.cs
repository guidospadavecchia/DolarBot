using System;

namespace DolarBot.Modules.Attributes
{
    /// <summary>
    /// Indicates the module title to be shown inside the help command.
    /// </summary>
    public class HelpTitleAttribute : Attribute
    {
        /// <summary>
        /// The module title.
        /// </summary>
        public readonly string Title;

        /// <summary>
        /// Initializes the attribute with the defined title.
        /// </summary>
        /// <param name="title">The module title.</param>
        public HelpTitleAttribute(string title)
        {
            Title = title;
        }
    }
}
