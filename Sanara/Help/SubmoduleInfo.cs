namespace Sanara.Help
{
    public record SubmoduleInfo
    {
        public SubmoduleInfo(string name, string description)
            => (Name, Description) = (name, description);

        public string Name { private init; get; }
        public string Description{ private init; get; }
    }
}
