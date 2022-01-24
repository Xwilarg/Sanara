namespace Sanara.Exception
{
    public sealed class CommandFailed : System.Exception
    {
        public CommandFailed(string reason, bool ephemeral = false) : base(reason)
        {
            Ephemeral = ephemeral;
        }

        public bool Ephemeral { private init; get; }
    }
}
