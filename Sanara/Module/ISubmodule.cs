using Sanara.Help;

namespace Sanara.Module
{
    public interface ISubmodule
    {
        public SubmoduleInfo GetInfo();
        public CommandInfo[] GetCommands();
    }
}
