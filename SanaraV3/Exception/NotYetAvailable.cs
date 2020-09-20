namespace SanaraV3.Exception
{
    public sealed class NotYetAvailable : System.Exception
    {
        public NotYetAvailable() : base("This feature was not yet restored on the V3 of Sanara, please retry later.")
        { }
    }
}
