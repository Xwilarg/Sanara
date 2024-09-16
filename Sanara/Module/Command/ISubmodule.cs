namespace Sanara.Module.Command
{
    public interface ISubmodule
    {
        public string Name { get; }
        public string Description { get; }
        public CommandData[] GetCommands();
    }
}
