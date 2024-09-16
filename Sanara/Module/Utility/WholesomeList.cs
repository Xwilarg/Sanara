namespace Sanara.Module.Utility;

public class WholesomeList
{
    public WholesomeListEntry Entry { set; get; }
}

public class WholesomeListEntry
{
    public string Title { set; get; }
    public string Tier { set; get; }
    public string Image { set; get; }
    public string[] Tags { set; get; }
    public string Note { set; get; }
    public string Parody { set; get; }
    public string Uuid { set; get; }
    public string EH { set; get; }
    public int Pages { set; get; }
}
