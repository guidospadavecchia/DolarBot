using System;

namespace DolarBot.Modules.Attributes
{
    public class HelpOrderAttribute : Attribute
    {
        public readonly int Order;

        public HelpOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
