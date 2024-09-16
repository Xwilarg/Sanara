using Sanara.Help.Data;
using System.Text.Json;
using Sanara.Command.SlashCommand.Submodule;

namespace Sanara.Help;

internal class Program
{
    static void Main(string[] args)
    {
        var submodules = Sanara.Program.Submodules;

        var res = submodules.Select(s => new Submodule()
        {
            Name = s.Name,
            Description = s.Description,
            Commands = s.GetCommands().Select(c => new Data.Command()
            {
                Name = c.SlashCommand.Name,
                Description = c.SlashCommand.Description,
                Restrictions = BuildPreconditions(c),
                Aliases = string.Join(", ", c.Aliases).ToLowerInvariant(),
                Arguments = c.SlashCommand.Options == null ? string.Empty : string.Join(" ", c.SlashCommand.Options.Select(x => x.IsRequired == true ? $"[{x.Name}]" : $"<{x.Name}>"))
            })
        });

        File.WriteAllText("help.json", JsonSerializer.Serialize(res));
    }

    private static string[] BuildPreconditions(CommandData c)
    {
        List<string> conditions = new();
        if (c.SlashCommand.IsNsfw)
        {
            conditions.Add("nsfw");
        }
        return conditions.ToArray();
    }
}
