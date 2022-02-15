namespace Sanara.Module.Utility
{
    public class WholesomeList
    {
        public WholesomeListEntry entry { set; get; }
    }

    public class WholesomeListEntry
    {
        public string link { set; get; }
        public string title { set; get; }
        public string tier { set; get; }
        public string image { set; get; }
        public string[] tags { set; get; }
        public string note { set; get; }
        public string parody { set; get; }
    }
}
