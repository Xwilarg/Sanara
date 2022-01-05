using Sanara.Help;

namespace Sanara.Module.Command
{
    public interface ISubmodule
    {
        public SubmoduleInfo GetInfo();
        public CommandInfo[] GetCommands();
    }
}
