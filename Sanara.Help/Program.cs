using Microsoft.Extensions.DependencyInjection;
using Sanara.Game;
using Sanara.Help.Data;
using Sanara.Module.Command;
using System.Text.Json;

namespace Sanara.Help;

internal class Program
{
    static void Main(string[] args)
    {
        var submodules = Sanara.Program.Submodules;

        var coll = new ServiceCollection();
        coll.AddSingleton<GameManager>();
        var provider = coll.BuildServiceProvider();

        var res = submodules.Select(s => new Submodule()
        {
            Name = s.Name,
            Description = s.Description,
            Commands = s.GetCommands(provider).Select(c => new Data.Command()
            {
                Name = c.SlashCommand.Name,
                Description = c.SlashCommand.Description,
                Restrictions = BuildPreconditions(c),
                Aliases = string.Join(", ", c.Aliases).ToLowerInvariant(),
                Arguments = c.SlashCommand.Options == null ? string.Empty : string.Join(" ", c.SlashCommand.Options.Select(x => x.IsRequired == true ? $"[{x.Name}]" : $"<{x.Name}>"))
            })
        });

        File.WriteAllText("Help.json", JsonSerializer.Serialize(res));
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
