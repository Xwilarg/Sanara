using Sanara.Module;

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
                    Restrictions = c.Precondition.ToString()
                }).ToArray()
            }).ToArray();
        }

        public Serialization.Submodule[] Data { private set; get; }
    }
}
