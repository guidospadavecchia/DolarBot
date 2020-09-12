using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DolarBot.Util.Extensions
{
    public static class DiscordExtensions
    {
        public static bool HasAttribute<T>(this ModuleInfo module) where T : Attribute
        {
            return module.Attributes.Any(a => (a as T) != null);
        }

        public static T GetAttribute<T>(this ModuleInfo module) where T : Attribute
        {
            return module.Attributes.Where(a => (a as T) != null).Select(a => a as T).FirstOrDefault();
        }

        public static CommandInfo GetCommand(this IEnumerable<CommandInfo> commands, string command)
        {
            return commands.FirstOrDefault(c => c.Aliases.Select(a => a.ToUpper().Trim()).Contains(command.ToUpper().Trim()));
        }
    }
}
