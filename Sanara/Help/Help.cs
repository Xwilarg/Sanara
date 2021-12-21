namespace Sanara.Help
{
    public struct Help
    {
        public Help(string subModuleName, string commandName, Argument[] arguments, string description, string[] aliases, Restriction restriction, string example)
        {
            SubmoduleName = subModuleName;
            CommandName = commandName;
            Arguments = arguments;
            Description = description;
            Aliases = aliases;
            Restriction = restriction;
            Example = example;
        }

        public string SubmoduleName;
        public string CommandName;
        public Argument[] Arguments;
        public string Description;
        public string[] Aliases;
        public Restriction Restriction;
        public string Example;
    }
}
