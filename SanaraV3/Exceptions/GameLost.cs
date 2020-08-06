using System;

namespace SanaraV3.Exceptions
{
    public sealed class GameLost : Exception
    {
        public GameLost(string msg) : base(msg)
        { }
    }
}
