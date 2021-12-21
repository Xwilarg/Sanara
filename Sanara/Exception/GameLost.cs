namespace Sanara.Exception
{
    public sealed class GameLost : System.Exception
    {
        public GameLost(string msg) : base(msg)
        { }
    }
}
