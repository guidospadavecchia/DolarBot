using System;

namespace DolarBot.Modules.Attributes
{
    public class HelpTitleAttribute : Attribute
    {
        public readonly string Title;

        public HelpTitleAttribute(string title)
        {
            Title = title;
        }
    }
}
