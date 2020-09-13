using System;

namespace SanaraV3.Exception
{
    public sealed class CommandFailed : System.Exception
    {
        public CommandFailed(string reason) : base(reason)
        { }
    }
}
