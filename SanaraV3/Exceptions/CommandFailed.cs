using System;

namespace SanaraV3.Exceptions
{
    public sealed class CommandFailed : Exception
    {
        public CommandFailed(string reason) : base(reason)
        { }
    }
}
