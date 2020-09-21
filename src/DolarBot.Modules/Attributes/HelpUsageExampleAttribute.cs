using System;

namespace DolarBot.Modules.Attributes
{
    public class HelpUsageExampleAttribute : Attribute
    {
        public readonly string[] Examples;
        public readonly bool IsPreformatted;

        public HelpUsageExampleAttribute(bool isPreformatted, params string[] exampleText)
        {
            Examples = exampleText;
            IsPreformatted = isPreformatted;
        }
    }
}
