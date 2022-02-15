using Sanara.Module.Command;

namespace Sanara.Help
{
    public class HelpPreload
    {
        public HelpPreload(List<ISubmodule> submodules)
        {
            Data = submodules.Select(s => new Serialization.Submodule()
            {
                Name = s.GetInfo().Name,
                Description = s.GetInfo().Description,
                Commands = s.GetCommands().Select(c => new Serialization.Command()
                {
                    Name = c.SlashCommand.Name.Value,
                    Description = c.SlashCommand.Description.Value,
                    Restrictions = c.Precondition.ToString(),
                    Aliases = string.Join(", ", c.Aliases).ToLowerInvariant(),
                    Arguments = !c.SlashCommand.Options.IsSpecified ? "" : string.Join(" ", c.SlashCommand.Options.Value.Select(x => x.IsRequired == true ? $"[{x.Name}]" : $"<{x.Name}>"))
                }).ToArray()
            }).ToArray();
        }

        public Serialization.Submodule[] Data { private set; get; }
    }
}
