namespace SanaraV3.Module.Administration
{
    public struct Help
    {
        public Help(string commandName, Argument[] arguments, string description, bool isNsfw)
        {
            CommandName = commandName;
            Arguments = arguments;
            Description = description;
            IsNsfw = isNsfw;
        }

        public string CommandName;
        public Argument[] Arguments;
        public string Description;
        public bool IsNsfw;
    }
}
