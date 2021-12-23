namespace Sanara.Help
{
    public record SubmoduleInfo
    {
        public SubmoduleInfo(string name, Help[] help)
            => (Name, Help) = (name, help);

        public string Name { init; get; }
        public Help[] Help { init; get; }
    }
}
