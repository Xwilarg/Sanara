namespace Sanara.Exception
{
    public sealed class CommandFailed : System.Exception
    {
        public CommandFailed(string reason) : base(reason)
        { }
    }
}
