using System;

namespace SanaraV3.Exceptions
{
    public sealed class InvalidGameAnswer : Exception
    {
        public InvalidGameAnswer(string message) : base(message)
        { }
    }
}
