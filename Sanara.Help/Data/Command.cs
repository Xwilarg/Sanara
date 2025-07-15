namespace Sanara.Help.Data;

public class Command
{
    public string Name { set; get; }
    public string Arguments { set; get; }
    public string Description { set; get; }
    public string[] Restrictions { set; get; }
    public string Aliases { set; get; }
}

public enum Support
{
    Supported,
    Partial,
    Unsupported
}
