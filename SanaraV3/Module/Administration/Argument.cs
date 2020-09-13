namespace SanaraV3.Module.Administration
{
    public struct Argument
    {
        public Argument(ArgumentType type, string content)
        {
            Type = type;
            Content = content;
        }

        public ArgumentType Type;
        public string Content;
    }
}
